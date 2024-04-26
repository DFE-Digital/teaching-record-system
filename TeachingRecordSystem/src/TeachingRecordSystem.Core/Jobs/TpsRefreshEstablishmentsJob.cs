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
        var tpsEstablishmentFileName = await tpsExtractStorageService.GetPendingEstablishmentImportFileName(cancellationToken);
        if (tpsEstablishmentFileName == null)
        {
            return;
        }

        var importJobId = await backgroundJobScheduler.Enqueue<TpsEstablishmentRefresher>(j => j.ImportFile(tpsEstablishmentFileName, cancellationToken));
        var archiveJobId = await backgroundJobScheduler.ContinueJobWith<ITpsExtractStorageService>(importJobId, j => j.ArchiveFile(tpsEstablishmentFileName, cancellationToken));
        var refreshJobId = await backgroundJobScheduler.ContinueJobWith<TpsEstablishmentRefresher>(archiveJobId, j => j.RefreshEstablishments(cancellationToken));
    }
}
