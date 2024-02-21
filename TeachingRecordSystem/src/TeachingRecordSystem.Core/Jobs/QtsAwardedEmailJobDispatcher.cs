using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Jobs.Scheduling;

namespace TeachingRecordSystem.Core.Jobs;

public class QtsAwardedEmailJobDispatcher
{
    private readonly TrsDbContext _dbContext;
    private readonly IBackgroundJobScheduler _backgroundJobScheduler;

    public QtsAwardedEmailJobDispatcher(
        TrsDbContext dbContext,
        IBackgroundJobScheduler backgroundJobScheduler)
    {
        _dbContext = dbContext;
        _backgroundJobScheduler = backgroundJobScheduler;
    }

    public async Task Execute(Guid qtsAwardedEmailsJobId)
    {
        var jobItems = await _dbContext.QtsAwardedEmailsJobItems
            .Where(i => i.QtsAwardedEmailsJobId == qtsAwardedEmailsJobId && i.EmailSent == false)
            .ToListAsync();

        foreach (var jobItem in jobItems)
        {
            await _backgroundJobScheduler.Enqueue<SendQtsAwardedEmailJob>(j => j.Execute(qtsAwardedEmailsJobId, jobItem.PersonId));
        }
    }
}
