using System.Text.Json.Nodes;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Events.Legacy;
using TeachingRecordSystem.Core.Models.SupportTasks;
using LegacyEventBase = TeachingRecordSystem.Core.Events.Legacy.EventBase;
using LegacySupportTaskUpdatedEvent = TeachingRecordSystem.Core.Events.Legacy.SupportTaskUpdatedEvent;
using SupportTaskUpdatedEvent = TeachingRecordSystem.Core.Events.SupportTaskUpdatedEvent;

namespace TeachingRecordSystem.Core.Jobs;

/// <summary>
/// Back-fills the support task columns added alongside the subject/assignment/outcome work
/// (<c>subject_name</c>, <c>subject_email_address</c>, <c>outcome_label</c>, <c>completed_on</c> and
/// <c>completed_by_user_id</c>) for tasks that pre-date those columns. <c>assigned_to_user_id</c> is
/// deliberately not back-filled, and <c>subject_names</c> is populated automatically by the
/// <c>trg_*_support_task_subject_names</c> triggers when <c>subject_name</c> is written.
///
/// The subject is reconstructed the same way the creating code path would have set it. Where the
/// subject name comes from a linked <see cref="Person"/> we use the person's name <em>as it was when
/// the task was created</em> (reconstructed from the person's attribute-change events), not their
/// current name. Completion details come from the event that closed the task, and that event's
/// <c>OutcomeLabel</c> is back-filled at the same time.
/// </summary>
public class BackfillSupportTaskColumnsJob(TrsDbContext dbContext)
{
    // Legacy events that carry a person's before/after name, used to reconstruct a historic name.
    private static readonly string[] _personAttributeEventNames =
        [.. LegacyEventBase.GetEventNamesForBaseType(typeof(IEventWithPersonAttributes))];

    // Legacy events that carry the support task's before/after state, i.e. can represent a close.
    private static readonly string[] _closingLegacyEventNames =
        [.. LegacyEventBase.GetEventNamesForBaseType(typeof(LegacySupportTaskUpdatedEvent))];

