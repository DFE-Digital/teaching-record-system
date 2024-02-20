using System.Transactions;
using Microsoft.Extensions.Options;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Jobs.Scheduling;

namespace TeachingRecordSystem.Core.Jobs;

public class BatchSendInternationalQtsAwardedEmailsJob
{
    private readonly BatchSendInternationalQtsAwardedEmailsJobOptions _batchSendInternationalQtsAwardedEmailsJobOptions;
    private readonly TrsDbContext _dbContext;
    private readonly IDataverseAdapter _dataverseAdapter;
    private readonly IBackgroundJobScheduler _backgroundJobScheduler;
    private readonly IClock _clock;

    public BatchSendInternationalQtsAwardedEmailsJob(
        IOptions<BatchSendInternationalQtsAwardedEmailsJobOptions> batchSendInternationalQtsAwardedEmailsJobOptions,
        TrsDbContext dbContext,
        IDataverseAdapter dataverseAdapter,
        IBackgroundJobScheduler backgroundJobScheduler,
        IClock clock)
    {
        _batchSendInternationalQtsAwardedEmailsJobOptions = batchSendInternationalQtsAwardedEmailsJobOptions.Value;
        _dbContext = dbContext;
        _dataverseAdapter = dataverseAdapter;
        _backgroundJobScheduler = backgroundJobScheduler;
        _clock = clock;
    }

    public async Task Execute(CancellationToken cancellationToken)
    {
        var lastAwardedToUtc = await _dbContext.InternationalQtsAwardedEmailsJobs.MaxAsync(j => (DateTime?)j.AwardedToUtc) ??
            _batchSendInternationalQtsAwardedEmailsJobOptions.InitialLastAwardedToUtc;

        // Look for new International QTS awards up to the end of the day the configurable amount of days ago to provide a delay between award being given and email being sent.
        var awardedToUtc = _clock.Today.AddDays(-_batchSendInternationalQtsAwardedEmailsJobOptions.EmailDelayDays).ToDateTime();

        var executed = _clock.UtcNow;
        var startDate = lastAwardedToUtc;
        var endDate = awardedToUtc;
        var internationalQtsAwardedEmailsJobId = Guid.NewGuid();
        var job = new InternationalQtsAwardedEmailsJob
        {
            InternationalQtsAwardedEmailsJobId = internationalQtsAwardedEmailsJobId,
            AwardedToUtc = awardedToUtc,
            ExecutedUtc = executed
        };

        using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        _dbContext.InternationalQtsAwardedEmailsJobs.Add(job);

        var totalInternationalQtsAwardees = 0;
        await foreach (var internationalQtsAwardees in _dataverseAdapter.GetInternationalQtsAwardeesForDateRange(startDate, endDate))
        {
            foreach (var internationalQtsAwardee in internationalQtsAwardees)
            {
                if (await _dbContext.InternationalQtsAwardedEmailsJobItems.AnyAsync(i => i.Trn == internationalQtsAwardee.Trn))
                {
                    continue;
                }

                var personalization = new Dictionary<string, string>()
                {
                    { "first name", internationalQtsAwardee.FirstName },
                    { "last name", internationalQtsAwardee.LastName },
                };

                var jobItem = new InternationalQtsAwardedEmailsJobItem
                {
                    InternationalQtsAwardedEmailsJobId = internationalQtsAwardedEmailsJobId,
                    PersonId = internationalQtsAwardee.TeacherId,
                    Trn = internationalQtsAwardee.Trn,
                    EmailAddress = internationalQtsAwardee.EmailAddress,
                    Personalization = personalization
                };

                _dbContext.InternationalQtsAwardedEmailsJobItems.Add(jobItem);

                totalInternationalQtsAwardees++;
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        if (totalInternationalQtsAwardees > 0)
        {
            await _backgroundJobScheduler.Enqueue<InternationalQtsAwardedEmailJobDispatcher>(j => j.Execute(internationalQtsAwardedEmailsJobId));
        }

        transaction.Complete();
    }
}
