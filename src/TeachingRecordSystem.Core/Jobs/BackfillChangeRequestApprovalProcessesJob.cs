using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models.SupportTasks;
using LegacyChangeDateOfBirthRequestSupportTaskApprovedEvent = TeachingRecordSystem.Core.Events.Legacy.ChangeDateOfBirthRequestSupportTaskApprovedEvent;
using LegacyChangeNameRequestSupportTaskApprovedEvent = TeachingRecordSystem.Core.Events.Legacy.ChangeNameRequestSupportTaskApprovedEvent;
using Process = TeachingRecordSystem.Core.DataStore.Postgres.Models.Process;

namespace TeachingRecordSystem.Core.Jobs;

/// <summary>
/// Back-fills <see cref="Process"/> and <see cref="ProcessEvent"/> records from the legacy change
/// request approved events stored in the <c>events</c> table.
/// </summary>
public class BackfillChangeRequestApprovalProcessesJob(TrsDbContext dbContext)
{
    // These match the EventName values stored in the events table for the legacy events.
    private static readonly string[] _legacyEventNames =
    [
        typeof(LegacyChangeNameRequestSupportTaskApprovedEvent).Name,
        typeof(LegacyChangeDateOfBirthRequestSupportTaskApprovedEvent).Name
    ];

    public async Task ExecuteAsync(bool dryRun, CancellationToken cancellationToken)
    {
        dbContext.Database.SetCommandTimeout(0);

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        // Only migrate events that haven't already been back-filled so the job is idempotent.
        var legacyEvents = await dbContext.Events
            .Where(e => _legacyEventNames.Contains(e.EventName))
            .Where(e => !dbContext.ProcessEvents.Any(pe => pe.ProcessEventId == e.EventId))
            .OrderBy(e => e.Created)
            .ToListAsync(cancellationToken);

        foreach (var legacyEvent in legacyEvents)
        {
            var eventData = legacyEvent.ToEventBase();

            var (processType, emailTemplateId) = eventData switch
            {
                LegacyChangeNameRequestSupportTaskApprovedEvent =>
                    (ProcessType.ChangeOfNameRequestApproving, EmailTemplateIds.GetAnIdentityChangeOfNameApprovedEmailConfirmation),
                LegacyChangeDateOfBirthRequestSupportTaskApprovedEvent =>
                    (ProcessType.ChangeOfDateOfBirthRequestApproving, EmailTemplateIds.GetAnIdentityChangeOfDateOfBirthApprovedEmailConfirmation),
                _ => throw new InvalidOperationException($"Unexpected event type: '{eventData.GetType().Name}'.")
            };

            var (personId, requestEmailAddress, changes, personAttributes, oldPersonAttributes) = eventData switch
            {
                LegacyChangeNameRequestSupportTaskApprovedEvent approved => (
                    approved.PersonId,
                    approved.RequestData.EmailAddress,
                    (int)approved.Changes,
                    approved.PersonAttributes,
                    approved.OldPersonAttributes),
                LegacyChangeDateOfBirthRequestSupportTaskApprovedEvent approved => (
                    approved.PersonId,
                    approved.RequestData.EmailAddress,
                    (int)approved.Changes,
                    approved.PersonAttributes,
                    approved.OldPersonAttributes),
                _ => throw new InvalidOperationException($"Unexpected event type: '{eventData.GetType().Name}'.")
            };

            var legacySupportTaskEvent = (LegacyEvents.SupportTaskUpdatedEvent)eventData;

            // The approval closes the support task (Status) and stamps the outcome onto its data (Data).
            var supportTaskUpdatedEvent = new SupportTaskUpdatedEvent
            {
                EventId = legacyEvent.EventId,
                SupportTaskReference = legacySupportTaskEvent.SupportTask.SupportTaskReference,
                Changes = SupportTaskUpdatedEventChanges.Status | SupportTaskUpdatedEventChanges.Data,
                SupportTask = legacySupportTaskEvent.SupportTask,
                OldSupportTask = legacySupportTaskEvent.OldSupportTask,
                Comments = null,
                RejectionReason = null
            };

            // The legacy events carry the person attribute changes shifted up by 16 bits; see PersonAttributesChanges.
            var personDetailsUpdatedEvent = new PersonDetailsUpdatedEvent
            {
                EventId = Guid.NewGuid(),
                PersonId = personId,
                PersonDetails = personAttributes,
                OldPersonDetails = oldPersonAttributes,
                Changes = (PersonDetailsUpdatedEventChanges)(changes >> 16)
            };

            // We have no record of whether the confirmation email was actually sent for these approvals,
            // so we assume one was sent to the address the change was requested from, falling back to the
            // address on the record. There's no corresponding row in the emails table.
            var emailAddress = !string.IsNullOrEmpty(requestEmailAddress)
                ? requestEmailAddress
                : personAttributes.EmailAddress ?? string.Empty;

            var emailSentEvent = new EmailSentEvent
            {
                EventId = Guid.NewGuid(),
                PersonId = personId,
                Email = new EventModels.Email
                {
                    EmailId = Guid.NewGuid(),
                    TemplateId = emailTemplateId,
                    EmailAddress = emailAddress,
                    Personalization = new Dictionary<string, string>
                    {
                        { ChangeRequestEmailConstants.FirstNameEmailPersonalisationKey, personAttributes.FirstName }
                    },
                    Metadata = new Dictionary<string, object>(),
                    SentOn = legacyEvent.Created,
                    EmailReplyToId = null
                }
            };

            CreateProcessAndProcessEvents(
                legacyEvent,
                processType,
                [supportTaskUpdatedEvent, personDetailsUpdatedEvent, emailSentEvent]);

            await dbContext.SaveChangesAsync(cancellationToken);
        }

        if (dryRun)
        {
            await transaction.RollbackAsync(cancellationToken);
        }
        else
        {
            await transaction.CommitAsync(cancellationToken);
        }
    }

    private void CreateProcessAndProcessEvents(Event legacyEvent, ProcessType processType, IEvent[] newEvents)
    {
        var legacyEventData = legacyEvent.ToEventBase();
        var processId = Guid.NewGuid();

        var process = new Process
        {
            ProcessId = processId,
            ProcessType = processType,
            CreatedOn = legacyEvent.Created,
            UpdatedOn = legacyEvent.Created,
            UserId = legacyEventData.RaisedBy.UserId,
            DqtUserId = legacyEventData.RaisedBy.DqtUserId,
            DqtUserName = legacyEventData.RaisedBy.DqtUserName,
            PersonIds = [.. newEvents.SelectMany(e => e.PersonIds).Distinct()],
            OneLoginUserSubjects = [.. newEvents.SelectMany(e => e.OneLoginUserSubjects).Distinct()],
            SupportTaskReferences = [.. newEvents.SelectMany(e => e.SupportTaskReferences).Distinct()],
            ChangeReason = null
        };

        dbContext.Processes.Add(process);

        foreach (var newEvent in newEvents)
        {
            dbContext.ProcessEvents.Add(new ProcessEvent
            {
                ProcessEventId = newEvent.EventId,
                ProcessId = processId,
                EventName = newEvent.GetType().Name,
                Payload = newEvent,
                PersonIds = newEvent.PersonIds,
                OneLoginUserSubjects = newEvent.OneLoginUserSubjects,
                SupportTaskReferences = newEvent.SupportTaskReferences,
                CreatedOn = legacyEvent.Created
            });
        }
    }
}
