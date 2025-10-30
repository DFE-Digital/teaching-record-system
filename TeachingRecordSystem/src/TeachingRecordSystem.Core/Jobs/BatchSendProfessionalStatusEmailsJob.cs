using System.Globalization;
using System.Transactions;
using Microsoft.Extensions.Options;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Events.Legacy;
using TeachingRecordSystem.Core.Jobs.Scheduling;

namespace TeachingRecordSystem.Core.Jobs;

public class BatchSendProfessionalStatusEmailsJob(
    IOptions<BatchSendProfessionalStatusEmailsOptions> jobOptionsAccessor,
    IDbContextFactory<TrsDbContext> dbContextFactory,
    IBackgroundJobScheduler backgroundJobScheduler,
    IClock clock)
{
    private readonly BatchSendProfessionalStatusEmailsOptions _batchSendQtsAwardedEmailsJobOptions = jobOptionsAccessor.Value;

    private static class MetadataKeys
    {
        public const string LastHoldsFromEnd = "LastHoldsFromEnd";
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        // Ensure enqueued Hangfire jobs are run in the same transaction as the database changes
        using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        var jobName = nameof(BatchSendProfessionalStatusEmailsJob);

        var jobMetadata = await dbContext.JobMetadata
            .FromSql($"select * from job_metadata where job_name = {jobName} for update")
            .SingleOrDefaultAsync(cancellationToken);

        if (jobMetadata is null)
        {
            jobMetadata = new JobMetadata { JobName = jobName, Metadata = new Dictionary<string, string>() };
            dbContext.JobMetadata.Add(jobMetadata);
        }

        var start = jobMetadata.Metadata.TryGetValue(MetadataKeys.LastHoldsFromEnd, out var lastHoldsFromEndStr) ?
            DateTime.Parse(lastHoldsFromEndStr).ToUniversalTime() :
            DateTime.SpecifyKind(_batchSendQtsAwardedEmailsJobOptions.InitialLastHoldsFromEndUtc, DateTimeKind.Utc);

        var end = clock.Today.AddDays(-_batchSendQtsAwardedEmailsJobOptions.EmailDelayDays).ToDateTime();

        var eventNames = EventBase.GetEventNamesForBaseType(typeof(IEventWithRouteToProfessionalStatus))
            .Except([nameof(RouteToProfessionalStatusMigratedEvent)])
            .ToArray();

        await ProcessQtsAwardeesAsync();
        await ProcessEytsAwardeesAsync();
        await ProcessQtlsLosersAsync();

        jobMetadata.Metadata[MetadataKeys.LastHoldsFromEnd] = end.ToString("s", CultureInfo.InvariantCulture);
        await dbContext.SaveChangesAsync(cancellationToken);

        transaction.Complete();

        async Task ProcessQtsAwardeesAsync()
        {
            var qtsAwardees = await dbContext.Database.SqlQuery<QtsAwardeeQueryResult>(
                    $"""
                     select
                         person_ids[1] as person_id,
                         (payload->'RouteToProfessionalStatus'->>'RouteToProfessionalStatusTypeId')::uuid route_to_professional_status_type_id
                     from events
                     where event_name = any({eventNames})
                     and created >= {start}
                     and created < {end}
                     and (payload->>'RaisedBy')::uuid = any({jobOptionsAccessor.Value.RaisedByUserIds})
                     and payload->'PersonAttributes'->>'QtsDate' is not null
                     and payload->'OldPersonAttributes'->>'QtsDate' is null
                     """)
                .Join(
                    dbContext.Persons,
                    e => e.person_id, p => p.PersonId,
                    (e, p) => new
                    {
                        p.PersonId,
                        p.Trn,
                        p.EmailAddress,
                        p.FirstName,
                        p.LastName,
                        p.QtsDate,
                        RouteToProfessionalStatusTypeId = e.route_to_professional_status_type_id
                    })
                .Where(p => p.QtsDate != null) // Check the person still has QTS
                .Where(p => p.EmailAddress != null)
                .ToArrayAsync(cancellationToken: cancellationToken);

            foreach (var qtsAwardee in qtsAwardees.DistinctBy(p => p.Trn))
            {
                var templateId = qtsAwardee.RouteToProfessionalStatusTypeId == RouteToProfessionalStatusType.InternationalQualifiedTeacherStatusId
                    ? EmailTemplateIds.InternationalQtsAwardedEmailConfirmation
                    : EmailTemplateIds.QtsAwardedEmailConfirmation;

                var personalization = new Dictionary<string, string>
                {
                    ["first name"] = qtsAwardee.FirstName,
                    ["last name"] = qtsAwardee.LastName
                };

                var metadata = new Dictionary<string, object> { [SendAytqInviteEmailJob.JobMetadataKeys.Trn] = qtsAwardee.Trn };

                var email = new Email
                {
                    EmailId = Guid.NewGuid(),
                    TemplateId = templateId,
                    EmailAddress = qtsAwardee.EmailAddress!,
                    Personalization = personalization,
                    Metadata = metadata
                };

                dbContext.Emails.Add(email);

                await backgroundJobScheduler.EnqueueAsync<SendAytqInviteEmailJob>(j => j.ExecuteAsync(email.EmailId));
            }

            await dbContext.SaveChangesAsync(cancellationToken);
        }

        async Task ProcessEytsAwardeesAsync()
        {
            var eytsAwardees = await dbContext.Database.SqlQuery<EytsAwardeeQueryResult>(
                    $"""
                     select person_ids[1] as person_id from events
                     where event_name = any({eventNames})
                     and created >= {start}
                     and created < {end}
                     and (payload->>'RaisedBy')::uuid = any({jobOptionsAccessor.Value.RaisedByUserIds})
                     and payload->'PersonAttributes'->>'EytsDate' is not null
                     and payload->'OldPersonAttributes'->>'EytsDate' is null
                     """)
                .Join(
                    dbContext.Persons,
                    e => e.person_id, p => p.PersonId,
                    (e, p) => new
                    {
                        p.PersonId,
                        p.Trn,
                        p.EmailAddress,
                        p.FirstName,
                        p.LastName,
                        p.EytsDate
                    })
                .Where(p => p.EytsDate != null) // Check the person still has EYTS
                .Where(p => p.EmailAddress != null)
                .ToArrayAsync(cancellationToken: cancellationToken);

            foreach (var eytsAwardee in eytsAwardees.DistinctBy(p => p.Trn))
            {
                var personalization = new Dictionary<string, string>
                {
                    ["first name"] = eytsAwardee.FirstName,
                    ["last name"] = eytsAwardee.LastName
                };

                var metadata = new Dictionary<string, object> { [SendAytqInviteEmailJob.JobMetadataKeys.Trn] = eytsAwardee.Trn };

                var email = new Email
                {
                    EmailId = Guid.NewGuid(),
                    TemplateId = EmailTemplateIds.EytsAwardedEmailConfirmation,
                    EmailAddress = eytsAwardee.EmailAddress!,
                    Personalization = personalization,
                    Metadata = metadata
                };

                dbContext.Emails.Add(email);

                await backgroundJobScheduler.EnqueueAsync<SendAytqInviteEmailJob>(j => j.ExecuteAsync(email.EmailId));
            }

            await dbContext.SaveChangesAsync(cancellationToken);
        }

        async Task ProcessQtlsLosersAsync()
        {
            var qtlsLosers = await dbContext.Database.SqlQuery<QtlsLoserQueryResult>(
                    $"""
                     select person_ids[1] as person_id from events
                     where event_name = any({eventNames})
                     and created >= {start}
                     and created < {end}
                     and (payload->>'RaisedBy')::uuid = any({jobOptionsAccessor.Value.RaisedByUserIds})
                     and payload->'PersonAttributes'->>'QtlsStatus' = '1' --Expired
                     and payload->'OldPersonAttributes'->>'QtlsStatus' = '2' --Active
                     """)
                .Join(
                    dbContext.Persons,
                    e => e.person_id, p => p.PersonId,
                    (e, p) => new
                    {
                        p.PersonId,
                        p.Trn,
                        p.EmailAddress,
                        p.FirstName,
                        p.LastName,
                        p.QtlsStatus
                    })
                .Where(p => p.QtlsStatus == QtlsStatus.Expired)
                .Where(p => p.EmailAddress != null)
                .ToArrayAsync(cancellationToken: cancellationToken);

            foreach (var qtlsLoser in qtlsLosers.DistinctBy(p => p.Trn))
            {
                var email = new Email
                {
                    EmailId = Guid.NewGuid(),
                    TemplateId = EmailTemplateIds.QtlsLapsed,
                    EmailAddress = qtlsLoser.EmailAddress!,
                    Personalization = new Dictionary<string, string>()
                };

                dbContext.Emails.Add(email);

                await backgroundJobScheduler.EnqueueAsync<SendEmailJob>(j => j.ExecuteAsync(email.EmailId));
            }

            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }

#pragma warning disable IDE1006 // Naming Styles
    private record EytsAwardeeQueryResult(Guid person_id);

    private record QtlsLoserQueryResult(Guid person_id);

    private record QtsAwardeeQueryResult(Guid person_id, Guid route_to_professional_status_type_id);
#pragma warning restore IDE1006 // Naming Styles
}
