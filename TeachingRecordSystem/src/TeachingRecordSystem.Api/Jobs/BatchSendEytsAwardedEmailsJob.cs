﻿using System.Transactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TeachingRecordSystem.Api.Jobs.Scheduling;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Dqt;

namespace TeachingRecordSystem.Api.Jobs;

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

        var totalEytsAwardees = 0;
        await foreach (var eytsAwardees in _dataverseAdapter.GetEytsAwardeesForDateRange(startDate, endDate))
        {
            totalEytsAwardees += eytsAwardees.Length;

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
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        if (totalEytsAwardees > 0)
        {
            await _backgroundJobScheduler.Enqueue<EytsAwardedEmailJobDispatcher>(j => j.Execute(eytsAwardedEmailsJobId));
        }

        transaction.Complete();
    }
}
