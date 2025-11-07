using TeachingRecordSystem.Core.Services.DqtReporting;

namespace TeachingRecordSystem.Core.Jobs;

public class BackfillTrnRequestMetadataInReportingDb(ReportingHelper reportingHelper)
{
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await reportingHelper.BackfillTrsTableAsync("trn_request_metadata", cancellationToken);
    }
}
