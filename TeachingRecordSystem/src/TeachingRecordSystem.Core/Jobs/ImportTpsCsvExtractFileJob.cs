using System.Transactions;
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

        // This job will need to be triggered for each file needing processing
        // Leaving it like this for now until we are totally happy we want to always import all of the pending files without any manual checks in between
        var tpsCsvExtractId = Guid.NewGuid();
        using var txn = new TransactionScope(
            TransactionScopeOption.RequiresNew,
            new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
            TransactionScopeAsyncFlowOption.Enabled);
        var importJobId = await backgroundJobScheduler.EnqueueAsync<TpsCsvExtractFileImporter>(j => j.ImportFileAsync(tpsCsvExtractId, pendingImportFileNames[0], cancellationToken));
        txn.Complete();
        var archiveJobId = await backgroundJobScheduler.ContinueJobWithAsync<ITpsExtractStorageService>(importJobId, j => j.ArchiveFileAsync(pendingImportFileNames[0], cancellationToken));
        var copyJobId = await backgroundJobScheduler.ContinueJobWithAsync<TpsCsvExtractFileImporter>(archiveJobId, j => j.CopyValidFormatDataToStagingAsync(tpsCsvExtractId, cancellationToken));
        var processInvalidTrnsJobId = await backgroundJobScheduler.ContinueJobWithAsync<TpsCsvExtractProcessor>(copyJobId, j => j.ProcessNonMatchingTrnsAsync(tpsCsvExtractId, cancellationToken));
        var processInvalidEstablishmentsJobId = await backgroundJobScheduler.ContinueJobWithAsync<TpsCsvExtractProcessor>(processInvalidTrnsJobId, j => j.ProcessNonMatchingEstablishmentsAsync(tpsCsvExtractId, cancellationToken));
        var processNewEmploymentHistoryJobId = await backgroundJobScheduler.ContinueJobWithAsync<TpsCsvExtractProcessor>(processInvalidEstablishmentsJobId, j => j.ProcessNewEmploymentHistoryAsync(tpsCsvExtractId, cancellationToken));
        var processUpdatedEmploymentHistoryJobId = await backgroundJobScheduler.ContinueJobWithAsync<TpsCsvExtractProcessor>(processNewEmploymentHistoryJobId, j => j.ProcessUpdatedEmploymentHistoryAsync(tpsCsvExtractId, cancellationToken));
    }
}
