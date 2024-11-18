using TeachingRecordSystem.Core.Jobs.Scheduling;
using TeachingRecordSystem.Core.Services.WorkforceData;

namespace TeachingRecordSystem.Core.Jobs;

public class ImportTpsCsvExtractFileJob(
    ITpsExtractStorageService tpsExtractStorageService,
    IBackgroundJobScheduler backgroundJobScheduler)
{
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var pendingImportFileNames = await tpsExtractStorageService.GetPendingImportFileNamesAsync(cancellationToken);
        if (pendingImportFileNames.Length == 0)
        {
            return;
        }

        // If we ever need to process more than one file then we can always manually trigger this job again or add a loop here
        var tpsCsvExtractId = Guid.NewGuid();
        var importJobId = await backgroundJobScheduler.EnqueueAsync<TpsCsvExtractFileImporter>(j => j.ImportFileAsync(tpsCsvExtractId, pendingImportFileNames[0], cancellationToken));
        var archiveJobId = await backgroundJobScheduler.ContinueJobWithAsync<ITpsExtractStorageService>(importJobId, j => j.ArchiveFileAsync(pendingImportFileNames[0], cancellationToken));
        var copyJobId = await backgroundJobScheduler.ContinueJobWithAsync<TpsCsvExtractFileImporter>(archiveJobId, j => j.CopyValidFormatDataToStagingAsync(tpsCsvExtractId, cancellationToken));
        var processInvalidTrnsJobId = await backgroundJobScheduler.ContinueJobWithAsync<TpsCsvExtractProcessor>(copyJobId, j => j.ProcessNonMatchingTrnsAsync(tpsCsvExtractId, cancellationToken));
        var processInvalidEstablishmentsJobId = await backgroundJobScheduler.ContinueJobWithAsync<TpsCsvExtractProcessor>(processInvalidTrnsJobId, j => j.ProcessNonMatchingEstablishmentsAsync(tpsCsvExtractId, cancellationToken));
        var processNewEmploymentHistoryJobId = await backgroundJobScheduler.ContinueJobWithAsync<TpsCsvExtractProcessor>(processInvalidEstablishmentsJobId, j => j.ProcessNewEmploymentHistoryAsync(tpsCsvExtractId, cancellationToken));
        var processUpdatedEmploymentHistoryJobId = await backgroundJobScheduler.ContinueJobWithAsync<TpsCsvExtractProcessor>(processNewEmploymentHistoryJobId, j => j.ProcessUpdatedEmploymentHistoryAsync(tpsCsvExtractId, cancellationToken));
    }
}
