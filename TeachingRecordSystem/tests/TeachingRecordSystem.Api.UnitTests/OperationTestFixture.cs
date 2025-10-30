using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting.Internal;
using TeachingRecordSystem.Api.Infrastructure.Security;
using TeachingRecordSystem.Api.UnitTests;
using TeachingRecordSystem.Core.Services.Notify;
using TeachingRecordSystem.Core.Services.TrnGeneration;
using TeachingRecordSystem.TestCommon.Infrastructure;

[assembly: AssemblyFixture(typeof(OperationTestFixture))]

namespace TeachingRecordSystem.Api.UnitTests;

public class OperationTestFixture : ServiceProviderFixture
{
    private readonly Mock<ICurrentUserProvider> _currentUserProviderMock = new();

    protected override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        var environment = new HostingEnvironment() { EnvironmentName = "Tests" };

        services
            .AddCoreServices(configuration, environment)
            .AddApiServices(configuration, environment)
            .AddSingleton<TestData>()
            .AddSingleton<FakeTrnGenerator>()
            .AddSingleton<ITrnGenerator>(sp => sp.GetRequiredService<FakeTrnGenerator>())
            .AddSingleton(_currentUserProviderMock.Object)
            .AddSingleton<INotificationSender, NoopNotificationSender>();

        // Publish events synchronously
        PublishEventsDbCommandInterceptor.ConfigureServices(services);

        EvidenceFilesHttpClientHelper.ConfigureServices(services);

        TestScopedServices.ConfigureServices(services);
    }

    public override async ValueTask InitializeAsync()
    {
        await base.InitializeAsync();

        TestScopedServices.Reset(Services);
        var testData = Services.GetRequiredService<TestData>();
        var applicationUser = await testData.CreateApplicationUserAsync();
        _currentUserProviderMock
            .Setup(mock => mock.GetCurrentApplicationUser())
            .Returns((applicationUser.UserId, applicationUser.Name));
    }
}
