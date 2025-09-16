using System.Globalization;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.Jobs;

public class RemoveStaleJourneyStatesJob(TrsDbContext dbContext, IClock clock)
{
    public const string JobSchedule = "25 14 * * *";  //3AM every day
    public const string LastRunDateKey = "LastRunDate";

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var item = await dbContext.JobMetadata.SingleOrDefaultAsync(i => i.JobName == nameof(RemoveStaleJourneyStatesJob));

        var cutoffDate = clock.Today.AddDays(-1).ToDateTime();
        var staleJourneyStates = await dbContext.JourneyStates
           .Where(e => e.Created < clock.Today.AddDays(-1).ToDateTime())
           .ToListAsync();

        dbContext.JourneyStates.RemoveRange(staleJourneyStates);

        //update last run
        if (item != null)
        {
            item.Metadata = new Dictionary<string, string>
            {
                { LastRunDateKey, clock.UtcNow.ToString("s", CultureInfo.InvariantCulture) }
            };
        }
        else
        {
            item = new JobMetadata()
            {
                JobName = nameof(RemoveStaleJourneyStatesJob),
                Metadata = new Dictionary<string, string>
                {
                    { LastRunDateKey, clock.UtcNow.ToString("s", CultureInfo.InvariantCulture) }
                }
            };
            dbContext.JobMetadata.Add(item);
        }

        await dbContext.SaveChangesAsync();
    }
}
