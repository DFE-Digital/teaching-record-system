using TeachingRecordSystem.Core.Jobs.Scheduling;
using TeachingRecordSystem.Core.Services.WorkforceData;

namespace TeachingRecordSystem.Core.Jobs;

public class ProcessEndedEmploymentsJob(IBackgroundJobScheduler backgroundJobScheduler)
{
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await backgroundJobScheduler.Enqueue<TpsCsvExtractProcessor>(j => j.ProcessEndedEmployments(cancellationToken));
    }
}
