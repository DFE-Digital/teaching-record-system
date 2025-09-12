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

    private static IServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();

        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<SharedDependenciesDataSourceAttribute>()
            .AddEnvironmentVariables()
            .Build();

        var pgConnectionString = configuration.GetRequiredConnectionString("DefaultConnection");
        DbHelper.ConfigureDbServices(services, pgConnectionString);

        services
            .AddSingleton<IConfiguration>(configuration)
            .AddSingleton<DbFixture>();

        return services.BuildServiceProvider();
    }
}
