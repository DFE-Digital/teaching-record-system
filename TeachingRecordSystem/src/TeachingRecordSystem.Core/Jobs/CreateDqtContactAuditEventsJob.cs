using Microsoft.Extensions.Logging;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Services.TrsDataSync;

namespace TeachingRecordSystem.Core.Jobs;

public class CreateDqtContactAuditEventsJob(
    TrsDataSyncHelper syncHelper,
    IDbContextFactory<TrsDbContext> dbContextFactory,
    ILogger<CreateDqtContactAuditEventsJob> logger)
{
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        dbContext.Database.SetCommandTimeout(0);

        int processed = 0;

        // If we've already processed some persons before, we can skip them
        var lastProcessedPersonId = await dbContext.Set<Process>()
            .Select(p => p.PersonIds.SingleOrDefault())
            .OrderBy(id => id)
            .LastOrDefaultAsync(cancellationToken);

        var personChunks = dbContext.Persons
            .Where(p => p.DqtContactId != null && p.PersonId > lastProcessedPersonId)
            .Select(p => p.PersonId)
            .OrderBy(id => id)
            .ToAsyncEnumerable()
            .ChunkAsync(250);

        await foreach (var personIds in personChunks.WithCancellation(cancellationToken))
        {
            await syncHelper.SyncPersonAuditsAsync(personIds, cancellationToken);

            processed += personIds.Length;

            if (processed % 10_000 == 0)
            {
                logger.LogInformation("Processed {Processed} persons", processed);
            }
        }
    }
}
