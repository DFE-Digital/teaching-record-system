using TeachingRecordSystem.Core.Services.DqtReporting;

namespace TeachingRecordSystem.Core.Jobs;

public class BackfillAlertsInReportingDb(ReportingHelper reportingHelper)
{
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await reportingHelper.BackfillTrsTableAsync("alerts", cancellationToken);
    }
}
