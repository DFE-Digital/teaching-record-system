using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.TestCommon;
using TeachingRecordSystem.TestCommon.Database;

namespace TeachingRecordSystem.Cli.Tests.CommandTests;

public abstract class CommandTestBase(IServiceProvider services) : PooledDatabaseTestBase
{
    protected FakeTimeProvider TimeProvider => (FakeTimeProvider)services.GetRequiredService<TimeProvider>();

    protected DbHelper DbHelper => services.GetRequiredService<DbHelper>();

    // The CLI commands build their own host from configuration, so they can't see the ambient data source.
    // Overlaying the leased database's connection string points them at the same database as the test.
    protected IConfiguration Configuration => new ConfigurationBuilder()
        .AddConfiguration(services.GetRequiredService<IConfiguration>())
        .AddInMemoryCollection([
            KeyValuePair.Create($"ConnectionStrings:{TrsDbContext.ConnectionName}", (string?)DatabaseConnectionString)
        ])
        .Build();

    protected IDbContextFactory<TrsDbContext> DbContextFactory => services.GetRequiredService<IDbContextFactory<TrsDbContext>>();

    protected TestData TestData => services.GetRequiredService<TestData>();

    protected Task<T> WithDbContextAsync<T>(Func<TrsDbContext, Task<T>> action) =>
        DbContextFactory.WithDbContextAsync(action);

    protected Task WithDbContextAsync(Func<TrsDbContext, Task> action) =>
        DbContextFactory.WithDbContextAsync(action);
}
