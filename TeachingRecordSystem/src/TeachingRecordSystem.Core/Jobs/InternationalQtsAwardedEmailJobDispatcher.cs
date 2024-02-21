using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Jobs.Scheduling;

namespace TeachingRecordSystem.Core.Jobs;

public class InternationalQtsAwardedEmailJobDispatcher
{
    private readonly TrsDbContext _dbContext;
    private readonly IBackgroundJobScheduler _backgroundJobScheduler;

    public InternationalQtsAwardedEmailJobDispatcher(
        TrsDbContext dbContext,
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
