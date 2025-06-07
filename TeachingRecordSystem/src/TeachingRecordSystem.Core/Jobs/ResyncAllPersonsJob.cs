using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Services.TrsDataSync;

namespace TeachingRecordSystem.Core.Jobs;

public class ResyncAllPersonsJob(TrsDataSyncHelper syncHelper, IDbContextFactory<TrsDbContext> dbContextFactory)
{
    public async Task ExecuteAsync(DateTime lastSyncedBefore, CancellationToken cancellationToken)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        dbContext.Database.SetCommandTimeout(0);

        var personsToSync = dbContext.Persons
            .Where(p => p.DqtLastSync < lastSyncedBefore)
            .Select(p => p.PersonId)
            .AsAsyncEnumerable();

        await foreach (var personIds in personsToSync.ChunkAsync(50).WithCancellation(cancellationToken))
        {
            await syncHelper.SyncPersonsAsync(personIds, syncAudit: false, cancellationToken: cancellationToken);
        }
    }
}
