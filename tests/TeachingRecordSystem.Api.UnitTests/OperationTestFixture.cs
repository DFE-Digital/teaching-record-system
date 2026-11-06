using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting.Internal;
using TeachingRecordSystem.Api.Infrastructure.Security;
using TeachingRecordSystem.Api.UnitTests;
using TeachingRecordSystem.Core.Services.Notify;
using TeachingRecordSystem.TestCommon.Infrastructure;

[assembly: AssemblyFixture(typeof(OperationTestFixture))]

namespace TeachingRecordSystem.Api.UnitTests;

public class OperationTestFixture : ServiceProviderFixture, IAsyncLifetime
{
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

#pragma warning disable CA1816
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
#pragma warning restore CA1816

    public async ValueTask InitializeAsync()
    {
        // The template's seeded rows include some that these tests need gone
        await DbHelper.ClearDataAsync();

        TestScopedServices.Reset(Services);
        var testData = Services.GetRequiredService<TestData>();
        var applicationUser = await testData.CreateApplicationUserAsync();
        _currentUserProviderMock
            .Setup(mock => mock.GetCurrentApplicationUserId())
            .Returns(applicationUser.UserId);
    }
}
