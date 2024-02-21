using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Jobs.Scheduling;

namespace TeachingRecordSystem.Core.Jobs;

public class EytsAwardedEmailJobDispatcher
{
    private readonly TrsDbContext _dbContext;
    private readonly IBackgroundJobScheduler _backgroundJobScheduler;

    public EytsAwardedEmailJobDispatcher(
        TrsDbContext dbContext,
        IBackgroundJobScheduler backgroundJobScheduler)
    {
        _dbContext = dbContext;
        _backgroundJobScheduler = backgroundJobScheduler;
    }

    public async Task Execute(Guid eytsAwardedEmailsJobId)
    {
        var jobItems = await _dbContext.EytsAwardedEmailsJobItems
            .Where(i => i.EytsAwardedEmailsJobId == eytsAwardedEmailsJobId && i.EmailSent == false)
            .ToListAsync();

        foreach (var jobItem in jobItems)
        {
            await _backgroundJobScheduler.Enqueue<SendEytsAwardedEmailJob>(j => j.Execute(eytsAwardedEmailsJobId, jobItem.PersonId));
        }
    }
}
