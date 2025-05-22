using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Jobs.Scheduling;
using TeachingRecordSystem.Core.Services.Files;
using TeachingRecordSystem.Core.Services.GetAnIdentityApi;
using TeachingRecordSystem.Core.Services.TrnGeneration;
using TeachingRecordSystem.Core.Services.TrsDataSync;
using TeachingRecordSystem.SupportUi.Services.AzureActiveDirectory;
using TeachingRecordSystem.SupportUi.Tests.Infrastructure.Security;
using TeachingRecordSystem.TestCommon.Infrastructure;
using TeachingRecordSystem.UiTestCommon.Infrastructure.FormFlow;
using TeachingRecordSystem.WebCommon.FormFlow.State;

namespace TeachingRecordSystem.SupportUi.Tests;

public class HostFixture : WebApplicationFactory<Program>
{
    private readonly IConfiguration _configuration;

    public HostFixture(IConfiguration configuration)
    {
        _configuration = configuration;
        _ = base.Services;  // Start the host
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        // N.B. Don't use builder.ConfigureAppConfiguration here since it runs *after* the entry point
        // i.e. Program.cs and that has a dependency on IConfiguration
        builder.UseConfiguration(_configuration);

        builder.ConfigureServices((context, services) =>
        {
            DbHelper.ConfigureDbServices(services, context.Configuration.GetRequiredConnectionString("DefaultConnection"));

            services.AddAuthentication()
                .AddScheme<TestAuthenticationOptions, TestAuthenticationHandler>("Test", options => { });

            // Remove the built-in antiforgery filters
            // (we want to be able to POST directly from a test without having to set antiforgery cookies etc.)
            services.AddSingleton<IPageApplicationModelProvider, RemoveAutoValidateAntiforgeryPageApplicationModelProvider>();

            // Publish events synchronously
            PublishEventsDbCommandInterceptor.ConfigureServices(services);

            services.AddSingleton<CurrentUserProvider>();
            services.AddSingleton<TestUsers>();
            services.AddSingleton<IEventObserver>(_ => new ForwardToTestScopedEventObserver());
            services.AddTestScoped<IClock>(tss => tss.Clock);
            services.AddTestScoped<IDataverseAdapter>(tss => tss.DataverseAdapterMock.Object);
            services.AddTestScoped<IGetAnIdentityApiClient>(tss => tss.GetAnIdentityApiClientMock.Object);
            services.AddTestScoped<IAadUserService>(tss => tss.AzureActiveDirectoryUserServiceMock.Object);
            services.AddTestScoped<IFeatureProvider>(tss => tss.FeatureProvider);
            services.AddSingleton<TestData>(
                sp => ActivatorUtilities.CreateInstance<TestData>(
                    sp,
                    (IClock)new ForwardToTestScopedClock(),
                    TestDataPersonDataSource.Trs));
            services.AddFakeXrm();
            services.AddSingleton<IUserInstanceStateProvider, InMemoryInstanceStateProvider>();
            services.AddSingleton<FakeTrnGenerator>();
            services.AddSingleton<ITrnGenerator>(sp => sp.GetRequiredService<FakeTrnGenerator>());
            services.AddSingleton<TrsDataSyncHelper>();
            services.AddSingleton<IAuditRepository, TestableAuditRepository>();
            services.AddTestScoped<IFileService>(tss => tss.BlobStorageFileServiceMock.Object);
            services.RemoveAll<ReferenceDataCache>();
            services.AddSingleton<ReferenceDataCache, TestReferenceDataCache>();
            services.AddSingleton<IBackgroundJobScheduler, TestBackgroundJobScheduler>();
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        // Ensure we can flow AsyncLocals from tests to the server
        builder.ConfigureServices(services => services.Configure<TestServerOptions>(o => o.PreserveExecutionContext = true));

        return base.CreateHost(builder);
    }

    private class RemoveAutoValidateAntiforgeryPageApplicationModelProvider : IPageApplicationModelProvider
    {
        public int Order => int.MaxValue;

        public void OnProvidersExecuted(PageApplicationModelProviderContext context)
        {
        }

        public void OnProvidersExecuting(PageApplicationModelProviderContext context)
        {
            var pageApplicationModel = context.PageApplicationModel;

            var autoValidateAttribute = pageApplicationModel.Filters.OfType<AutoValidateAntiforgeryTokenAttribute>().SingleOrDefault();
            if (autoValidateAttribute is not null)
            {
                pageApplicationModel.Filters.Remove(autoValidateAttribute);
            }
        }
    }

    // IEventObserver needs to be a singleton but we want it to resolve to a test-scoped CaptureEventObserver.
    // This provides a wrapper that can be registered as a singleton that delegates to the test-scoped IEventObserver instance.
    private class ForwardToTestScopedEventObserver : IEventObserver
    {
        public void OnEventCreated(EventBase @event) => TestScopedServices.GetCurrent().EventObserver.OnEventCreated(@event);
    }

    private class ForwardToTestScopedClock : IClock
    {
        public DateTime UtcNow => TestScopedServices.GetCurrent().Clock.UtcNow;
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
