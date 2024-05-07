using TeachingRecordSystem.Core.Services.WorkforceData;

namespace TeachingRecordSystem.Core.Jobs;

public class ProcessEndedEmploymentsJob(TpsCsvExtractProcessor tpsCsvExtractProcessor)
{
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await tpsCsvExtractProcessor.ProcessEndedEmployments(cancellationToken);
    }
}
