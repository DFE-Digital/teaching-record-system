using Microsoft.EntityFrameworkCore;
using TeachingRecordSystem.Api.DataStore.Sql;
using TeachingRecordSystem.Api.Jobs.Scheduling;

namespace TeachingRecordSystem.Api.Jobs;

public class InternationalQtsAwardedEmailJobDispatcher
{
    private readonly DqtContext _dbContext;
    private readonly IBackgroundJobScheduler _backgroundJobScheduler;

    public InternationalQtsAwardedEmailJobDispatcher(
        DqtContext dbContext,
        IBackgroundJobScheduler backgroundJobScheduler)
    {
        _dbContext = dbContext;
        _backgroundJobScheduler = backgroundJobScheduler;
    }

    public async Task Execute(Guid internationalQtsAwardedEmailsJobId)
    {
        var jobItems = await _dbContext.InternationalQtsAwardedEmailsJobItems
            .Where(i => i.InternationalQtsAwardedEmailsJobId == internationalQtsAwardedEmailsJobId && i.EmailSent == false)
            .ToListAsync();

        foreach (var jobItem in jobItems)
        {
            await _backgroundJobScheduler.Enqueue<SendInternationalQtsAwardedEmailJob>(j => j.Execute(internationalQtsAwardedEmailsJobId, jobItem.PersonId));
        }
    }
}
