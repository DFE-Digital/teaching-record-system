namespace TeachingRecordSystem.SupportUi.EndToEndTests;

public static class Setup
{
    public static IServiceProvider Services { get; } = CreateServiceProvider();

    [Before(Assembly)]
    public static async Task AssemblySetup(AssemblyHookContext context)
    {
        await Services.GetRequiredService<HostFixture>().InitializeAsync();
    }

    private static IServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();

        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<SharedDependenciesDataSourceAttribute>()
            .AddEnvironmentVariables()
            .Build();

        services
            .AddSingleton<IConfiguration>(configuration)
            .AddSingleton<HostFixture>();

        return services.BuildServiceProvider();
    }
}
