using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using LegacyNpqTrnRequestResolvedReason = TeachingRecordSystem.Core.Events.Legacy.NpqTrnRequestResolvedReason;
using LegacyNpqTrnRequestSupportTaskRejectedEvent = TeachingRecordSystem.Core.Events.Legacy.NpqTrnRequestSupportTaskRejectedEvent;
using LegacyNpqTrnRequestSupportTaskResolvedEvent = TeachingRecordSystem.Core.Events.Legacy.NpqTrnRequestSupportTaskResolvedEvent;
using Process = TeachingRecordSystem.Core.DataStore.Postgres.Models.Process;

namespace TeachingRecordSystem.Core.Jobs;

/// <summary>
/// Back-fills <see cref="Process"/> and <see cref="ProcessEvent"/> records from the legacy NPQ TRN request
/// resolved/rejected events stored in the <c>events</c> table. The NPQ TRN request journey wrote these events
/// directly until it moved onto the event pipeline, so everything from before then has the legacy event alone.
///
/// Nothing here needs to add an <see cref="EmailSentEvent"/>: the 'TRN generated for NPQ' email was introduced
/// after the journey was already running as a process, and it was always enqueued with the process id, so every
/// resolution that sent one already has both the process and the event.
/// </summary>
public class BackfillNpqTrnRequestProcessesJob(TrsDbContext dbContext)
{
    // These match the EventName values stored in the events table for the legacy events.
    private static readonly string[] _legacyEventNames =
    [
        typeof(LegacyNpqTrnRequestSupportTaskResolvedEvent).Name,
        typeof(LegacyNpqTrnRequestSupportTaskRejectedEvent).Name
    ];

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        dbContext.Database.SetCommandTimeout(0);

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        // Only migrate events that haven't already been back-filled so the job is idempotent. The dual-write era
        // gave the legacy event the same id as the process event it was written alongside, so those are skipped
        // here too.
        var legacyEvents = await dbContext.Events
            .Where(e => _legacyEventNames.Contains(e.EventName))
            .Where(e => !dbContext.ProcessEvents.Any(pe => pe.ProcessEventId == e.EventId))
            .OrderBy(e => e.Created)
            .ToListAsync(cancellationToken);

