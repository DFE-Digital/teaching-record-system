using System.Transactions;
using Microsoft.Extensions.Options;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Jobs.Scheduling;

namespace TeachingRecordSystem.Core.Jobs;

public class BatchSendEytsAwardedEmailsJob
{
    private readonly BatchSendEytsAwardedEmailsJobOptions _batchSendEytsAwardedEmailsJobOptions;
    private readonly TrsDbContext _dbContext;
    private readonly IDataverseAdapter _dataverseAdapter;
    private readonly IBackgroundJobScheduler _backgroundJobScheduler;
    private readonly IClock _clock;

    public BatchSendEytsAwardedEmailsJob(
        IOptions<BatchSendEytsAwardedEmailsJobOptions> batchSendEytsAwardedEmailsJobOptions,
        TrsDbContext dbContext,
        IDataverseAdapter dataverseAdapter,
        IBackgroundJobScheduler backgroundJobScheduler,
        IClock clock)
    {
        _batchSendEytsAwardedEmailsJobOptions = batchSendEytsAwardedEmailsJobOptions.Value;
        _dbContext = dbContext;
        _dataverseAdapter = dataverseAdapter;
        _backgroundJobScheduler = backgroundJobScheduler;
        _clock = clock;
    }

    public async Task Execute(CancellationToken cancellationToken)
    {
        var lastAwardedToUtc = await _dbContext.EytsAwardedEmailsJobs.MaxAsync(j => (DateTime?)j.AwardedToUtc) ??
            _batchSendEytsAwardedEmailsJobOptions.InitialLastAwardedToUtc;

        // Look for new EYTS awards up to the end of the day the configurable amount of days ago to provide a delay between award being given and email being sent.
        var awardedToUtc = _clock.Today.AddDays(-_batchSendEytsAwardedEmailsJobOptions.EmailDelayDays).ToDateTime();

        var executed = _clock.UtcNow;
        var startDate = lastAwardedToUtc;
        var endDate = awardedToUtc;
        var eytsAwardedEmailsJobId = Guid.NewGuid();
        var job = new EytsAwardedEmailsJob
        {
            EytsAwardedEmailsJobId = eytsAwardedEmailsJobId,
            AwardedToUtc = awardedToUtc,
            ExecutedUtc = executed
        };

        using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        _dbContext.EytsAwardedEmailsJobs.Add(job);

        var totalEytsAwardees = 0;
        await foreach (var eytsAwardees in _dataverseAdapter.GetEytsAwardeesForDateRange(startDate, endDate))
        {
            foreach (var eytsAwardee in eytsAwardees)
            {
                if (await _dbContext.EytsAwardedEmailsJobItems.AnyAsync(i => i.Trn == eytsAwardee.Trn))
                {
                    continue;
                }

                var personalization = new Dictionary<string, string>()
                {
                    { "first name", eytsAwardee.FirstName },
                    { "last name", eytsAwardee.LastName },
                };

                var jobItem = new EytsAwardedEmailsJobItem
                {
                    EytsAwardedEmailsJobId = eytsAwardedEmailsJobId,
                    PersonId = eytsAwardee.TeacherId,
                    Trn = eytsAwardee.Trn,
                    EmailAddress = eytsAwardee.EmailAddress,
                    Personalization = personalization
                };

                _dbContext.EytsAwardedEmailsJobItems.Add(jobItem);

                totalEytsAwardees++;
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        if (totalEytsAwardees > 0)
        {
            await _backgroundJobScheduler.Enqueue<EytsAwardedEmailJobDispatcher>(j => j.Execute(eytsAwardedEmailsJobId));
        }

        transaction.Complete();
    }
}
