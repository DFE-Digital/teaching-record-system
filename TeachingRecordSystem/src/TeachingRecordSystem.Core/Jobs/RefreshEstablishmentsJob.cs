using TeachingRecordSystem.Core.Jobs.Scheduling;
using TeachingRecordSystem.Core.Services.Establishments.Gias;

namespace TeachingRecordSystem.Core.Jobs;

public class RefreshEstablishmentsJob(IBackgroundJobScheduler backgroundJobScheduler)
{
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var refreshJobId = await backgroundJobScheduler.Enqueue<EstablishmentRefresher>(j => j.RefreshEstablishments(cancellationToken));

        // The following is temporarily commented out as the current query was take over an hour to return and needs further investigation
        // await backgroundJobScheduler.ContinueJobWith<TpsCsvExtractProcessor>(refreshJobId, j => j.UpdateLatestEstablishmentVersions(cancellationToken));
    }
}
