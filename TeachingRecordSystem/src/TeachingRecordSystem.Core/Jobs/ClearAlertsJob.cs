using Hangfire;
using TeachingRecordSystem.Core.DataStore.Postgres;

namespace TeachingRecordSystem.Core.Jobs;

[AutomaticRetry(Attempts = 0)]
public class ClearAlertsJob(TrsDbContext dbContext)
{
    public async Task ExecuteAsync()
    {
        await using var txn = await dbContext.Database.BeginTransactionAsync();
        dbContext.Database.SetCommandTimeout(0);
        await dbContext.Database.ExecuteSqlAsync($"delete from events where event_name like 'Alert%';");
        await dbContext.Database.ExecuteSqlAsync($"delete from alerts;");
        await txn.CommitAsync();
    }
}
