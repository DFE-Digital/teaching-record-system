using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using TeachingRecordSystem.Core.Events.Legacy;
using TeachingRecordSystem.Core.Services.Notify;
using TeachingRecordSystem.Core.Services.TrnGeneration;
using TeachingRecordSystem.SupportUi.Tests.Infrastructure.Security;
using TeachingRecordSystem.TestCommon.Infrastructure;
using TeachingRecordSystem.UiTestCommon.Infrastructure.FormFlow;
using TeachingRecordSystem.WebCommon.FormFlow.State;

namespace TeachingRecordSystem.SupportUi.Tests;

[SharedDependenciesDataSource]
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
        builder.UseEnvironment("Tests");

        // N.B. Don't use builder.ConfigureAppConfiguration here since it runs *after* the entry point
        // i.e. Program.cs and that has a dependency on IConfiguration
        builder.UseConfiguration(_configuration);

        builder.ConfigureServices((context, services) =>
        {
            services.AddAuthentication()
                .AddScheme<TestAuthenticationOptions, TestAuthenticationHandler>("Test", options => { });

            // Remove the built-in antiforgery filters
            // (we want to be able to POST directly from a test without having to set antiforgery cookies etc.)
            services.AddSingleton<IPageApplicationModelProvider, RemoveAutoValidateAntiforgeryPageApplicationModelProvider>();

            // Publish events synchronously
            PublishEventsDbCommandInterceptor.ConfigureServices(services);

            services
                .AddSingleton(DbHelper.Instance)
                .AddSingleton<CurrentUserProvider>()
                .AddSingleton<IEventObserver>(_ => new ForwardToTestScopedEventObserver())
                .AddSingleton<TestData>()
                .AddSingleton<FakeTrnGenerator>()
                .AddSingleton<ITrnGenerator>(sp => sp.GetRequiredService<FakeTrnGenerator>())
                .AddSingleton<IUserInstanceStateProvider, InMemoryInstanceStateProvider>()
                .AddSingleton<INotificationSender, NoopNotificationSender>()
                .AddSingleton<IStartupFilter, ExecuteScheduledJobsStartupFilter>();

            TestScopedServices.ConfigureServices(services);
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

    public class ExecuteScheduledJobsStartupFilter : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) =>
            app =>
            {
                app.Use(async (_, next) =>
                {
                    await next();

                    await TestScopedServices.GetCurrent().BackgroundJobScheduler.ExecuteDeferredJobsAsync();
                });

                next(app);
            };
    }
}
