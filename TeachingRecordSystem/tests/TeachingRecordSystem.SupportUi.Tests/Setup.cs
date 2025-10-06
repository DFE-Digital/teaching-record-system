using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

[assembly: RetryOnTransientError(3), ParallelLimiter<LimitToDbPoolSizeParallelLimit>]

namespace TeachingRecordSystem.SupportUi.Tests;

public static class Setup
{
    public static IServiceProvider Services { get; } = CreateServiceProvider();

    public static User AdminUser { get; private set; } = null!;

    [Before(Assembly)]
    public static async Task AssemblySetup()
    {
        await Services.GetRequiredService<DbHelper>().InitializeAsync();

        var hostFixture = Services.GetRequiredService<HostFixture>();
        var addTestRoutes = ActivatorUtilities.CreateInstance<AddTestRouteTypesStartupTask>(
            Services,
            hostFixture.Services.GetRequiredService<ReferenceDataCache>());
        await addTestRoutes.ExecuteAsync();

        AdminUser = await CreateAdminUserAsync();
    }

    private static async Task<User> CreateAdminUserAsync()
    {
        await using var dbContext = await Services.GetRequiredService<IDbContextFactory<TrsDbContext>>().CreateDbContextAsync();

        var user = new User
        {
            Active = true,
            Name = "Test admin user",
            Email = "test.admin@example.org",
            Role = UserRoles.Administrator,
            UserId = Guid.NewGuid(),
            AzureAdUserId = null
        };

        dbContext.Users.Add(user);

        await dbContext.SaveChangesAsync();

        return user;
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
