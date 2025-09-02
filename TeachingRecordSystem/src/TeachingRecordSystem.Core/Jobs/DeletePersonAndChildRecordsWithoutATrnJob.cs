using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Services.Files;

namespace TeachingRecordSystem.Core.Jobs;

public class DeletePersonAndChildRecordsWithoutATrnJob(
    TrsDbContext dbContext,
    IFileService fileService,
    IClock clock)
{
    public async Task ExecuteAsync(bool dryRun, CancellationToken cancellationToken)
    {
        // There are 300K+ persons without a TRN in production so we need to process in batches
        // hoping to use just us a simple on delete cascade but might not be possible
        // checked most foreign keys to persons table and they are all set to cascade on delete except support_tasks for some reason
        // e.g. there is a merged_with_person_id foreign key on the Person table so we'll need to blank that out first
        // One idea is to use raw SQL delete from .. returning person_id
        // then keep track of these IDs to upload the CSV file at the end
        // TpsCsvExtractProcessor is an example batching up into multiple batches
        // would need to allow for dryRun flag and always rollback each batch if dryRun
        // if cascade doesn't work then will have to delete all child records e.g. alerts, qualifications etc. manually in batches too
        // for the moment we are leaving the events alone (even if they are associated with a deleted person) - there is no foreign key so will not cause an issue
    }
}
