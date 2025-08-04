using System.Text;
using JustEat.HttpClientInterception;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Npgsql;
using TeachingRecordSystem.Api.Infrastructure.Security;
using TeachingRecordSystem.Api.V3.Implementation.Operations;
using TeachingRecordSystem.Core.DataStore.Postgres;
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

public class Startup
{
    public static readonly string EvidenceFileUrl = Faker.Internet.SecureUrl();
    public static readonly byte[] EvidenceFileContent = Encoding.UTF8.GetBytes("Test file");

    public static Guid GetAnIdentityApplicationUserId { get; } = new("873f0cb0-7174-4256-921a-e8a8aaa06361");

    public void ConfigureHost(IHostBuilder hostBuilder)
    {
        var settings = new Dictionary<string, string>
        {
            { "GetAnIdentityApplicationUserId", GetAnIdentityApplicationUserId.ToString() }
        };

        hostBuilder
            .ConfigureHostConfiguration(builder => builder
                .AddUserSecrets<Startup>(optional: true)
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .AddInMemoryCollection(settings!))
            .ConfigureServices((context, services) =>
            {
                var pgConnectionString = new NpgsqlConnectionStringBuilder(context.Configuration.GetRequiredConnectionString("DefaultConnection"))
                {
                    // We rely on error details to get the offending duplicate key values in the TrsDataSyncHelper
                    IncludeErrorDetail = true
                }.ConnectionString;

                DbHelper.ConfigureDbServices(services, pgConnectionString);

                // Publish events synchronously
                PublishEventsDbCommandInterceptor.ConfigureServices(services);

                services
                    .AddSingleton<DbFixture>()
                    .AddSingleton<OperationTestFixture>()
                    .AddTrsBaseServices()
                    .AddTestScoped<IClock>(tss => tss.Clock)
                    .AddSingleton<FakeTrnGenerator>()
                    .AddSingleton<ITrnGenerator>(sp => sp.GetRequiredService<FakeTrnGenerator>())
                    .AddCrmQueries()
                    .AddFakeXrm()
                    .Decorate<ICrmQueryDispatcher>(
                        inner => new CrmQueryDispatcherDecorator(
                            inner,
                            TestScopedServices.TryGetCurrent(out var tss) ? tss.CrmQueryDispatcherSpy : new()))
                    .AddSingleton<ICurrentUserProvider>(Mock.Of<ICurrentUserProvider>())
                    .AddNameSynonyms()
                    .AddTestScoped<IGetAnIdentityApiClient>(tss => tss.GetAnIdentityApiClient.Object)
                    .AddTestScoped<IFileService>(tss => tss.BlobStorageFileService.Object)
                    .AddTestScoped<IFeatureProvider>(tss => tss.FeatureProvider)
                    .AddSingleton<IEventObserver>(_ => new ForwardToTestScopedEventObserver())
                    .AddSingleton<WebhookMessageFactory>()
                    .AddSingleton<EventMapperRegistry>()
                    .AddMemoryCache()
                    .AddTransient<GetPersonHelper>()
                    .AddPersonMatching()
                    .AddTrnRequestService(context.Configuration)
                    .AddSingleton<IBackgroundJobScheduler, TestBackgroundJobScheduler>()
                    .AddTestScoped<IOptions<TrnRequestOptions>>(tss => Options.Create(tss.TrnRequestOptions))
                    .AddSingleton<INotificationSender, NoopNotificationSender>();

                // Intercept HTTP calls to download evidence files so we don't have to call a real website
                var options = new HttpClientInterceptorOptions();
                var builder = new HttpRequestInterceptionBuilder();

                var evidenceFileUri = new Uri(EvidenceFileUrl);

                builder
                    .Requests()
                    .ForGet()
                    .ForHttps()
                    .ForHost(evidenceFileUri.Host)
                    .ForPath(evidenceFileUri.LocalPath.TrimStart('/'))
                    .Responds()
                    .WithContentStream(() => new MemoryStream(EvidenceFileContent))
                    .RegisterWith(options);

                services
                    .AddHttpClient("EvidenceFiles")
                    .AddHttpMessageHandler(_ => options.CreateHttpMessageHandler())
                    .ConfigurePrimaryHttpMessageHandler(_ => new NotFoundHandler());

                services.AddStartupTask(async sp =>
                {
                    await using var dbContext = await sp.GetRequiredService<IDbContextFactory<TrsDbContext>>().CreateDbContextAsync();

                    dbContext.ApplicationUsers.Add(new Core.DataStore.Postgres.Models.ApplicationUser()
                    {
                        UserId = GetAnIdentityApplicationUserId,
                        Name = "Get an identity",
                        ApiRoles = [ApiRoles.UpdatePerson]
                    });

                    await dbContext.SaveChangesAsync();
                });
            });
    }

    private class NotFoundHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.NotFound));
        }
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
