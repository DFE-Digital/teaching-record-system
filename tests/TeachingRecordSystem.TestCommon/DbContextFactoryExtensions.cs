using TeachingRecordSystem.Core.DataStore.Postgres;

namespace TeachingRecordSystem.TestCommon;

public static class DbContextFactoryExtensions
{
    extension(IDbContextFactory<TrsDbContext> dbContextFactory)
    {
        public async Task<T> WithDbContextAsync<T>(Func<TrsDbContext, Task<T>> action)
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync();
            return await action(dbContext);
        }

        public Task WithDbContextAsync(Func<TrsDbContext, Task> action) =>
            dbContextFactory.WithDbContextAsync(async dbContext =>
            {
                await action(dbContext);
                return 0;
            });
    }
}
