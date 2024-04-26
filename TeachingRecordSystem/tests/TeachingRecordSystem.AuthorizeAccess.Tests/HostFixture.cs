using GovUk.OneLogin.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using TeachingRecordSystem.AuthorizeAccess.Tests.Infrastructure.Security;
using TeachingRecordSystem.Core.Events.Processing;
using TeachingRecordSystem.Core.Services.TrsDataSync;
using TeachingRecordSystem.FormFlow.State;
using TeachingRecordSystem.TestCommon.Infrastructure;
using TeachingRecordSystem.UiTestCommon.Infrastructure.FormFlow;

namespace TeachingRecordSystem.AuthorizeAccess.Tests;

public class HostFixture : WebApplicationFactory<Program>
{
    private readonly IConfiguration _configuration;
    private readonly FakeTrnGenerator _fakeTrnGenerator;

    public HostFixture(IConfiguration configuration, FakeTrnGenerator fakeTrnGenerator)
    {
        _configuration = configuration;
        _fakeTrnGenerator = fakeTrnGenerator;
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
            services.AddDbContext<IdDbContext>(options => options.UseInMemoryDatabase("TeacherAuthId"), contextLifetime: ServiceLifetime.Transient);

            services
                .Configure<AuthenticationOptions>(options =>
                {
                    options.AddScheme(OneLoginDefaults.AuthenticationScheme, b => b.HandlerType = typeof(DummyOneLoginHandler));
                })
                .AddSingleton<DummyOneLoginHandler>();

            // Remove the built-in antiforgery filters
            // (we want to be able to POST directly from a test without having to set antiforgery cookies etc.)
            services.AddSingleton<IPageApplicationModelProvider, RemoveAutoValidateAntiforgeryPageApplicationModelProvider>();

            // Publish events synchronously
            PublishEventsDbCommandInterceptor.ConfigureServices(services);

            services.AddSingleton<IEventObserver>(_ => new ForwardToTestScopedEventObserver());
            services.AddTestScoped<IClock>(tss => tss.Clock);
            services.AddSingleton<TestData>(
                sp => ActivatorUtilities.CreateInstance<TestData>(
                    sp,
                    (IClock)new ForwardToTestScopedClock(),
                    TestDataSyncConfiguration.Sync(sp.GetRequiredService<TrsDataSyncHelper>())));
            services.AddFakeXrm();
            services.AddSingleton<IUserInstanceStateProvider, InMemoryInstanceStateProvider>();
            services.AddSingleton(_fakeTrnGenerator);
            services.AddSingleton<TrsDataSyncHelper>();
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
    // This provides a wrapper that can be registered as a singleon that delegates to the test-scoped IEventObserver instance.
    private class ForwardToTestScopedEventObserver : IEventObserver
    {
        public Task OnEventSaved(EventBase @event) => TestScopedServices.GetCurrent().EventObserver.OnEventSaved(@event);
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
