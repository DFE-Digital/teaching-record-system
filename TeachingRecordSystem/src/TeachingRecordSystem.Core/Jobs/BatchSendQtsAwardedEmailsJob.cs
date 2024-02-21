using System.Transactions;
using Microsoft.Extensions.Options;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Jobs.Scheduling;

namespace TeachingRecordSystem.Core.Jobs;

public class BatchSendQtsAwardedEmailsJob
{
    private readonly BatchSendQtsAwardedEmailsJobOptions _batchSendQtsAwardedEmailsJobOptions;
    private readonly TrsDbContext _dbContext;
    private readonly IDataverseAdapter _dataverseAdapter;
    private readonly IBackgroundJobScheduler _backgroundJobScheduler;
    private readonly IClock _clock;

    public BatchSendQtsAwardedEmailsJob(
        IOptions<BatchSendQtsAwardedEmailsJobOptions> batchSendQtsAwardedEmailsJobOptions,
        TrsDbContext dbContext,
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
        var lastAwardedToUtc = await _dbContext.QtsAwardedEmailsJobs.MaxAsync(j => (DateTime?)j.AwardedToUtc) ??
            _batchSendQtsAwardedEmailsJobOptions.InitialLastAwardedToUtc;

        // Look for new QTS awards up to the end of the day the configurable amount of days ago to provide a delay between award being given and email being sent.
        var awardedToUtc = _clock.Today.AddDays(-_batchSendQtsAwardedEmailsJobOptions.EmailDelayDays).ToDateTime();

        var executed = _clock.UtcNow;
        var startDate = lastAwardedToUtc;
        var endDate = awardedToUtc;
        var qtsAwardedEmailsJobId = Guid.NewGuid();
        var job = new QtsAwardedEmailsJob
        {
            QtsAwardedEmailsJobId = qtsAwardedEmailsJobId,
            AwardedToUtc = awardedToUtc,
            ExecutedUtc = executed
        };

        using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        _dbContext.QtsAwardedEmailsJobs.Add(job);

        var totalQtsAwardees = 0;
        await foreach (var qtsAwardees in _dataverseAdapter.GetQtsAwardeesForDateRange(startDate, endDate))
        {
            foreach (var qtsAwardee in qtsAwardees)
            {
                if (await _dbContext.QtsAwardedEmailsJobItems.AnyAsync(i => i.Trn == qtsAwardee.Trn))
                {
                    continue;
                }

                var personalization = new Dictionary<string, string>()
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
                    Personalization = personalization
                };

                _dbContext.QtsAwardedEmailsJobItems.Add(jobItem);

                totalQtsAwardees++;
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        if (totalQtsAwardees > 0)
        {
            await _backgroundJobScheduler.Enqueue<QtsAwardedEmailJobDispatcher>(j => j.Execute(qtsAwardedEmailsJobId));
        }

        transaction.Complete();
    }
}