        foreach (var legacyEvent in legacyEvents)
        {
            var eventData = legacyEvent.ToEventBase();

            var (processType, newEvents, personId) = eventData switch
            {
                LegacyNpqTrnRequestSupportTaskResolvedEvent resolved =>
                    (ProcessType.NpqTrnRequestApproving, CreateApprovingEvents(legacyEvent.EventId, resolved), (Guid?)resolved.PersonId),
                LegacyNpqTrnRequestSupportTaskRejectedEvent rejected =>
                    (ProcessType.NpqTrnRequestRejecting, CreateRejectingEvents(legacyEvent.EventId, rejected), null),
                _ => throw new InvalidOperationException($"Unexpected event type: '{eventData.GetType().Name}'.")
            };

            CreateProcessAndProcessEvents(legacyEvent, processType, newEvents, personId);

            await dbContext.SaveChangesAsync(cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
    }

    private static IEvent[] CreateApprovingEvents(Guid eventId, LegacyNpqTrnRequestSupportTaskResolvedEvent resolved)
    {
        // Resolving the request closes the support task (Status) and stamps the outcome onto its data (Data).
        var supportTaskUpdatedEvent = new SupportTaskUpdatedEvent
        {
            EventId = eventId,
            SupportTaskReference = resolved.SupportTask.SupportTaskReference,
            Changes = SupportTaskUpdatedEventChanges.Status | SupportTaskUpdatedEventChanges.Data,
            SupportTask = resolved.SupportTask,
            OldSupportTask = resolved.OldSupportTask,
            Comments = resolved.Comments,
            RejectionReason = null
        };

        var trnRequestUpdatedEvent = CreateTrnRequestUpdatedEvent(
            resolved.RequestData,
            TrnRequestStatus.Completed,
            resolvedPersonId: resolved.PersonId);

        var newEvents = new List<IEvent> { supportTaskUpdatedEvent, trnRequestUpdatedEvent };

        // The legacy reason was derived from whether the resolution created a record, so map it back the same way.
        if (resolved.ChangeReason is LegacyNpqTrnRequestResolvedReason.RecordCreated)
        {
            newEvents.Add(new PersonCreatedEvent
            {
                EventId = Guid.NewGuid(),
                PersonId = resolved.PersonId,
                Details = resolved.PersonAttributes,
                TrnRequestMetadata = resolved.RequestData
            });
        }
        else
        {
            // The legacy event carries the person attribute changes shifted up by 16 bits; see PersonAttributesChanges.
            // A merge that took every attribute from the existing record changed nothing about the person, and the
            // journey published no event for it either.
            var personDetailsChanges = (PersonDetailsUpdatedEventChanges)((int)resolved.Changes >> 16);

            if (personDetailsChanges is not PersonDetailsUpdatedEventChanges.None &&
                resolved.OldPersonAttributes is { } oldPersonAttributes)
            {
                newEvents.Add(new PersonDetailsUpdatedEvent
                {
                    EventId = Guid.NewGuid(),
                    PersonId = resolved.PersonId,
                    PersonDetails = resolved.PersonAttributes,
                    OldPersonDetails = oldPersonAttributes,
                    Changes = personDetailsChanges
                });
            }
        }

        return [.. newEvents];
    }

    private static IEvent[] CreateRejectingEvents(Guid eventId, LegacyNpqTrnRequestSupportTaskRejectedEvent rejected)
    {
        // Rejecting the request closes the support task (Status) and stamps the outcome onto its data (Data).
        var supportTaskUpdatedEvent = new SupportTaskUpdatedEvent
        {
            EventId = eventId,
            SupportTaskReference = rejected.SupportTask.SupportTaskReference,
            Changes = SupportTaskUpdatedEventChanges.Status | SupportTaskUpdatedEventChanges.Data,
            SupportTask = rejected.SupportTask,
            OldSupportTask = rejected.OldSupportTask,
            Comments = null,
            RejectionReason = rejected.RejectionReason
        };

        var trnRequestUpdatedEvent = CreateTrnRequestUpdatedEvent(
            rejected.RequestData,
            TrnRequestStatus.Rejected,
            resolvedPersonId: null);

        return [supportTaskUpdatedEvent, trnRequestUpdatedEvent];
    }

    // The request metadata on the legacy events was snapshotted before the journey moved the request's status —
    // and on the oldest events the status predates the column altogether — so both sides of the transition are
    // stamped here from the outcome the event records rather than taken from the snapshot.
    private static TrnRequestUpdatedEvent CreateTrnRequestUpdatedEvent(
        EventModels.TrnRequestMetadata requestData,
        TrnRequestStatus status,
        Guid? resolvedPersonId)
    {
        var trnRequest = requestData with { Status = status, ResolvedPersonId = resolvedPersonId };
        var oldTrnRequest = requestData with { Status = TrnRequestStatus.Pending, ResolvedPersonId = null };

        var changes = TrnRequestUpdatedChanges.Status |
            (resolvedPersonId is not null ? TrnRequestUpdatedChanges.ResolvedPersonId : 0);

        return new TrnRequestUpdatedEvent
        {
            EventId = Guid.NewGuid(),
            SourceApplicationUserId = requestData.ApplicationUserId,
            RequestId = requestData.RequestId,
            Changes = changes,
            TrnRequest = trnRequest,
            OldTrnRequest = oldTrnRequest,
            ReasonDetails = null
        };
    }

    private void CreateProcessAndProcessEvents(Event legacyEvent, ProcessType processType, IEvent[] newEvents, Guid? personId)
    {
        var legacyEventData = legacyEvent.ToEventBase();
        var processId = Guid.NewGuid();

        // NPQ support tasks don't carry a person id and a merge that changed nothing has no person event, so the
        // resolved person is added explicitly to keep the process on that person's change history.
        var personIds = newEvents
            .SelectMany(e => e.PersonIds)
            .Concat(personId is { } id ? [id] : Array.Empty<Guid>())
            .Distinct();

        var process = new Process
        {
            ProcessId = processId,
            ProcessType = processType,
            CreatedOn = legacyEvent.Created,
            UpdatedOn = legacyEvent.Created,
            UserId = legacyEventData.RaisedBy.UserId,
            DqtUserId = legacyEventData.RaisedBy.DqtUserId,
            DqtUserName = legacyEventData.RaisedBy.DqtUserName,
            PersonIds = [.. personIds],
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
