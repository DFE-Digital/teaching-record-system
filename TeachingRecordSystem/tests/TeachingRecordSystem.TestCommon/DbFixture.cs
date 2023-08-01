using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TeachingRecordSystem.Core.DataStore.Postgres;

namespace TeachingRecordSystem.TestCommon;

public class DbFixture
{
    public DbFixture(DbHelper dbHelper, IServiceProvider serviceProvider)
    {
        DbHelper = dbHelper;
        Services = serviceProvider;
    }

    public DbHelper DbHelper { get; }

    public IServiceProvider Services { get; }

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
