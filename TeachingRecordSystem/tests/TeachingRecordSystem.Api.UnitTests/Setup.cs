using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting.Internal;
using TeachingRecordSystem.Api.Infrastructure.Security;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Events.Legacy;
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

        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<SharedDependenciesDataSourceAttribute>()
            .AddJsonFile("appsettings.json")
            .AddEnvironmentVariables()
            .Build();

        var environment = new HostingEnvironment { EnvironmentName = "Tests" };

        DbHelper.ConfigureDbServices(services, configuration.GetPostgresConnectionString());

        // Publish events synchronously
        PublishEventsDbCommandInterceptor.ConfigureServices(services);

        EvidenceFilesHttpClientHelper.ConfigureServices(services);

        services
            .AddSingleton<IConfiguration>(configuration)
            .AddCoreServices(configuration, environment)
            .AddApiServices(configuration, environment)
            .AddSingleton<DbFixture>()
            .AddSingleton(
                sp => ActivatorUtilities.CreateInstance<TestData>(
                    sp,
                    new ForwardToTestScopedClock()))
            .AddSingleton<FakeTrnGenerator>()
            .AddSingleton<ITrnGenerator>(sp => sp.GetRequiredService<FakeTrnGenerator>())
            .AddSingleton(_currentUserProviderMock.Object)
            .AddSingleton<IEventObserver>(_ => new ForwardToTestScopedEventObserver())
            .AddSingleton<INotificationSender, NoopNotificationSender>();

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

    private class ForwardToTestScopedClock : IClock
    {
        public DateTime UtcNow => TestScopedServices.GetCurrent().Clock.UtcNow;
    }

    // IEventObserver needs to be a singleton but we want it to resolve to a test-scoped CaptureEventObserver.
    // This provides a wrapper that can be registered as a singleton that delegates to the test-scoped IEventObserver instance.
    private class ForwardToTestScopedEventObserver : IEventObserver
    {
        public void OnEventCreated(EventBase @event) => TestScopedServices.GetCurrent().EventObserver.OnEventCreated(@event);
    }
}
