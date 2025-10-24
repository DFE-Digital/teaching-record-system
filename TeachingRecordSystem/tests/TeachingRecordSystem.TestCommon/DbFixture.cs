using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using TeachingRecordSystem.Core.DataStore.Postgres;

namespace TeachingRecordSystem.TestCommon;

public class DbFixture(IServiceProvider services)
{
    public DbHelper DbHelper => services.GetRequiredService<DbHelper>();

    public NpgsqlDataSource DataSource => services.GetRequiredService<NpgsqlDataSource>();

    public IDbContextFactory<TrsDbContext> DbContextFactory => services.GetRequiredService<IDbContextFactory<TrsDbContext>>();

    public IServiceProvider Services => services;

    public async Task<T> WithDbContextAsync<T>(Func<TrsDbContext, Task<T>> action)
    {
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        return await action(dbContext);
    }

    public Task WithDbContextAsync(Func<TrsDbContext, Task> action) =>
        WithDbContextAsync(async dbContext =>
        {
            await action(dbContext);
            return 0;
        });
}
