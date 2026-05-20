using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using TeachingRecordSystem.Core.DataStore.Postgres;

namespace TeachingRecordSystem.TestCommon;

public class DbFixture(DbHelper dbHelper, IServiceProvider services)
{
    public DbHelper DbHelper { get; } = dbHelper;

    public NpgsqlDataSource DataSource => services.GetRequiredService<NpgsqlDataSource>();

    public IDbContextFactory<TrsDbContext> DbContextFactory => services.GetRequiredService<IDbContextFactory<TrsDbContext>>();

    public Task<T> WithDbContextAsync<T>(Func<TrsDbContext, Task<T>> action) =>
        DbContextFactory.WithDbContextAsync(action);

    public Task WithDbContextAsync(Func<TrsDbContext, Task> action) =>
        DbContextFactory.WithDbContextAsync(action);
}
