using GovUk.Questions.AspNetCore.Testing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using TeachingRecordSystem.Core.Services.Notify;
using TeachingRecordSystem.Core.Services.OneLogin;
using TeachingRecordSystem.Core.Services.SupportTasks;
using TeachingRecordSystem.SupportUi.Tests;
using TeachingRecordSystem.SupportUi.Tests.Infrastructure.Security;
using TeachingRecordSystem.TestCommon.Database;
using TeachingRecordSystem.TestCommon.Infrastructure;
using User = TeachingRecordSystem.Core.DataStore.Postgres.Models.User;

[assembly: AssemblyFixture(typeof(HostFixture))]

namespace TeachingRecordSystem.SupportUi.Tests;

public class HostFixture : IAsyncLifetime
{
    private readonly SupportUiApplicationFactory _webApplicationFactory;

    public HostFixture()
    {
        _webApplicationFactory = new SupportUiApplicationFactory();
    }

    // Seeded into the template rather than created at host startup, so it exists in every test's database.
    // The id is fixed so this instance matches the seeded row.
    public static User AdminUser { get; } = new()
    {
        UserId = new Guid("8f0f0b47-3f0e-4a6d-9a1e-2f9b6a2a1c10"),
        Active = true,
        Name = "Test admin user",
        Email = "test.admin@example.org",
        Role = UserRoles.Administrator,
        AzureAdUserId = null
    };

    // Resolves through the running test's scope when there is one, so scoped services aren't cached in the
    // root provider and shared across tests. Falls back to the root provider outside of a test (host start-up).
    public IServiceProvider Services => TestServiceScope.Current ?? _webApplicationFactory.Services;

    public IServiceProvider RootServices => _webApplicationFactory.Services;

    public HttpClient CreateClient() => _webApplicationFactory.CreateClient();

    public HttpClient CreateClient(WebApplicationFactoryClientOptions options) => _webApplicationFactory.CreateClient(options);

    public async ValueTask InitializeAsync()
    {
        TestDatabases.AddTemplateSeed("supportui-admin-user-v1", async dbContext =>
        {
            dbContext.Users.Add(AdminUser);
            await dbContext.SaveChangesAsync();
        });

        TestDatabases.AddTemplateSeed("supportui-test-route-types-v1", AddTestRouteTypes.SeedAsync);

        await TestDatabases.InitializeAsync();

        _ = RootServices;  // Start the host
    }

    public async ValueTask DisposeAsync() => await TestDatabases.DisposeAsync();

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
                // Add controllers defined in this test assembly
                services.AddMvc().AddApplicationPart(typeof(HostFixture).Assembly);

                services.AddAuthentication()
                    .AddScheme<TestAuthenticationOptions, TestAuthenticationHandler>("Test", options => { });

                // Remove the built-in antiforgery filters
                // (we want to be able to POST directly from a test without having to set antiforgery cookies etc.)
                services.AddSingleton<IPageApplicationModelProvider, RemoveAutoValidateAntiforgeryPageApplicationModelProvider>();

                // Publish events synchronously
                PublishEventsDbCommandInterceptor.ConfigureServices(services);

                services
                    .AddSingleton<CurrentUserProvider>()
                    .AddSingleton<TestData>()
                    .AddSingleton<INotificationSender, NoopNotificationSender>()
                    .AddSingleton<IStartupFilter, ExecuteScheduledJobsStartupFilter>()
                    .AddOneLoginService()
                    .AddSupportTaskServices()
                    .AddGovUkQuestionsTestingServices();

                TestScopedServices.ConfigureServices(services);

                // Route every TrsDbContext at the database leased by the running test.
                services.AddPooledTestDatabase();
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
