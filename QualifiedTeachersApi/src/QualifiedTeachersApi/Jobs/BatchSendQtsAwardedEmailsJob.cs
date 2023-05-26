using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using QualifiedTeachersApi.DataStore.Crm;
using QualifiedTeachersApi.DataStore.Sql;
using QualifiedTeachersApi.DataStore.Sql.Models;
using QualifiedTeachersApi.Jobs.Scheduling;

namespace QualifiedTeachersApi.Jobs;

public class BatchSendQtsAwardedEmailsJob
{
    private readonly BatchSendQtsAwardedEmailsJobOptions _batchSendQtsAwardedEmailsJobOptions;
    private readonly DqtContext _dbContext;
    private readonly IDataverseAdapter _dataverseAdapter;
    private readonly IBackgroundJobScheduler _backgroundJobScheduler;
    private readonly IClock _clock;

    public BatchSendQtsAwardedEmailsJob(
        IOptions<BatchSendQtsAwardedEmailsJobOptions> batchSendQtsAwardedEmailsJobOptions,
        DqtContext dbContext,
        IDataverseAdapter dataverseAdapter,
        IBackgroundJobScheduler backgroundJobScheduler,
        IClock clock)
    {
        _batchSendQtsAwardedEmailsJobOptions = batchSendQtsAwardedEmailsJobOptions.Value;
        _dbContext = dbContext;
        _dataverseAdapter = dataverseAdapter;
        _backgroundJobScheduler = backgroundJobScheduler;
        _clock = clock;
    }

    public async Task Execute(CancellationToken cancellationToken)
    {
        var lastAwardedToUtc = _batchSendQtsAwardedEmailsJobOptions.InitialLastAwardedDateUtc;
        var lastExecutedJob = await _dbContext.QtsAwardedEmailsJobs.OrderBy(j => j.ExecutedUtc).LastOrDefaultAsync();
        if (lastExecutedJob != null)
        {
            lastAwardedToUtc = lastExecutedJob.AwardedToUtc;
        }

        // Look for QTS up to the end of the day the configurable amount of days ago to provide a delay between award and email being sent.
        var awardedToUtc = (_clock.Today.ToDateTime() - TimeSpan.FromDays(_batchSendQtsAwardedEmailsJobOptions.EmailDelayDays + 1)) - TimeSpan.FromSeconds(1);

        var executed = _clock.UtcNow;
        var startDate = lastAwardedToUtc.AddSeconds(1);
        var endDate = awardedToUtc;
        var qtsAwardedEmailsJobId = Guid.NewGuid();
        var job = new QtsAwardedEmailsJob
        {
            QtsAwardedEmailsJobId = qtsAwardedEmailsJobId,
            AwardedToUtc = awardedToUtc,
            ExecutedUtc = executed
        };
        await _dbContext.QtsAwardedEmailsJobs.AddAsync(job, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var qtsAwardees = await _dataverseAdapter.GetQtsAwardeesForDateRange(startDate, endDate);
        foreach (var qtsAwardee in qtsAwardees)
        {
            var personalisation = new Dictionary<string, string>()
            {
                { "first name", qtsAwardee.FirstName },
                { "last name", qtsAwardee.LastName },
            };

            var jobItem = new QtsAwardedEmailsJobItem
            {
                QtsAwardedEmailsJobId = qtsAwardedEmailsJobId,
                PersonId = qtsAwardee.TeacherId,
                Trn = qtsAwardee.Trn,
                EmailAddress = qtsAwardee.EmailAddress,
                Personalization = personalisation
            };

            await _dbContext.QtsAwardedEmailsJobItems.AddAsync(jobItem, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            await _backgroundJobScheduler.Enqueue<SendQtsAwardedEmailJob>(j => j.Execute(qtsAwardedEmailsJobId, jobItem.PersonId));
        }
    }
}
