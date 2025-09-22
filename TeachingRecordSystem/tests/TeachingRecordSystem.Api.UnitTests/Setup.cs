using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting.Internal;
using TeachingRecordSystem.Api.Infrastructure.Security;
using TeachingRecordSystem.Core.Jobs.Scheduling;
using TeachingRecordSystem.Core.Services.Notify;
using TeachingRecordSystem.Core.Services.TrnGeneration;
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

        var environment = new HostingEnvironment { EnvironmentName = "Tests" };

        DbHelper.ConfigureDbServices(services, configuration.GetPostgresConnectionString());

        // Publish events synchronously
        PublishEventsDbCommandInterceptor.ConfigureServices(services);

        EvidenceFilesHttpClientHelper.ConfigureServices(services);

        services
            .AddSingleton<IConfiguration>(configuration)
            .AddCoreServices(configuration, environment)
            .AddApiServices(configuration, environment)
            .AddSingleton<DbFixture>()
            .AddFakeXrm()
            .AddSingleton(
                sp => ActivatorUtilities.CreateInstance<TestData>(
                    sp,
                    new ForwardToTestScopedClock(),
                    TestDataPersonDataSource.CrmAndTrs))
            .AddSingleton<FakeTrnGenerator>()
            .AddSingleton<ITrnGenerator>(sp => sp.GetRequiredService<FakeTrnGenerator>())
            .AddSingleton(Mock.Of<ICurrentUserProvider>())
            .AddSingleton<IEventObserver>(_ => new ForwardToTestScopedEventObserver())
            .AddSingleton<IBackgroundJobScheduler, ExecuteOnCommitBackgroundJobScheduler>()
            .AddSingleton<INotificationSender, NoopNotificationSender>();

        TestScopedServices.ConfigureServices(services);

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
