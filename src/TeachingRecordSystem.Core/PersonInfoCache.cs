using Microsoft.Extensions.Caching.Memory;
using TeachingRecordSystem.Core.DataStore.Postgres;

namespace TeachingRecordSystem.Core;

public class PersonInfoCache(IDbContextFactory<TrsDbContext> dbContextFactory, IMemoryCache memoryCache)
{
    public async Task<PersonInfo> GetRequiredPersonInfoAsync(Guid personId, CancellationToken cancellationToken = default) =>
        await GetPersonInfoAsync(personId, cancellationToken) ?? throw new ArgumentException("Person not found.", nameof(personId));

    public async Task<PersonInfo?> GetPersonInfoAsync(Guid personId, CancellationToken cancellationToken = default) =>
        await memoryCache.GetOrCreateAsync(CacheKeys.PersonInfo(personId), async _ =>
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

            return await dbContext.Persons
                .IgnoreQueryFilters([QueryFilterNames.Person.Deactivated])
                .Where(p => p.PersonId == personId)
                .Select(p => new PersonInfo(p.PersonId, p.Trn))
                .SingleOrDefaultAsync(cancellationToken);
        });
}

public record PersonInfo(Guid PersonId, string Trn);
