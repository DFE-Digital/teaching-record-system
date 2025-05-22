using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;
using TeachingRecordSystem.Api.Infrastructure.Security;
using TeachingRecordSystem.Api.V3.Implementation.Operations;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Jobs.Scheduling;
using TeachingRecordSystem.Core.Services.GetAnIdentityApi;
using TeachingRecordSystem.Core.Services.NameSynonyms;
using TeachingRecordSystem.Core.Services.PersonMatching;
using TeachingRecordSystem.Core.Services.TrnGeneration;
using TeachingRecordSystem.Core.Services.TrnRequests;
using TeachingRecordSystem.Core.Services.Webhooks;
using TeachingRecordSystem.TestCommon.Infrastructure;

namespace TeachingRecordSystem.Api.UnitTests;

public class Startup
{
    public void ConfigureHost(IHostBuilder hostBuilder)
    {
        hostBuilder
            .ConfigureHostConfiguration(builder => builder
                .AddUserSecrets<Startup>(optional: true)
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables())
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
                    .AddTestScoped<IFeatureProvider>(tss => tss.FeatureProvider)
                    .AddSingleton<IEventObserver>(_ => new ForwardToTestScopedEventObserver())
                    .AddSingleton<WebhookMessageFactory>()
                    .AddSingleton<EventMapperRegistry>()
                    .AddMemoryCache()
                    .AddTransient<GetPersonHelper>()
                    .AddPersonMatching()
                    .AddTrnRequestService(context.Configuration)
                    .AddSingleton<IBackgroundJobScheduler, TestBackgroundJobScheduler>();
            });
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
