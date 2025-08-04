using System.Transactions;
using Microsoft.Extensions.Options;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Jobs.Scheduling;

namespace TeachingRecordSystem.Core.Jobs;

public class BatchSendInductionCompletedEmailsJob(
    IOptions<BatchSendInductionCompletedEmailsJobOptions> jobOptionsAccessor,
    TrsDbContext dbContext,
    IBackgroundJobScheduler backgroundJobScheduler,
    IClock clock)
{
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var lastPassedEndUtc = await dbContext.InductionCompletedEmailsJobs.MaxAsync(j => (DateTime?)j.PassedEndUtc) ??
            jobOptionsAccessor.Value.InitialLastPassedEndUtc;

        // Look for new induction awards up to the end of the day the configurable amount of days ago to provide a delay between award being given and email being sent.
        var passedEndUtc = clock.Today.AddDays(-(jobOptionsAccessor.Value.EmailDelayDays + 1)).ToDateTime();

        var executed = clock.UtcNow;
        var startDate = lastPassedEndUtc;
        var endDate = passedEndUtc;
        var inductionCompletedEmailsJobId = Guid.NewGuid();

        var job = new InductionCompletedEmailsJob
        {
            InductionCompletedEmailsJobId = inductionCompletedEmailsJobId,
            PassedEndUtc = passedEndUtc,
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
             """)
            .Join(dbContext.Persons, e => e.PersonIds.First(), p => p.PersonId, (e, p) => p)
            .Where(p => p.InductionStatus == InductionStatus.Passed)  // Check the status is still Passed
            .Where(p => p.Trn != null && p.EmailAddress != null)
            .Where(p => !dbContext.InductionCompletedEmailsJobItems.Any(i => i.Trn == p.Trn))  // Ensure we haven't already processed this TRN
            .Select(p => new { p.PersonId, Trn = p.Trn!, EmailAddress = p.EmailAddress!, p.FirstName, p.LastName })
            .ToArrayAsync(cancellationToken: cancellationToken);

        // Ensure enqueued Hangfire jobs are run in the same transaction as the database changes
        using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        dbContext.InductionCompletedEmailsJobs.Add(job);

        foreach (var inductionCompletee in inductionCompletees.DistinctBy(p => p.Trn))
        {
            var personalization = new Dictionary<string, string>()
            {
                { "first name", inductionCompletee.FirstName },
                { "last name", inductionCompletee.LastName }
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

            await backgroundJobScheduler.EnqueueAsync<SendInductionCompletedEmailJob>(j => j.ExecuteAsync(inductionCompletedEmailsJobId, jobItem.PersonId));
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        transaction.Complete();
    }
}
