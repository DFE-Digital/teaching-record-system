using TeachingRecordSystem.SupportUi.EndToEndTests;

[assembly: RetryOnCI(3)]

namespace TeachingRecordSystem.SupportUi.EndToEndTests;

public static class Setup
{
    public static IServiceProvider Services { get; } = CreateServiceProvider();

    [Before(Assembly)]
    public static async Task AssemblySetup(AssemblyHookContext context)
    {
        await Services.GetRequiredService<DbHelper>().InitializeAsync();

        await Services.GetRequiredService<HostFixture>().InitializeAsync();
    }

    private static IServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();

        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<SharedDependenciesDataSourceAttribute>()
            .AddEnvironmentVariables()
            .Build();

        DbHelper.ConfigureDbServices(services, configuration.GetPostgresConnectionString());

        services
            .AddSingleton<IConfiguration>(configuration)
            .AddSingleton<HostFixture>();

        return services.BuildServiceProvider();
    }
}