    public async Task ExecuteAsync(bool dryRun, CancellationToken cancellationToken)
    {
        dbContext.Database.SetCommandTimeout(0);

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        var supportTasks = await dbContext.SupportTasks
            .IgnoreQueryFilters()
            .Include(t => t.Person)
            .Include(t => t.OneLoginUser)
            .Include(t => t.TrnRequestMetadata)
            .ToListAsync(cancellationToken);

        var closingEventsByReference = await GetClosingEventsAsync(cancellationToken);

        foreach (var supportTask in supportTasks)
        {
            await BackfillSubjectAsync(supportTask, cancellationToken);

            if (supportTask.Status is SupportTaskStatus.Closed)
            {
                BackfillCompletionAndOutcome(supportTask, closingEventsByReference);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        if (dryRun)
        {
            await transaction.RollbackAsync(cancellationToken);
        }
        else
        {
            await transaction.CommitAsync(cancellationToken);
        }
    }

    private async Task BackfillSubjectAsync(SupportTask supportTask, CancellationToken cancellationToken)
    {
        var (name, emailAddress) = supportTask.SupportTaskType switch
        {
            // Subject is the linked person's name at the time the task was created.
            SupportTaskType.ChangeNameRequest or
            SupportTaskType.ChangeDateOfBirthRequest or
            SupportTaskType.TrnRequestManualChecksNeeded =>
                (await GetPersonNameAtCreationAsync(supportTask, cancellationToken), (string?)null),

            // Subject name comes from the TRN request that the task was raised against.
            SupportTaskType.TrnRequest or
            SupportTaskType.TeacherPensionsPotentialDuplicate =>
                SubjectParts(SupportTask.Subject.FromTrnRequest(supportTask.TrnRequestMetadata!)),

            // Subject comes from the stated/verified identity captured on the task's data.
            SupportTaskType.OneLoginUserIdVerification =>
                SubjectParts(SupportTask.Subject.FromOneLoginUser(
                    supportTask.GetData<OneLoginUserIdVerificationData>().StatedFirstName,
                    supportTask.GetData<OneLoginUserIdVerificationData>().StatedLastName)),

            SupportTaskType.OneLoginUserRecordMatching =>
                SubjectParts(GetRecordMatchingSubject(supportTask)),

            _ => throw new NotSupportedException(
                $"Cannot reconstruct the subject for a support task of type '{supportTask.SupportTaskType}'.")
        };

        // SubjectName is init-only, so update it through the change tracker. Writing subject_name fires
        // the database trigger that (re)populates subject_names.
        dbContext.Entry(supportTask).Property(t => t.SubjectName).CurrentValue = name;
        dbContext.Entry(supportTask).Property(t => t.SubjectEmailAddress).CurrentValue = emailAddress;
    }

    private static (string? Name, string? EmailAddress) SubjectParts(SupportTask.Subject subject) =>
        (subject.Name, subject.EmailAddress);

    private static SupportTask.Subject GetRecordMatchingSubject(SupportTask supportTask)
    {
        var data = supportTask.GetData<OneLoginUserRecordMatchingData>();
        return data.VerifiedNames is not null
            ? SupportTask.Subject.FromOneLoginUser(data.VerifiedNames)
            : SupportTask.Subject.FromOneLoginUser(supportTask.OneLoginUser!);
    }

    private async Task<string> GetPersonNameAtCreationAsync(SupportTask supportTask, CancellationToken cancellationToken)
    {
        var person = supportTask.Person!;
        var personId = supportTask.PersonId!.Value;

        // Any name-changing event that happened after the task was created moves the name away from what
        // it was at creation time. The earliest such event's "old" attributes therefore hold the name as
        // it was when the task was created; if there were no later name changes, the current name applies.
        var laterEvents = await dbContext.Events
            .Where(e => _personAttributeEventNames.Contains(e.EventName))
            .Where(e => e.PersonId == personId || e.PersonIds.Contains(personId))
            .Where(e => e.Created > supportTask.CreatedOn)
            .OrderBy(e => e.Created)
            .ToListAsync(cancellationToken);

        foreach (var @event in laterEvents)
        {
            if (@event.ToEventBase() is IEventWithPersonAttributes { OldPersonAttributes: { } oldAttributes } current &&
                NameChanged(oldAttributes, current.PersonAttributes))
            {
                return string.JoinNonEmpty(' ', oldAttributes.FirstName, oldAttributes.MiddleName, oldAttributes.LastName);
            }
        }

        return string.JoinNonEmpty(' ', person.FirstName, person.MiddleName, person.LastName);
    }

    private static bool NameChanged(EventModels.PersonDetails oldDetails, EventModels.PersonDetails newDetails) =>
        oldDetails.FirstName != newDetails.FirstName ||
        oldDetails.MiddleName != newDetails.MiddleName ||
        oldDetails.LastName != newDetails.LastName;

    private void BackfillCompletionAndOutcome(SupportTask supportTask, IReadOnlyDictionary<string, ClosingEvent> closingEventsByReference)
    {
        var outcomeLabel = supportTask.Data.GetOutcomeLabel();
        supportTask.OutcomeLabel = outcomeLabel;

        if (!closingEventsByReference.TryGetValue(supportTask.SupportTaskReference, out var closingEvent))
        {
            return;
        }

        supportTask.CompletedOn = closingEvent.CompletedOn;
        supportTask.CompletedByUserId = closingEvent.CompletedByUserId;

        // Back-fill the OutcomeLabel on the (single) event that closed the task.
        if (closingEvent.ProcessEvent is { Payload: SupportTaskUpdatedEvent processPayload } processEvent &&
            processPayload.SupportTask.OutcomeLabel is null)
        {
            dbContext.Entry(processEvent).Property(e => e.Payload).CurrentValue = processPayload with
            {
                SupportTask = processPayload.SupportTask with { OutcomeLabel = outcomeLabel }
            };
        }

        if (closingEvent.LegacyEvent is { } legacyEvent)
        {
            var payload = JsonNode.Parse(legacyEvent.Payload)!;
            if (payload["SupportTask"] is { } supportTaskNode && supportTaskNode["OutcomeLabel"] is null)
            {
                supportTaskNode["OutcomeLabel"] = outcomeLabel;
                dbContext.Entry(legacyEvent).Property(e => e.Payload).CurrentValue = payload.ToJsonString();
            }
        }
    }

    private async Task<Dictionary<string, ClosingEvent>> GetClosingEventsAsync(CancellationToken cancellationToken)
    {
        var closingEvents = new Dictionary<string, ClosingEvent>();

        // New pipeline: SupportTaskUpdatedEvents live in process_events; the raising user and timestamp come
        // from the owning process. This covers every task closed via the new pipeline, including those (e.g.
        // One Login matching) that never produced a legacy event.
        var processUpdates = await (
            from processEvent in dbContext.ProcessEvents
            join process in dbContext.Processes on processEvent.ProcessId equals process.ProcessId
            where processEvent.EventName == nameof(SupportTaskUpdatedEvent)
            select new { ProcessEvent = processEvent, process.UserId })
            .ToListAsync(cancellationToken);

        foreach (var processUpdate in processUpdates)
        {
            if (processUpdate.ProcessEvent.Payload is SupportTaskUpdatedEvent { SupportTask: var newTask, OldSupportTask: var oldTask } &&
                ClosesTask(oldTask.Status, newTask.Status))
            {
                closingEvents[newTask.SupportTaskReference] = new ClosingEvent(
                    processUpdate.ProcessEvent.CreatedOn,
                    processUpdate.UserId,
                    ProcessEvent: processUpdate.ProcessEvent,
                    LegacyEvent: null);
            }
        }

        // Legacy events: needed both for tasks closed before the new pipeline and to back-fill the legacy
        // representation of new-pipeline closes.
        var legacyEvents = await dbContext.Events
            .Where(e => _closingLegacyEventNames.Contains(e.EventName))
            .ToListAsync(cancellationToken);

        foreach (var legacyEvent in legacyEvents)
        {
            if (legacyEvent.ToEventBase() is LegacySupportTaskUpdatedEvent { SupportTask: var newTask, OldSupportTask: var oldTask } eventBase &&
                ClosesTask(oldTask.Status, newTask.Status))
            {
                var reference = newTask.SupportTaskReference;

                if (closingEvents.TryGetValue(reference, out var existing))
                {
                    // Prefer the process event for completion details (it always carries the user); attach the
                    // legacy event so its payload gets its OutcomeLabel back-filled too.
                    closingEvents[reference] = existing with { LegacyEvent = legacyEvent };
                }
                else
                {
                    closingEvents[reference] = new ClosingEvent(
                        legacyEvent.Created,
                        eventBase.RaisedBy.UserId,
                        ProcessEvent: null,
                        LegacyEvent: legacyEvent);
                }
            }
        }

        return closingEvents;
    }

    private static bool ClosesTask(SupportTaskStatus oldStatus, SupportTaskStatus newStatus) =>
        oldStatus is not SupportTaskStatus.Closed && newStatus is SupportTaskStatus.Closed;

    private sealed record ClosingEvent(
        DateTime CompletedOn,
        Guid? CompletedByUserId,
        ProcessEvent? ProcessEvent,
        Event? LegacyEvent);
}
