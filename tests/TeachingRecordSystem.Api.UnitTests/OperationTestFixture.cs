using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting.Internal;
using TeachingRecordSystem.Api.Infrastructure.Security;
using TeachingRecordSystem.Api.UnitTests;
using TeachingRecordSystem.Core.Services.Notify;
using TeachingRecordSystem.TestCommon.Database;
using TeachingRecordSystem.TestCommon.Infrastructure;

[assembly: AssemblyFixture(typeof(OperationTestFixture))]

namespace TeachingRecordSystem.Api.UnitTests;

public class OperationTestFixture : ServiceProviderFixture
{
    protected override bool UsePooledDatabase => true;

    private readonly Mock<ICurrentUserProvider> _currentUserProviderMock = new();

    protected override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        var environment = new HostingEnvironment() { EnvironmentName = "Tests" };

        services
            .AddCoreServices(configuration, environment)
            .AddApiServices(configuration, environment)
            .AddSingleton<TestData>()
            .AddSingleton(_currentUserProviderMock.Object)
            .AddSingleton<INotificationSender, NoopNotificationSender>();

        // Publish events synchronously
        PublishEventsDbCommandInterceptor.ConfigureServices(services);

        EvidenceFilesHttpClientHelper.ConfigureServices(services);

        TestScopedServices.ConfigureServices(services);
    }

    // Seeded into the template rather than created here: fixture initialisation runs outside any test, so
    // there is no leased database to write to.
    private static readonly Guid _applicationUserId = new("3f2b8a6c-9d41-4e77-b0a5-6c1e8f24d930");

    public override async ValueTask InitializeAsync()
    {
        TestDatabases.AddTemplateSeed("api-unittests-application-user-v1", async dbContext =>
        {
            dbContext.ApplicationUsers.Add(new Core.DataStore.Postgres.Models.ApplicationUser
            {
                UserId = _applicationUserId,
                Name = "Tests",
                ShortName = "tests",
                ApiRoles = []
            });

            await dbContext.SaveChangesAsync();
        });

        await base.InitializeAsync();

        _currentUserProviderMock
            .Setup(mock => mock.GetCurrentApplicationUserId())
            .Returns(_applicationUserId);
    }
}
