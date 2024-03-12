using TeachingRecordSystem.Core.Jobs.Scheduling;
using TeachingRecordSystem.Core.Services.WorkforceData;

namespace TeachingRecordSystem.Core.Jobs;

public class ImportTpsCsvExtractFileJob(
    ITpsExtractStorageService tpsExtractStorageService,
    IBackgroundJobScheduler backgroundJobScheduler)
{
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var pendingImportFileNames = await tpsExtractStorageService.GetPendingImportFileNames(cancellationToken);
        if (pendingImportFileNames.Length == 0)
        {
            return;
        }

        // If we ever need to process more than one file then we can always manually trigger this job again or add a loop here
        var tpsCsvExtractId = Guid.NewGuid();
        var importJobId = await backgroundJobScheduler.Enqueue<TpsCsvExtractFileImporter>(j => j.ImportFile(tpsCsvExtractId, pendingImportFileNames[0], cancellationToken));
        var archiveJobId = await backgroundJobScheduler.ContinueJobWith<ITpsExtractStorageService>(importJobId, j => j.ArchiveFile(pendingImportFileNames[0], cancellationToken));
        var copyJobId = await backgroundJobScheduler.ContinueJobWith<TpsCsvExtractFileImporter>(archiveJobId, j => j.CopyValidFormatDataToStaging(tpsCsvExtractId, cancellationToken));
        var processInvalidTrnsJobId = await backgroundJobScheduler.ContinueJobWith<TpsCsvExtractProcessor>(copyJobId, j => j.ProcessNonMatchingTrns(tpsCsvExtractId, cancellationToken));
        var processInvalidEstablishmentsJobId = await backgroundJobScheduler.ContinueJobWith<TpsCsvExtractProcessor>(processInvalidTrnsJobId, j => j.ProcessNonMatchingEstablishments(tpsCsvExtractId, cancellationToken));
        var processNewEmploymentHistoryJobId = await backgroundJobScheduler.ContinueJobWith<TpsCsvExtractProcessor>(processInvalidEstablishmentsJobId, j => j.ProcessNewEmploymentHistory(tpsCsvExtractId, cancellationToken));
        var processUpdatedEmploymentHistoryJobId = await backgroundJobScheduler.ContinueJobWith<TpsCsvExtractProcessor>(processNewEmploymentHistoryJobId, j => j.ProcessUpdatedEmploymentHistory(tpsCsvExtractId, cancellationToken));
    }
}
