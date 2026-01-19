using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Services.Notify;
using TeachingRecordSystem.SupportUi.Tests;
using TeachingRecordSystem.SupportUi.Tests.Infrastructure.Security;
using TeachingRecordSystem.TestCommon.Infrastructure;
using TeachingRecordSystem.UiTestCommon.Infrastructure.FormFlow;
using TeachingRecordSystem.WebCommon.FormFlow.State;

[assembly: AssemblyFixture(typeof(HostFixture))]

namespace TeachingRecordSystem.SupportUi.Tests;

public class HostFixture : InitializeDbFixture
{
    private readonly SupportUiApplicationFactory _webApplicationFactory;

    public HostFixture()
    {
        _webApplicationFactory = new SupportUiApplicationFactory();
    }

    public static User AdminUser { get; private set; } = null!;

    public IServiceProvider Services => _webApplicationFactory.Services;

    public HttpClient CreateClient() => _webApplicationFactory.CreateClient();

    public HttpClient CreateClient(WebApplicationFactoryClientOptions options) => _webApplicationFactory.CreateClient(options);

    public override async ValueTask InitializeAsync()
    {
        await InitializeDbAsync();

        _ = Services;  // Start the host

        AdminUser = await CreateUser();

        async Task<User> CreateUser()
        {
            await using var dbContext = await Services.GetRequiredService<IDbContextFactory<TrsDbContext>>().CreateDbContextAsync();

            var user = new User
            {
                Active = true,
                Name = "Test admin user",
                Email = "test.admin@example.org",
                Role = UserRoles.Administrator,
                UserId = Guid.NewGuid(),
                AzureAdUserId = null
            };

            dbContext.Users.Add(user);

            await dbContext.SaveChangesAsync();

            return user;
        }
    }

    private class SupportUiApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Tests");

            // N.B. Don't use builder.ConfigureAppConfiguration here since it runs *after* the entry point
            // i.e. Program.cs and that has a dependency on IConfiguration
            var configuration = TestConfiguration.GetConfiguration();
            builder.UseConfiguration(configuration);

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
                    .AddSingleton<TestData>()
                    .AddSingleton<IUserInstanceStateProvider, InMemoryInstanceStateProvider>()
                    .AddSingleton<INotificationSender, NoopNotificationSender>()
                    .AddSingleton<IStartupFilter, ExecuteScheduledJobsStartupFilter>()
                    .AddStartupTask<AddTestRouteTypesStartupTask>();

                TestScopedServices.ConfigureServices(services);
            });
        }

        protected override TestServer CreateServer(IServiceProvider serviceProvider)
        {
            var server = base.CreateServer(serviceProvider);
            // Ensure we can flow AsyncLocals from tests to the server
            server.PreserveExecutionContext = true;
            return server;
        }
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

    private class ExecuteScheduledJobsStartupFilter : IStartupFilter
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
