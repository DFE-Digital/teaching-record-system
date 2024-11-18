using TeachingRecordSystem.Core.Jobs.Scheduling;
using TeachingRecordSystem.Core.Services.Establishments.Gias;
using TeachingRecordSystem.Core.Services.WorkforceData;

namespace TeachingRecordSystem.Core.Jobs;

public class RefreshEstablishmentsJob(IBackgroundJobScheduler backgroundJobScheduler)
{
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var refreshJobId = await backgroundJobScheduler.EnqueueAsync<EstablishmentRefresher>(j => j.RefreshEstablishmentsAsync(cancellationToken));
        await backgroundJobScheduler.ContinueJobWithAsync<TpsCsvExtractProcessor>(refreshJobId, j => j.UpdateLatestEstablishmentVersionsAsync(cancellationToken));
    }
}
