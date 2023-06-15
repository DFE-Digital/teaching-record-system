using System.Transactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TeachingRecordSystem.Api.DataStore.Crm;
using TeachingRecordSystem.Api.DataStore.Sql;
using TeachingRecordSystem.Api.DataStore.Sql.Models;
using TeachingRecordSystem.Api.Jobs.Scheduling;

namespace TeachingRecordSystem.Api.Jobs;

public class BatchSendEytsAwardedEmailsJob
{
    private readonly BatchSendEytsAwardedEmailsJobOptions _batchSendEytsAwardedEmailsJobOptions;
    private readonly DqtContext _dbContext;
    private readonly IDataverseAdapter _dataverseAdapter;
    private readonly IBackgroundJobScheduler _backgroundJobScheduler;
    private readonly IClock _clock;

    public BatchSendEytsAwardedEmailsJob(
        IOptions<BatchSendEytsAwardedEmailsJobOptions> batchSendEytsAwardedEmailsJobOptions,
        DqtContext dbContext,
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
        var lastAwardedToUtc = _batchSendEytsAwardedEmailsJobOptions.InitialLastAwardedToUtc;
        var lastExecutedJob = await _dbContext.EytsAwardedEmailsJobs.OrderBy(j => j.ExecutedUtc).LastOrDefaultAsync();
        if (lastExecutedJob != null)
        {
            lastAwardedToUtc = lastExecutedJob.AwardedToUtc;
        }

        // Look for new QTS awards up to the end of the day the configurable amount of days ago to provide a delay between award being given and email being sent.
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

        await _dbContext.EytsAwardedEmailsJobs.AddAsync(job, cancellationToken);

        var eytsAwardees = await _dataverseAdapter.GetEytsAwardeesForDateRange(startDate, endDate);
        foreach (var eytsAwardee in eytsAwardees)
        {
            var personalisation = new Dictionary<string, string>()
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
                Personalization = personalisation
            };

            await _dbContext.EytsAwardedEmailsJobItems.AddAsync(jobItem, cancellationToken);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        if (eytsAwardees.Length > 0)
        {
            await _backgroundJobScheduler.Enqueue<EytsAwardedEmailJobDispatcher>(j => j.Execute(eytsAwardedEmailsJobId));
        }

        transaction.Complete();
    }
}
