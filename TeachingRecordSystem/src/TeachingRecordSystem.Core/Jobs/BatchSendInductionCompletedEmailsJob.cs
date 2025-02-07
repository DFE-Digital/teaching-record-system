using System.Transactions;
using Microsoft.Extensions.Options;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Jobs.Scheduling;

namespace TeachingRecordSystem.Core.Jobs;

public class BatchSendInductionCompletedEmailsJob(
    IOptions<BatchSendInductionCompletedEmailsJobOptions> batchSendInductionCompletedEmailsJobOptions,
    TrsDbContext dbContext,
    IBackgroundJobScheduler backgroundJobScheduler,
    IClock clock)
{
    private readonly BatchSendInductionCompletedEmailsJobOptions _batchSendInductionCompletedEmailsJobOptions = batchSendInductionCompletedEmailsJobOptions.Value;

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var lastAwardedToUtc = await dbContext.InductionCompletedEmailsJobs.MaxAsync(j => (DateTime?)j.AwardedToUtc) ??
            _batchSendInductionCompletedEmailsJobOptions.InitialLastAwardedToUtc;

        // Look for new induction awards up to the end of the day the configurable amount of days ago to provide a delay between award being given and email being sent.
        var awardedToUtc = clock.Today.AddDays(-(_batchSendInductionCompletedEmailsJobOptions.EmailDelayDays + 1)).ToDateTime();

        var executed = clock.UtcNow;
        var startDate = lastAwardedToUtc;
        var endDate = awardedToUtc;
        var inductionCompletedEmailsJobId = Guid.NewGuid();
        var job = new InductionCompletedEmailsJob
        {
            InductionCompletedEmailsJobId = inductionCompletedEmailsJobId,
            AwardedToUtc = awardedToUtc,
            ExecutedUtc = executed
        };

        var inductionCompletees = await dbContext.Events.FromSql(
            $"""
             select * from events
             where event_name = 'PersonInductionUpdatedEvent'
             and created >= {startDate}
             and created < {endDate}
             and payload->'Induction'->>'Status' = '4'
             and payload->'OldInduction'->>'Status' != '4'
             union
             select * from events
             where event_name = 'DqtInductionUpdatedEvent'
             and created >= {startDate}
             and created < {endDate}
             and payload->'Induction'->>'InductionStatus' = 'Pass'
             and payload->'OldInduction'->>'InductionStatus' != 'Pass'
             """)
            .Join(dbContext.Persons, e => e.PersonId, p => p.PersonId, (e, p) => p)
            .ToArrayAsync();

        using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        dbContext.InductionCompletedEmailsJobs.Add(job);

        var totalInductionCompletees = 0;
        foreach (var inductionCompletee in inductionCompletees)
        {
            if (await dbContext.InductionCompletedEmailsJobItems.AnyAsync(i => i.Trn == inductionCompletee.Trn))
            {
                continue;
            }

            if (inductionCompletee.Trn is null || inductionCompletee.EmailAddress is null)
            {
                continue;
            }

            var personalization = new Dictionary<string, string>()
            {
                { "first name", inductionCompletee.FirstName },
                { "last name", inductionCompletee.LastName },
            };

            var jobItem = new InductionCompletedEmailsJobItem
            {
                InductionCompletedEmailsJobId = inductionCompletedEmailsJobId,
                PersonId = inductionCompletee.PersonId,
                Trn = inductionCompletee.Trn,
                EmailAddress = inductionCompletee.EmailAddress,
                Personalization = personalization
            };

            dbContext.InductionCompletedEmailsJobItems.Add(jobItem);

            totalInductionCompletees++;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        if (totalInductionCompletees > 0)
        {
            await backgroundJobScheduler.EnqueueAsync<InductionCompletedEmailJobDispatcher>(j => j.ExecuteAsync(inductionCompletedEmailsJobId));
        }

        transaction.Complete();
    }
}
