using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TeachingRecordSystem.Api.Infrastructure.Security;
using TeachingRecordSystem.Api.V3.Implementation.Operations;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Jobs.Scheduling;
using TeachingRecordSystem.Core.Services.Files;
using TeachingRecordSystem.Core.Services.GetAnIdentityApi;
using TeachingRecordSystem.Core.Services.NameSynonyms;
using TeachingRecordSystem.Core.Services.Notify;
using TeachingRecordSystem.Core.Services.PersonMatching;
using TeachingRecordSystem.Core.Services.TrnGeneration;
using TeachingRecordSystem.Core.Services.TrnRequests;
using TeachingRecordSystem.Core.Services.Webhooks;
using TeachingRecordSystem.TestCommon.Infrastructure;

namespace TeachingRecordSystem.Api.UnitTests;

public static class Setup
{
    private static readonly SemaphoreSlim _createdApplicationUserLock = new(1, 1);
    private static bool _applicationUserCreated;

    public static IServiceProvider Services { get; } = CreateServiceProvider();

    [Before(Assembly)]
    public static async Task AssemblySetup(AssemblyHookContext context)
    {
        await Services.GetRequiredService<DbHelper>().EnsureSchemaAsync();
    }

    [BeforeEvery(Test)]
    public static async Task TestSetup(TestContext context)
    {
        var testScopedServices = TestScopedServices.Reset(Services);
        await EnsureApplicationUser();
        testScopedServices.EventObserver.Clear();

        context.AddAsyncLocalValues();
    }

    private static IServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();

        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<SharedDependenciesDataSourceAttribute>()
            .AddJsonFile("appsettings.json")
            .AddEnvironmentVariables()
            .Build();

        var pgConnectionString = configuration.GetRequiredConnectionString("DefaultConnection");
        DbHelper.ConfigureDbServices(services, pgConnectionString);

        EvidenceFilesHttpClientHelper.ConfigureServices(services);

        services
            .AddSingleton<IConfiguration>(configuration)
            .AddTrsBaseServices(configuration)
            .AddSingleton<DbFixture>()
            .AddSingleton<TestData>(
                sp => ActivatorUtilities.CreateInstance<TestData>(
                    sp,
                    (IClock)new ForwardToTestScopedClock(),
                    TestDataPersonDataSource.CrmAndTrs))
            .AddApiCommands()
            .AddTestScoped<IClock>(tss => tss.Clock)
            .AddSingleton<FakeTrnGenerator>()
            .AddSingleton<ITrnGenerator>(sp => sp.GetRequiredService<FakeTrnGenerator>())
            .AddCrmQueries()
            .AddFakeXrm()
            .AddSingleton<ICurrentUserProvider>(Mock.Of<ICurrentUserProvider>())
            .AddNameSynonyms()
            .AddTestScoped<IGetAnIdentityApiClient>(tss => tss.GetAnIdentityApiClient.Object)
            .AddTestScoped<IFileService>(tss => tss.BlobStorageFileService.Object)
            .AddTestScoped<IFeatureProvider>(tss => tss.FeatureProvider)
            .AddEventObserver()
            .AddSingleton<IEventObserver>(_ => new ForwardToTestScopedEventObserver())
            .AddTestScoped<CaptureEventObserver>(tss => tss.EventObserver)
            .AddTransient<WebhookMessageFactory>()
            .AddSingleton<EventMapperRegistry>()
            .AddMemoryCache()
            .AddTransient<GetPersonHelper>()
            .AddPersonMatching()
            .AddTrnRequestService(configuration)
            .AddSingleton<IBackgroundJobScheduler, ExecuteOnCommitBackgroundJobScheduler>()
            .AddTestScoped<IOptions<TrnRequestOptions>>(tss => Options.Create(tss.TrnRequestOptions))
            .AddSingleton<INotificationSender, NoopNotificationSender>();

        return services.BuildServiceProvider();
    }

    private static async Task EnsureApplicationUser()
    {
        await _createdApplicationUserLock.WaitAsync();

        try
        {
            if (_applicationUserCreated)
            {
                return;
            }

            using var scope = Services.GetRequiredService<IServiceScopeFactory>().CreateScope();
            var applicationUser = await scope.ServiceProvider.GetRequiredService<TestData>().CreateApplicationUserAsync();

            Mock.Get(Services.GetRequiredService<ICurrentUserProvider>())
                .Setup(mock => mock.GetCurrentApplicationUser())
                .Returns((applicationUser.UserId, applicationUser.Name));

            _applicationUserCreated = true;
        }
        finally
        {
            _createdApplicationUserLock.Release();
        }
    }

    private class ForwardToTestScopedClock : IClock
    {
        public DateTime UtcNow => TestScopedServices.GetCurrent().Clock.UtcNow;
    }

    // IEventObserver needs to be a singleton but we want it to resolve to a test-scoped CaptureEventObserver.
    // This provides a wrapper that can be registered as a singleton that delegates to the test-scoped IEventObserver instance.
    private class ForwardToTestScopedEventObserver : IEventObserver
    {
        public void OnEventCreated(EventBase @event) => TestScopedServices.GetCurrent().EventObserver.OnEventCreated(@event);
    }
}

file static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTestScoped<T>(this IServiceCollection services, Func<TestScopedServices, T> resolveService)
        where T : class
    {
        return services.AddTransient<T>(_ => resolveService(TestScopedServices.GetCurrent()));
    }
}
