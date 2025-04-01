using TeachingRecordSystem.Core.Services.WorkforceData;

namespace TeachingRecordSystem.Core.Jobs;

public class BackfillEmployerEmailAddressInEmploymentHistoryJob(TpsCsvExtractProcessor tpsCsvExtractProcessor)
{
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await tpsCsvExtractProcessor.BackfillEmployerEmailAddressInEmploymentHistoryAsync(cancellationToken);
    }
}
