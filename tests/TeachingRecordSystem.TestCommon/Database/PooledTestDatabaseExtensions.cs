using Microsoft.Extensions.DependencyInjection;
using TeachingRecordSystem.Core.DataStore.Postgres;

namespace TeachingRecordSystem.TestCommon.Database;

public static class PooledTestDatabaseExtensions
{
    // Points a test host at whichever pooled database the running test owns.
    //
    // Only the connection is swapped: DbContextOptions<TrsDbContext> still comes from DI so that anything
    // which has decorated it - PublishEventsDbCommandInterceptor, in particular - stays attached. Registering
    // an ambient NpgsqlDataSource instead does not work, because the container disposes the services it hands
    // out and a closing request scope would dispose a data source the pool still owns.
    public static IServiceCollection AddPooledTestDatabase(this IServiceCollection services)
    {
        services.AddScoped(sp => CreateForCurrentTest(sp.GetRequiredService<DbContextOptions<TrsDbContext>>()));

        services.AddSingleton<IDbContextFactory<TrsDbContext>>(
            sp => new PooledTestDbContextFactory(sp.GetRequiredService<DbContextOptions<TrsDbContext>>()));

        services.AddScoped(sp => PooledReferenceDataCaches.ForCurrentDatabase(
            sp.GetRequiredService<IDbContextFactory<TrsDbContext>>()));

        return services;
    }

    private static TrsDbContext CreateForCurrentTest(DbContextOptions<TrsDbContext> options)
    {
        var dbContext = new TrsDbContext(options);
        dbContext.Database.SetDbConnection(TestDatabaseScope.CurrentDataSource.CreateConnection(), contextOwnsConnection: true);
        return dbContext;
    }

    // For fixtures that hold a run-scoped lease and need a factory bound to it rather than to the
    // ambient per-test scope.
    public static IDbContextFactory<TrsDbContext> CreateDbContextFactory(TestDatabaseLease lease) =>
        new LeasedDbContextFactory(lease);

    private class LeasedDbContextFactory(TestDatabaseLease lease) : IDbContextFactory<TrsDbContext>
    {
        public TrsDbContext CreateDbContext() => TrsDbContext.Create(lease.DataSource);
    }

    private class PooledTestDbContextFactory(DbContextOptions<TrsDbContext> options) : IDbContextFactory<TrsDbContext>
    {
        public TrsDbContext CreateDbContext() => CreateForCurrentTest(options);
    }
}
