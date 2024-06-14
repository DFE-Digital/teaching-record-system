using TeachingRecordSystem.Core.Services.WorkforceData;

namespace TeachingRecordSystem.Core.Jobs;

public class BackfillNinoAndPersonPostcodeInEmploymentHistoryJob(TpsCsvExtractProcessor tpsCsvExtractProcessor)
{
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await tpsCsvExtractProcessor.BackfillNinoAndPersonPostcodeInEmploymentHistory(cancellationToken);
    }
}
