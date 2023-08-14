using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Events.Processing;
using TeachingRecordSystem.SupportUi.Tests.Infrastructure;
using TeachingRecordSystem.SupportUi.Tests.Infrastructure.Security;
using TeachingRecordSystem.TestCommon.Infrastructure;

namespace TeachingRecordSystem.SupportUi.Tests;

public class HostFixture : WebApplicationFactory<Program>
{
    private readonly IConfiguration _configuration;

    public HostFixture(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public CaptureEventObserver EventObserver => (CaptureEventObserver)Services.GetRequiredService<IEventObserver>();

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
            services.AddSingleton<PublishEventsDbCommandInterceptor>();
            services.Decorate<DbContextOptions<TrsDbContext>>((inner, sp) =>
            {
                var coreOptionsExtension = inner.GetExtension<CoreOptionsExtension>();

                return (DbContextOptions<TrsDbContext>)inner.WithExtension(
                    coreOptionsExtension.WithInterceptors(new IInterceptor[]
                    {
                        sp.GetRequiredService<PublishEventsDbCommandInterceptor>(),
                    }));
            });

            services.AddSingleton<CurrentUserProvider>();
            services.AddTransient<TestUsers.CreateUsersStartupTask>();
            services.AddStartupTask<TestUsers.CreateUsersStartupTask>();

            services.AddSingleton<IEventObserver, CaptureEventObserver>();
            services.AddTestScoped<IClock>(tss => tss.Clock);
            services.AddTestScoped<IDataverseAdapter>(tss => tss.DataverseAdapterMock.Object);
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
}

file static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTestScoped<T>(this IServiceCollection services, Func<TestScopedServices, T> resolveService)
        where T : class
    {
        return services.AddTransient<T>(_ => resolveService(TestScopedServices.GetCurrent()));
    }
}
