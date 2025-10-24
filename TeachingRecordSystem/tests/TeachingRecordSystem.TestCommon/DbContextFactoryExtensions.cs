using TeachingRecordSystem.Core.DataStore.Postgres;

namespace TeachingRecordSystem.TestCommon;

public static class DbContextFactoryExtensions
{
    public static async Task<T> WithDbContextAsync<T>(this IDbContextFactory<TrsDbContext> dbContextFactory, Func<TrsDbContext, Task<T>> action)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        return await action(dbContext);
    }

    public static Task WithDbContextAsync(this IDbContextFactory<TrsDbContext> dbContextFactory, Func<TrsDbContext, Task> action) =>
        WithDbContextAsync(dbContextFactory, async dbContext =>
        {
            await action(dbContext);
            return 0;
        });
}
