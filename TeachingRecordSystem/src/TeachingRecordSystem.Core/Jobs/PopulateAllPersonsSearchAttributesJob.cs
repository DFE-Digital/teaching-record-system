using Hangfire;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Jobs.Scheduling;

namespace TeachingRecordSystem.Core.Jobs;

[AutomaticRetry(Attempts = 0)]
public class PopulateAllPersonsSearchAttributesJob(TrsDbContext dbContext, IBackgroundJobScheduler backgroundJobScheduler)
{
    public async Task Execute()
    {
        await foreach (var personId in dbContext.Persons.AsNoTracking().Select(p => p.PersonId).AsAsyncEnumerable())
        {
            await backgroundJobScheduler.Enqueue<PopulatePersonSearchAttributesJob>(j => j.Execute(personId));
        }
    }
}
