using TeachingRecordSystem.Core.Services.DqtReporting;

namespace TeachingRecordSystem.Core.Jobs;

public class BackfillSupportTasksInReportingDb(ReportingHelper reportingHelper)
{
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await reportingHelper.BackfillTrsTableAsync("support_tasks", cancellationToken);
    }
}
