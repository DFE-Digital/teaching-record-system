using Microsoft.Extensions.Caching.Memory;
using TeachingRecordSystem.Core.DataStore.Postgres;

namespace TeachingRecordSystem.Core;

public class PersonInfoCache(IDbContextFactory<TrsDbContext> dbContextFactory, IMemoryCache memoryCache)
{
    public async Task<PersonInfo> GetRequiredPersonInfoAsync(Guid personId) =>
        await GetPersonInfoAsync(personId) ?? throw new ArgumentException("Person not found.", nameof(personId));

    public async Task<PersonInfo?> GetPersonInfoAsync(Guid personId) =>
        await memoryCache.GetOrCreateAsync(CacheKeys.PersonInfo(personId), async _ =>
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync();

            return await dbContext.Persons
                .IgnoreQueryFilters()
                .Where(p => p.PersonId == personId)
                .Select(p => new PersonInfo(p.PersonId, p.Trn))
                .SingleOrDefaultAsync();
        });
}

public record PersonInfo(Guid PersonId, string Trn);
