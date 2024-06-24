using TeachingRecordSystem.Core.Jobs.Scheduling;
using TeachingRecordSystem.Core.Services.Establishments.Gias;
using TeachingRecordSystem.Core.Services.WorkforceData;

namespace TeachingRecordSystem.Core.Jobs;

public class RefreshEstablishmentsJob(IBackgroundJobScheduler backgroundJobScheduler)
{
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var refreshJobId = await backgroundJobScheduler.Enqueue<EstablishmentRefresher>(j => j.RefreshEstablishments(cancellationToken));
        await backgroundJobScheduler.ContinueJobWith<TpsCsvExtractProcessor>(refreshJobId, j => j.UpdateLatestEstablishmentVersions(cancellationToken));
    }
}
