using Microsoft.Extensions.DependencyInjection;
using TeachingRecordSystem.TestCommon;

namespace TeachingRecordSystem.Cli.Tests;

public static class Setup
{
    public static IServiceProvider Services { get; } = CreateServiceProvider();

    [Before(Assembly)]
    public static async Task AssemblySetup(AssemblyHookContext context)
    {
        await Services.GetRequiredService<DbHelper>().EnsureSchemaAsync();
    }

    private static ServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();

        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<SharedDependenciesDataSourceAttribute>()
            .AddEnvironmentVariables()
            .Build();

        DbHelper.ConfigureDbServices(services, configuration.GetPostgresConnectionString());

        services
            .AddSingleton<IConfiguration>(configuration)
            .AddSingleton<DbFixture>();

        return services.BuildServiceProvider();
    }
}
