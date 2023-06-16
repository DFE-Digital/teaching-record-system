using Microsoft.EntityFrameworkCore;
using TeachingRecordSystem.Api.DataStore.Sql;
using TeachingRecordSystem.Api.Jobs.Scheduling;

namespace TeachingRecordSystem.Api.Jobs;

public class EytsAwardedEmailJobDispatcher
{
    private readonly DqtContext _dbContext;
    private readonly IBackgroundJobScheduler _backgroundJobScheduler;

    public EytsAwardedEmailJobDispatcher(
        DqtContext dbContext,
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
