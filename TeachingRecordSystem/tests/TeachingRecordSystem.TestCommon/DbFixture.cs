using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using TeachingRecordSystem.Core.DataStore.Postgres;

namespace TeachingRecordSystem.TestCommon;

public class DbFixture(DbHelper dbHelper, IServiceProvider serviceProvider)
{
    public DbHelper DbHelper { get; } = dbHelper;

    public IServiceProvider Services { get; } = serviceProvider;

    public NpgsqlDataSource GetDataSource() => Services.GetRequiredService<NpgsqlDataSource>();

    public Task CreateReplicationSlotAsync(string slot) => WithDbContextAsync(dbContext =>
        dbContext.Database.ExecuteSqlAsync($"select * from pg_create_logical_replication_slot({slot}, 'pgoutput');"));

    public Task DropReplicationSlotAsync(string slot) => WithDbContextAsync(dbContext =>
        dbContext.Database.ExecuteSqlAsync($"select pg_drop_replication_slot({slot});"));

    public Task AdvanceReplicationSlotToCurrentWalLsnAsync(string slot) => WithDbContextAsync(dbContext =>
        dbContext.Database.ExecuteSqlAsync($"select * from pg_replication_slot_advance({slot}, pg_current_wal_lsn());"));

    public IDbContextFactory<TrsDbContext> GetDbContextFactory() => Services.GetRequiredService<IDbContextFactory<TrsDbContext>>();

    public virtual async Task<T> WithDbContextAsync<T>(Func<TrsDbContext, Task<T>> action)
    {
        await using var dbContext = await GetDbContextFactory().CreateDbContextAsync();
        return await action(dbContext);
    }

    public virtual Task WithDbContextAsync(Func<TrsDbContext, Task> action) =>
        WithDbContextAsync(async dbContext =>
        {
            await action(dbContext);
            return 0;
        });

    public Task DeleteAllPersonsAsync() =>
        WithDbContextAsync(dbContext =>
            dbContext.Database.ExecuteSqlAsync(
                $"""
                 update persons set merged_with_person_id = null;
                 delete from persons;
                 """));
}
