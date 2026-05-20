using TeachingRecordSystem.Core.Services.DqtReporting;

namespace TeachingRecordSystem.Core.Jobs;

public class BackfillUsersInReportingDb(ReportingHelper reportingHelper)
{
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await reportingHelper.BackfillTrsTableAsync("users", cancellationToken);
    }
}
