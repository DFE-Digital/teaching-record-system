using System.Data;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Infrastructure.Json;
using TeachingRecordSystem.Core.Models.SupportTasks;

namespace TeachingRecordSystem.Core.Jobs;

public class BackfillChangeRequestProcessesJob(TrsDbContext dbContext)
{
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        dbContext.Database.SetCommandTimeout(0);

        var txn = await dbContext.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken: cancellationToken);

        var results = await dbContext.Database.SqlQueryRaw<Result>(
            """
            insert into processes (process_id, process_type, created_on, user_id, person_ids, updated_on)
            select
                e.event_id,
                case (e.payload->'SupportTask'->>'SupportTaskType')::int when 2 then 13 when 3 then 12 else null end,
                e.created,
                (e.payload->>'RaisedBy')::uuid,
                e.person_ids,
                e.created
            from events e
            left join process_events pe on e.event_id = pe.process_event_id
            where e.event_name = 'SupportTaskCreatedEvent' and (e.payload->'SupportTask'->>'SupportTaskType')::int in (2, 3)
            and pe.process_event_id is null  --exclude events that will already have a process created
            ;

            insert into process_events (process_event_id, process_id, event_name, payload, person_ids, created_on)
            select
                e.event_id,
                e.event_id,
                'SupportTaskCreatedEvent',
                e.payload - 'CreatedUtc' - 'RaisedBy' || json_build_object('PersonIds', e.person_ids)::jsonb,
                e.person_ids,
                e.created
            from events e
            left join process_events pe on e.event_id = pe.process_event_id
            where e.event_name = 'SupportTaskCreatedEvent' and (e.payload->'SupportTask'->>'SupportTaskType')::int in (2, 3)
            and pe.process_event_id is null  --exclude events that already have a process
            returning process_event_id, process_id, (payload->'SupportTask'->>'SupportTaskType')::int support_task_type, payload->'SupportTask'->'Data'->>'EmailAddress' email_address, person_ids, created_on;
            """).ToListAsync(cancellationToken);

        foreach (var resultsForPerson in results.GroupBy(r => new { PersonId = r.person_ids.Single(), r.support_task_type, r.email_address }))
        {
            var templateId = resultsForPerson.Key.support_task_type == SupportTaskType.ChangeDateOfBirthRequest
                ? EmailTemplateIds.GetAnIdentityChangeOfDateOfBirthSubmittedEmailConfirmation
                : EmailTemplateIds.GetAnIdentityChangeOfNameSubmittedEmailConfirmation;

            var emailAddress = resultsForPerson.Key.email_address;

            var emailsForAddress = await dbContext.Emails.Where(e => e.EmailAddress == emailAddress && e.TemplateId == templateId).ToListAsync(cancellationToken);

            foreach (var e in resultsForPerson)
            {
                var emailsForTemplate = emailsForAddress.Where(m => m.SentOn > e.created_on).OrderBy(m => m.SentOn).ToArray();

                var thisTaskEmail = emailsForTemplate.FirstOrDefault() ??
                    throw new Exception($"No email found for {emailAddress} with template '{templateId}' after {e.created_on}.");

                // if ((thisTaskEmail.SentOn!.Value - e.created_on) > TimeSpan.FromHours(3) && emailsForTemplate.Length > 1)
                // {
                //     throw new InvalidOperationException(
                //         $"Couldn't find matching email for event '{@e.process_event_id}' (nearest is {(thisTaskEmail.SentOn!.Value - e.created_on)}.");
                // }

                emailsForAddress.Remove(thisTaskEmail);

                dbContext.Set<ProcessEvent>().Add(new ProcessEvent
                {
                    ProcessEventId = thisTaskEmail.EmailId,
                    ProcessId = e.process_id,
                    EventName = "EmailSentEvent",
                    Payload = new EmailSentEvent
                    {
                        EventId = thisTaskEmail.EmailId, PersonId = e.person_ids.Single(), Email = EventModels.Email.FromModel(thisTaskEmail)
                    },
                    PersonIds = e.person_ids,
                    CreatedOn = thisTaskEmail.SentOn!.Value
                });
            }

            await dbContext.SaveChangesAsync(cancellationToken);
        }

        await txn.CommitAsync(cancellationToken);
    }

#pragma warning restore IDE1006 // Naming Styles
    private record Result(Guid process_event_id, Guid process_id, SupportTaskType support_task_type, string email_address, Guid[] person_ids, DateTime created_on);
}
