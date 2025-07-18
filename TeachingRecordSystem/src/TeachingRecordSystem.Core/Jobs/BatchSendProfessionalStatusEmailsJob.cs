using System.Transactions;
using Microsoft.Extensions.Options;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Jobs.Scheduling;

namespace TeachingRecordSystem.Core.Jobs;

public class BatchSendProfessionalStatusEmailsJob(
    IOptions<BatchSendProfessionalStatusEmailsOptions> jobOptionsAccessor,
    TrsDbContext dbContext,
    IBackgroundJobScheduler backgroundJobScheduler,
    IClock clock)
{
    private readonly BatchSendProfessionalStatusEmailsOptions _batchSendQtsAwardedEmailsJobOptions = jobOptionsAccessor.Value;

    private static class MetadataKeys
    {
        public const string LastHoldsFromEnd = "LastHoldsFromEnd";
    }

    public static class TemplateIds
    {
        public const string EytsAwardedEmailConfirmationTemplateId = "f85babdb-049b-4f32-9579-2a812acc0a2b";
        public const string InternationalQtsAwardedEmailConfirmationTemplateId = "f4200027-de67-4a55-808a-b37ae2653660";
        public const string QtsAwardedEmailConfirmationTemplateId = "68814f63-b63a-4f79-b7df-c52f5cd55710";
        public const string QtlsLapsedTemplateId = "269cefcc-71ac-4ca0-b348-719d4ee6d9d2";
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var jobName = nameof(BatchSendProfessionalStatusEmailsJob);

        var jobMetadata = await dbContext.JobMetadata
            .FromSql($"select * from job_metadata where job_name = {jobName} for update")
            .SingleOrDefaultAsync(cancellationToken);

        if (jobMetadata is null)
        {
            jobMetadata = new JobMetadata { JobName = jobName, Metadata = new Dictionary<string, object>() };
            dbContext.JobMetadata.Add(jobMetadata);
        }

        if (!jobMetadata.Metadata.TryGetValue(MetadataKeys.LastHoldsFromEnd, out var lastHoldsFromEndObj) ||
            lastHoldsFromEndObj is not DateTime lastHoldsFromEnd)
        {
            lastHoldsFromEnd = DateTime.SpecifyKind(_batchSendQtsAwardedEmailsJobOptions.InitialLastHoldsFromEndUtc, DateTimeKind.Utc);
        }

        var start = lastHoldsFromEnd;
        var end = clock.Today.AddDays(-_batchSendQtsAwardedEmailsJobOptions.EmailDelayDays).ToDateTime();

        var eventNames = EventBase.GetEventNamesForBaseType(typeof(IEventWithRouteToProfessionalStatus))
            .Except([nameof(RouteToProfessionalStatusMigratedEvent)])
            .ToArray();

        // Ensure enqueued Hangfire jobs are run in the same transaction as the database changes
        using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        await ProcessQtsAwardeesAsync();
        await ProcessEytsAwardeesAsync();
        await ProcessQtlsLosersAsync();

        jobMetadata.Metadata[MetadataKeys.LastHoldsFromEnd] = end;
        await dbContext.SaveChangesAsync(cancellationToken);

        transaction.Complete();

        async Task ProcessQtsAwardeesAsync()
        {
            var qtsAwardees = await dbContext.Database.SqlQuery<QtsAwardeeQueryResult>(
                    $"""
                     select
                         person_id,
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
                .Where(p => p.Trn != null && p.EmailAddress != null)
                .ToArrayAsync(cancellationToken: cancellationToken);

            foreach (var qtsAwardee in qtsAwardees.DistinctBy(p => p.Trn))
            {
                var templateId = qtsAwardee.RouteToProfessionalStatusTypeId == RouteToProfessionalStatusType.InternationalQualifiedTeacherStatusId
                    ? TemplateIds.InternationalQtsAwardedEmailConfirmationTemplateId
                    : TemplateIds.QtsAwardedEmailConfirmationTemplateId;

                var personalization = new Dictionary<string, string>
                {
                    ["first name"] = qtsAwardee.FirstName,
                    ["last name"] = qtsAwardee.LastName
                };

                var metadata = new Dictionary<string, object> { [SendAytqInviteEmailJob.JobMetadataKeys.Trn] = qtsAwardee.Trn! };

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
                     select person_id from events
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
                .Where(p => p.Trn != null && p.EmailAddress != null)
                .ToArrayAsync(cancellationToken: cancellationToken);

            foreach (var eytsAwardee in eytsAwardees.DistinctBy(p => p.Trn))
            {
                var personalization = new Dictionary<string, string>
                {
                    ["first name"] = eytsAwardee.FirstName,
                    ["last name"] = eytsAwardee.LastName
                };

                var metadata = new Dictionary<string, object> { [SendAytqInviteEmailJob.JobMetadataKeys.Trn] = eytsAwardee.Trn! };

                var email = new Email
                {
                    EmailId = Guid.NewGuid(),
                    TemplateId = TemplateIds.EytsAwardedEmailConfirmationTemplateId,
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
                     select person_id from events
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
                .Where(p => p.Trn != null && p.EmailAddress != null)
                .ToArrayAsync(cancellationToken: cancellationToken);

            foreach (var qtlsLoser in qtlsLosers.DistinctBy(p => p.Trn))
            {
                var email = new Email
                {
                    EmailId = Guid.NewGuid(),
                    TemplateId = TemplateIds.QtlsLapsedTemplateId,
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
