using TeachingRecordSystem.Core.DataStore.Postgres;

namespace TeachingRecordSystem.Core.Jobs;

public class DeleteStaleJourneyStatesJob(TrsDbContext dbContext, TimeProvider timeProvider)
{
    public const string JobSchedule = "0 3 * * *";  //3AM every day

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        dbContext.Database.SetCommandTimeout(0);

        var cutoffDate = timeProvider.Today.AddDays(-1).ToDateTime();

        await dbContext.JourneyStates
           .Where(e => e.Created < cutoffDate)
           .ExecuteDeleteAsync(cancellationToken);
    }
}
