using Microsoft.Extensions.DependencyInjection;
using TeachingRecordSystem.Core.DataStore.Postgres;

namespace TeachingRecordSystem.TestCommon;

public class DbFixture(DbHelper dbHelper, IServiceProvider serviceProvider)
{
    public DbHelper DbHelper { get; } = dbHelper;

    public IServiceProvider Services { get; } = serviceProvider;

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
