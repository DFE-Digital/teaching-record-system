using TeachingRecordSystem.Core.Services.WorkforceData;

namespace TeachingRecordSystem.Core.Jobs;

public class ExportWorkforceDataJob(WorkforceDataExporter workforceDataExporter)
{
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await workforceDataExporter.ExportAsync(cancellationToken);
    }
}
