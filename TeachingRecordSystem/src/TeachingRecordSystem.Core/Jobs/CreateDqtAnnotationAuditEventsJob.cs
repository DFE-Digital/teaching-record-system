using Hangfire.Server;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Services.TrsDataSync;

namespace TeachingRecordSystem.Core.Jobs;

public class CreateDqtAnnotationAuditEventsJob(
    TrsDataSyncHelper syncHelper,
    IDbContextFactory<TrsDbContext> dbContextFactory)
{
    public async Task ExecuteAsync(PerformContext context, CancellationToken cancellationToken)
    {
        var jobId = context.BackgroundJob.Id;
        var jobName = $"{nameof(CreateDqtAnnotationAuditEventsJob)}-{jobId}";

        await using var metadataDbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        metadataDbContext.Database.SetCommandTimeout(0);

        await using var readAnnotationsDbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        readAnnotationsDbContext.Database.SetCommandTimeout(0);

        var jobMetadata = await metadataDbContext.JobMetadata.SingleOrDefaultAsync(j => j.JobName == jobName, cancellationToken: cancellationToken);

        if (jobMetadata is null)
        {
            jobMetadata = new JobMetadata { JobName = jobName, Metadata = new() };
            metadataDbContext.JobMetadata.Add(jobMetadata);
            await metadataDbContext.SaveChangesAsync(cancellationToken);
        }

        var lastProcessedAnnotationId = jobMetadata.Metadata.TryGetValue(MetadataKeys.LastProcessedNoteId, out var lppIdStr)
            ? new Guid(lppIdStr)
            : Guid.Empty;

        int processed = 0;

        var annotationChunks = readAnnotationsDbContext.Notes
            .IgnoreQueryFilters()
            .Where(p => p.ContentHtml != null && p.NoteId > lastProcessedAnnotationId)
            .Select(p => p.NoteId)
            .OrderBy(id => id)
            .ToAsyncEnumerable()
            .ChunkAsync(250);

        await foreach (var annotationIds in annotationChunks.WithCancellation(cancellationToken))
        {
            await syncHelper.SyncAnnotationAuditsAsync(annotationIds, cancellationToken);

            processed += annotationIds.Length;

            if (processed % 1000 == 0)
            {
                jobMetadata.Metadata[MetadataKeys.LastProcessedNoteId] = annotationIds.Last().ToString();
                await metadataDbContext.SaveChangesAsync(cancellationToken);
            }
        }

        await metadataDbContext.JobMetadata.Where(j => j.JobName == jobName).ExecuteDeleteAsync();
    }

    private static class MetadataKeys
    {
        public const string LastProcessedNoteId = nameof(LastProcessedNoteId);
    }
}
