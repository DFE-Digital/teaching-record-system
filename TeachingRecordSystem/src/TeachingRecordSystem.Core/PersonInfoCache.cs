using Microsoft.Extensions.Caching.Memory;
using TeachingRecordSystem.Core.DataStore.Postgres;

namespace TeachingRecordSystem.Core;

public class PersonInfoCache(IDbContextFactory<TrsDbContext> dbContextFactory, IMemoryCache memoryCache)
{
    public async Task<PersonInfo> GetPersonInfoAsync(Guid personId) =>
        await memoryCache.GetOrCreateAsync<PersonInfo?>(CacheKeys.PersonInfo(personId), async _ =>
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync();

            return await dbContext.Persons
                .Where(p => p.PersonId == personId && p.Trn != null)
                .Select(p => new PersonInfo(p.PersonId, p.Trn!))
                .SingleOrDefaultAsync();
        }) ?? throw new ArgumentException("Person not found.", nameof(personId));
}

public record PersonInfo(Guid PersonId, string Trn);
