using TeachingRecordSystem.Core.Jobs.Scheduling;
using TeachingRecordSystem.Core.Services.Establishments.Tps;
using TeachingRecordSystem.Core.Services.WorkforceData;

namespace TeachingRecordSystem.Core.Jobs;

public class TpsRefreshEstablishmentsJob(
    ITpsExtractStorageService tpsExtractStorageService,
    IBackgroundJobScheduler backgroundJobScheduler)
{
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var tpsEstablishmentFileName = await tpsExtractStorageService.GetPendingEstablishmentImportFileNameAsync(cancellationToken);
        if (tpsEstablishmentFileName == null)
        {
            return;
        }

        var importJobId = await backgroundJobScheduler.EnqueueAsync<TpsEstablishmentRefresher>(j => j.ImportFileAsync(tpsEstablishmentFileName, cancellationToken));
        var archiveJobId = await backgroundJobScheduler.ContinueJobWithAsync<ITpsExtractStorageService>(importJobId, j => j.ArchiveFileAsync(tpsEstablishmentFileName, cancellationToken));
        var refreshJobId = await backgroundJobScheduler.ContinueJobWithAsync<TpsEstablishmentRefresher>(archiveJobId, j => j.RefreshEstablishmentsAsync(cancellationToken));
    }
}
