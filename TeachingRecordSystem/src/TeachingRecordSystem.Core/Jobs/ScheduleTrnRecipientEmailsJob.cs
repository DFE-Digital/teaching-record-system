using System.Globalization;
using System.Transactions;
using Microsoft.Extensions.Options;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Jobs.Scheduling;

namespace TeachingRecordSystem.Core.Jobs;

public class ScheduleTrnRecipientEmailsJob(
    IDbContextFactory<TrsDbContext> dbContextFactory,
    IBackgroundJobScheduler backgroundJobScheduler,
    IClock clock,
    IOptions<ScheduleTrnRecipientEmailsJobOptions> optionsAccessor)
{
    private const string JobName = nameof(ScheduleTrnRecipientEmailsJob);

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var options = optionsAccessor.Value;

        await using var outerDbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        var jobMetadata = await outerDbContext.JobMetadata.SingleOrDefaultAsync(m => m.JobName == JobName, cancellationToken);
        if (jobMetadata is null)
        {
            jobMetadata = new JobMetadata { JobName = JobName, Metadata = new Dictionary<string, string>() };
            outerDbContext.JobMetadata.Add(jobMetadata);
        }

        var minCreatedOn = jobMetadata.Metadata.TryGetValue(JobMetadataKeys.LastMaxCreatedOn, out var lastMaxCreatedOnStr) &&
                DateTime.TryParse(lastMaxCreatedOnStr, out var lastMaxCreatedOn)
            ? lastMaxCreatedOn.ToUniversalTime()
            : DateTime.SpecifyKind(options.EarliestRecordCreationDate.ToDateTime(new TimeOnly(0, 0)), DateTimeKind.Utc);

        var maxCreatedOn = clock.Today.AddDays(-options.EmailDelayDays).ToDateTime();

        outerDbContext.Database.SetCommandTimeout(0);

        var trnRecipients = outerDbContext.Database
            .SqlQuery<Result>(
                $"""
                 select p.person_id, p.trn, p.email_address, p.first_name, p.last_name, p.created_on from persons p
                 -- Exclude persons who have already been sent a TRN recipient notification
                 left join processes x on p.person_id = any(x.person_ids) and x.process_type = {(int)ProcessType.NotifyingTrnRecipient}
                 where x.process_id is null
                 and p.source_application_user_id = any({options.RequestedByUserIds})
                 and p.created_on >= {minCreatedOn}
                 and p.created_on < {maxCreatedOn}
                 and p.email_address is not null
                 and p.trn is not null
                 """)
            .ToAsyncEnumerable()
            .ChunkAsync(100);

        await foreach (var chunk in trnRecipients.WithCancellation(cancellationToken))
        {
            using var txn = new TransactionScope(
                TransactionScopeOption.RequiresNew,
                new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
                TransactionScopeAsyncFlowOption.Enabled);

            await using var innerDbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

            foreach (var recipient in chunk)
            {
                var email = new Email
                {
                    EmailId = Guid.NewGuid(),
                    TemplateId = EmailTemplateIds.TraineeTrnRecipient,
                    EmailAddress = recipient.EmailAddress,
                    Personalization =
                        new Dictionary<string, string> { { "TRN", recipient.Trn }, { "name", $"{recipient.FirstName} {recipient.LastName}" } },
                    Metadata = new Dictionary<string, object> { { "PersonId", recipient.PersonId } }
                };

                innerDbContext.Emails.Add(email);

                await innerDbContext.SaveChangesAsync(cancellationToken);

                await backgroundJobScheduler.EnqueueAsync<SendTrnRecipientEmailJob>(j => j.ExecuteAsync(email.EmailId));
            }

            txn.Complete();
        }

        jobMetadata.Metadata[JobMetadataKeys.LastMaxCreatedOn] = maxCreatedOn.ToString("s", CultureInfo.InvariantCulture);
        await outerDbContext.SaveChangesAsync(cancellationToken);
    }

    private record Result(Guid PersonId, string Trn, string EmailAddress, string FirstName, string LastName, DateTime CreatedOn);

    private static class JobMetadataKeys
    {
        public const string LastMaxCreatedOn = "LastMaxCreatedOn";
    }
}
