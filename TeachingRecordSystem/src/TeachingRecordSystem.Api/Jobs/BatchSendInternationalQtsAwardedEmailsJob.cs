using System.Transactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TeachingRecordSystem.Api.Jobs.Scheduling;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Dqt;

namespace TeachingRecordSystem.Api.Jobs;

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
        var lastAwardedToUtc = _batchSendInternationalQtsAwardedEmailsJobOptions.InitialLastAwardedToUtc;
        var lastExecutedJob = await _dbContext.InternationalQtsAwardedEmailsJobs.OrderBy(j => j.ExecutedUtc).LastOrDefaultAsync();
        if (lastExecutedJob != null)
        {
            lastAwardedToUtc = lastExecutedJob.AwardedToUtc;
        }

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

        await _dbContext.InternationalQtsAwardedEmailsJobs.AddAsync(job, cancellationToken);

        var internationalQtsAwardees = await _dataverseAdapter.GetInternationalQtsAwardeesForDateRange(startDate, endDate);
        foreach (var internationalQtsAwardee in internationalQtsAwardees)
        {
            var personalisation = new Dictionary<string, string>()
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
                Personalization = personalisation
            };

            await _dbContext.InternationalQtsAwardedEmailsJobItems.AddAsync(jobItem, cancellationToken);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        if (internationalQtsAwardees.Length > 0)
        {
            await _backgroundJobScheduler.Enqueue<InternationalQtsAwardedEmailJobDispatcher>(j => j.Execute(internationalQtsAwardedEmailsJobId));
        }

        transaction.Complete();
    }
}
