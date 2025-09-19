using TeachingRecordSystem.Core.DataStore.Postgres;

namespace TeachingRecordSystem.Core.Jobs;

public class DeleteStaleJourneyStatesJob(TrsDbContext dbContext, IClock clock)
{
    public const string JobSchedule = "0 3 * * *";  //3AM every day
    public const string LastRunDateKey = "LastRunDate";

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var cutoffDate = clock.Today.AddDays(-1).ToDateTime();
        var staleJourneyStates = await dbContext.JourneyStates
           .Where(e => e.Created < clock.Today.AddDays(-1).ToDateTime())
           .ExecuteDeleteAsync();

        await dbContext.SaveChangesAsync();
    }
}
