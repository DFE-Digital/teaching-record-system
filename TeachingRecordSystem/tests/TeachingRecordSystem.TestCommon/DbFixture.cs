using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using TeachingRecordSystem.Core.DataStore.Postgres;

namespace TeachingRecordSystem.TestCommon;

public class DbFixture(DbHelper dbHelper, IServiceProvider serviceProvider)
{
    public DbHelper DbHelper { get; } = dbHelper;

    public IServiceProvider Services { get; } = serviceProvider;

    public NpgsqlDataSource GetDataSource() => Services.GetRequiredService<NpgsqlDataSource>();

    public Task CreateReplicationSlot(string slot) => WithDbContext(dbContext =>
        dbContext.Database.ExecuteSqlAsync($"select * from pg_create_logical_replication_slot({slot}, 'pgoutput');"));

    public Task DropReplicationSlot(string slot) => WithDbContext(dbContext =>
        dbContext.Database.ExecuteSqlAsync($"select pg_drop_replication_slot({slot});"));

    public Task AdvanceReplicationSlotToCurrentWalLsn(string slot) => WithDbContext(dbContext =>
        dbContext.Database.ExecuteSqlAsync($"select * from pg_replication_slot_advance({slot}, pg_current_wal_lsn());"));

    public TrsDbContext GetDbContext() => Services.GetRequiredService<TrsDbContext>();

    public IDbContextFactory<TrsDbContext> GetDbContextFactory() => Services.GetRequiredService<IDbContextFactory<TrsDbContext>>();

    public virtual async Task<T> WithDbContext<T>(Func<TrsDbContext, Task<T>> action)
    {
        await using var dbContext = await GetDbContextFactory().CreateDbContextAsync();
        return await action(dbContext);
    }

    public virtual Task WithDbContext(Func<TrsDbContext, Task> action) =>
        WithDbContext(async dbContext =>
        {
            await action(dbContext);
            return 0;
        });
}
