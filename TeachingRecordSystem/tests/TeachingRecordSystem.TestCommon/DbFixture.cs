using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using TeachingRecordSystem.Core.DataStore.Postgres;

namespace TeachingRecordSystem.TestCommon;

public class DbFixture(DbHelper dbHelper, IServiceProvider services)
{
    public DbHelper DbHelper { get; } = dbHelper;

    public NpgsqlDataSource GetDataSource() => services.GetRequiredService<NpgsqlDataSource>();

    public IDbContextFactory<TrsDbContext> GetDbContextFactory() => services.GetRequiredService<IDbContextFactory<TrsDbContext>>();

    public async Task<T> WithDbContextAsync<T>(Func<TrsDbContext, Task<T>> action)
    {
        await using var dbContext = await GetDbContextFactory().CreateDbContextAsync();
        return await action(dbContext);
    }

    public Task WithDbContextAsync(Func<TrsDbContext, Task> action) =>
        WithDbContextAsync(async dbContext =>
        {
            await action(dbContext);
            return 0;
        });
}
