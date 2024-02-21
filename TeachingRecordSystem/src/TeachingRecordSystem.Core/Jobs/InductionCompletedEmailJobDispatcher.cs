using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Jobs.Scheduling;

namespace TeachingRecordSystem.Core.Jobs;

public class InductionCompletedEmailJobDispatcher
{
    private readonly TrsDbContext _dbContext;
    private readonly IBackgroundJobScheduler _backgroundJobScheduler;

    public InductionCompletedEmailJobDispatcher(
        TrsDbContext dbContext,
        IBackgroundJobScheduler backgroundJobScheduler)
    {
        _dbContext = dbContext;
        _backgroundJobScheduler = backgroundJobScheduler;
    }

    public async Task Execute(Guid inductionCompletedEmailsJobId)
    {
        var jobItems = await _dbContext.InductionCompletedEmailsJobItems
            .Where(i => i.InductionCompletedEmailsJobId == inductionCompletedEmailsJobId && i.EmailSent == false)
            .ToListAsync();

        foreach (var jobItem in jobItems)
        {
            await _backgroundJobScheduler.Enqueue<SendInductionCompletedEmailJob>(j => j.Execute(inductionCompletedEmailsJobId, jobItem.PersonId));
        }
    }
}
