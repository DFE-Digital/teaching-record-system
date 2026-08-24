using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models.SupportTasks;
using LegacyChangeDateOfBirthRequestSupportTaskApprovedEvent = TeachingRecordSystem.Core.Events.Legacy.ChangeDateOfBirthRequestSupportTaskApprovedEvent;
using LegacyChangeNameRequestSupportTaskApprovedEvent = TeachingRecordSystem.Core.Events.Legacy.ChangeNameRequestSupportTaskApprovedEvent;
using Process = TeachingRecordSystem.Core.DataStore.Postgres.Models.Process;

namespace TeachingRecordSystem.Core.Jobs;

/// <summary>
/// Back-fills <see cref="Process"/> and <see cref="ProcessEvent"/> records from the legacy change
/// request approved events stored in the <c>events</c> table. Every approval is assumed to have sent
/// a confirmation email; where no matching <see cref="Email"/> can be found one is added so the
/// <see cref="EmailSentEvent"/> points at a real row.
/// </summary>
public class BackfillChangeRequestApprovalProcessesJob(TrsDbContext dbContext)
{
    // These match the EventName values stored in the events table for the legacy events.
    private static readonly string[] _legacyEventNames =
    [
        typeof(LegacyChangeNameRequestSupportTaskApprovedEvent).Name,
        typeof(LegacyChangeDateOfBirthRequestSupportTaskApprovedEvent).Name
    ];

    private static readonly string[] _approvalEmailTemplateIds =
    [
        EmailTemplateIds.GetAnIdentityChangeOfNameApprovedEmailConfirmation,
        EmailTemplateIds.GetAnIdentityChangeOfDateOfBirthApprovedEmailConfirmation
    ];

    // The confirmation email is sent by a background job shortly after the approval, so an email sent
    // within this window of the approval is taken to be that approval's confirmation email.
    private static readonly TimeSpan _emailSentBeforeApprovalTolerance = TimeSpan.FromHours(1);
    private static readonly TimeSpan _emailSentAfterApprovalTolerance = TimeSpan.FromDays(1);

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

        var candidateEmails = (await dbContext.Emails
                .Where(e => _approvalEmailTemplateIds.Contains(e.TemplateId) && e.SentOn != null)
                .ToListAsync(cancellationToken))
            .GroupBy(e => (e.TemplateId, e.EmailAddress))
            .ToDictionary(g => g.Key, g => g.OrderBy(e => e.SentOn).ToArray());

        // Emails that an approval process already points at are not up for grabs.
        var claimedEmailIds = (await dbContext.ProcessEvents
                .Where(pe => pe.EventName == nameof(EmailSentEvent))
                .Where(pe => dbContext.Processes.Any(p =>
                    p.ProcessId == pe.ProcessId &&
                    (p.ProcessType == ProcessType.ChangeOfNameRequestApproving || p.ProcessType == ProcessType.ChangeOfDateOfBirthRequestApproving)))
                .Select(pe => pe.Payload)
                .ToListAsync(cancellationToken))
            .Select(payload => ((EmailSentEvent)payload).Email.EmailId)
            .ToHashSet();

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

            // We have no record of whether the confirmation email was actually sent for these approvals, so
            // we assume one was sent to the address the change was requested from, falling back to the
            // address on the record.
            var emailAddress = !string.IsNullOrEmpty(requestEmailAddress)
                ? requestEmailAddress
                : personAttributes.EmailAddress ?? string.Empty;

            var email = FindSentEmail(candidateEmails, claimedEmailIds, emailTemplateId, emailAddress, legacyEvent.Created);

            if (email is null)
            {
                email = new Email
                {
                    EmailId = Guid.NewGuid(),
                    TemplateId = emailTemplateId,
                    EmailAddress = emailAddress,
                    Personalization = new Dictionary<string, string>
                    {
                        { ChangeRequestEmailConstants.FirstNameEmailPersonalisationKey, personAttributes.FirstName }
                    },
                    SentOn = legacyEvent.Created
                };

                dbContext.Emails.Add(email);
            }

            claimedEmailIds.Add(email.EmailId);

            var emailSentEvent = new EmailSentEvent
            {
                EventId = Guid.NewGuid(),
                PersonId = personId,
                Email = EventModels.Email.FromModel(email)
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

    private static Email? FindSentEmail(
        IReadOnlyDictionary<(string TemplateId, string EmailAddress), Email[]> candidateEmails,
        HashSet<Guid> claimedEmailIds,
        string templateId,
        string emailAddress,
        DateTime approvedOn)
    {
        if (!candidateEmails.TryGetValue((templateId, emailAddress), out var emails))
        {
            return null;
        }

        return emails
            .Where(e => !claimedEmailIds.Contains(e.EmailId))
            .Where(e =>
                e.SentOn >= approvedOn - _emailSentBeforeApprovalTolerance &&
                e.SentOn <= approvedOn + _emailSentAfterApprovalTolerance)
            .OrderBy(e => (e.SentOn!.Value - approvedOn).Duration())
            .FirstOrDefault();
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
