using System.Transactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TeachingRecordSystem.Api.Jobs.Scheduling;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Dqt;

namespace TeachingRecordSystem.Api.Jobs;

public class BatchSendInductionCompletedEmailsJob
{
    private readonly BatchSendInductionCompletedEmailsJobOptions _batchSendInductionCompletedEmailsJobOptions;
    private readonly TrsDbContext _dbContext;
    private readonly IDataverseAdapter _dataverseAdapter;
    private readonly IBackgroundJobScheduler _backgroundJobScheduler;
    private readonly IClock _clock;

    public BatchSendInductionCompletedEmailsJob(
        IOptions<BatchSendInductionCompletedEmailsJobOptions> batchSendInductionCompletedEmailsJobOptions,
        TrsDbContext dbContext,
        IDataverseAdapter dataverseAdapter,
        IBackgroundJobScheduler backgroundJobScheduler,
        IClock clock)
    {
        _batchSendInductionCompletedEmailsJobOptions = batchSendInductionCompletedEmailsJobOptions.Value;
        _dbContext = dbContext;
        _dataverseAdapter = dataverseAdapter;
        _backgroundJobScheduler = backgroundJobScheduler;
        _clock = clock;
    }

    public async Task Execute(CancellationToken cancellationToken)
    {
        var lastAwardedToUtc = _batchSendInductionCompletedEmailsJobOptions.InitialLastAwardedToUtc;
        var lastExecutedJob = await _dbContext.InductionCompletedEmailsJobs.OrderBy(j => j.ExecutedUtc).LastOrDefaultAsync();
        if (lastExecutedJob != null)
        {
            lastAwardedToUtc = lastExecutedJob.AwardedToUtc;
        }

        // Look for new QTS awards up to the end of the day the configurable amount of days ago to provide a delay between award being given and email being sent.
        var awardedToUtc = _clock.Today.AddDays(-_batchSendInductionCompletedEmailsJobOptions.EmailDelayDays).ToDateTime();

        var executed = _clock.UtcNow;
        var startDate = lastAwardedToUtc;
        var endDate = awardedToUtc;
        var inductionCompletedEmailsJobId = Guid.NewGuid();
        var job = new InductionCompletedEmailsJob
        {
            InductionCompletedEmailsJobId = inductionCompletedEmailsJobId,
            AwardedToUtc = awardedToUtc,
            ExecutedUtc = executed
        };

        using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        await _dbContext.InductionCompletedEmailsJobs.AddAsync(job, cancellationToken);

        var totalInductionCompletees = 0;
        await foreach (var inductionCompletees in _dataverseAdapter.GetInductionCompleteesForDateRange(startDate, endDate))
        {
            totalInductionCompletees += inductionCompletees.Length;

            foreach (var inductionCompletee in inductionCompletees)
            {
                var personalisation = new Dictionary<string, string>()
                {
                    { "first name", inductionCompletee.FirstName },
                    { "last name", inductionCompletee.LastName },
                };

                var jobItem = new InductionCompletedEmailsJobItem
                {
                    InductionCompletedEmailsJobId = inductionCompletedEmailsJobId,
                    PersonId = inductionCompletee.TeacherId,
                    Trn = inductionCompletee.Trn,
                    EmailAddress = inductionCompletee.EmailAddress,
                    Personalization = personalisation
                };

                await _dbContext.InductionCompletedEmailsJobItems.AddAsync(jobItem, cancellationToken);
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        if (totalInductionCompletees > 0)
        {
            await _backgroundJobScheduler.Enqueue<InductionCompletedEmailJobDispatcher>(j => j.Execute(inductionCompletedEmailsJobId));
        }

        transaction.Complete();
    }
}
