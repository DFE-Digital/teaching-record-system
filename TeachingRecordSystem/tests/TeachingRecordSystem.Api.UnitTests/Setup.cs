using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting.Internal;
using TeachingRecordSystem.Api.Infrastructure.Security;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Services.Notify;
using TeachingRecordSystem.Core.Services.TrnGeneration;
using TeachingRecordSystem.TestCommon.Infrastructure;

namespace TeachingRecordSystem.Api.UnitTests;

public static class Setup
{
    private static readonly Mock<ICurrentUserProvider> _currentUserProviderMock = new();

    public static IServiceProvider Services { get; } = CreateServiceProvider();

    [Before(Assembly)]
    public static async Task AssemblySetup()
    {
        await Services.GetRequiredService<DbHelper>().InitializeAsync();

        var applicationUser = await CreateApplicationUser();
        _currentUserProviderMock
            .Setup(mock => mock.GetCurrentApplicationUser())
            .Returns((applicationUser.UserId, applicationUser.Name));
    }

    private static IServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();

        var configuration = TestConfiguration.GetConfiguration();

        var environment = new HostingEnvironment { EnvironmentName = "Tests" };

        services
            .AddSingleton<IConfiguration>(configuration)
            .AddSingleton(DbHelper.Instance)
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

        return services.BuildServiceProvider();
    }

    private static async Task<ApplicationUser> CreateApplicationUser()
    {
        // HACK until TestData is easier to construct
        using var scope = Services.GetRequiredService<IServiceScopeFactory>().CreateScope();
        TestScopedServices.Reset(scope.ServiceProvider);
        return await scope.ServiceProvider.GetRequiredService<TestData>().CreateApplicationUserAsync();
    }
}
