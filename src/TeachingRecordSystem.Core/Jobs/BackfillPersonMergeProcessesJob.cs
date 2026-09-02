using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Events.ChangeReasons;
using LegacyPersonsMergedEvent = TeachingRecordSystem.Core.Events.Legacy.PersonsMergedEvent;
using Process = TeachingRecordSystem.Core.DataStore.Postgres.Models.Process;

namespace TeachingRecordSystem.Core.Jobs;

/// <summary>
/// Back-fills <see cref="Process"/> and <see cref="ProcessEvent"/> records from the legacy
/// <c>PersonsMergedEvent</c>s stored in the <c>events</c> table. The Support UI merge journey wrote the legacy
/// event directly until it moved onto <see cref="PersonService.MergePersonsAsync"/>, so everything from before
/// then has the legacy event alone.
///
/// There is no merge event in the process model: a merge is a <see cref="PersonDeactivatedEvent"/> against the
/// record that was deactivated, carrying the record it was merged into, plus a
/// <see cref="PersonDetailsUpdatedEvent"/> for whatever the retained record took from it.
/// </summary>
public class BackfillPersonMergeProcessesJob(TrsDbContext dbContext)
{
    // This matches the EventName value stored in the events table for the legacy event.
    private static readonly string _legacyEventName = typeof(LegacyPersonsMergedEvent).Name;

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        dbContext.Database.SetCommandTimeout(0);

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        // Only migrate events that haven't already been back-filled so the job is idempotent. The dual-write era
        // gave the legacy event the same id as the PersonDeactivatedEvent it was written alongside, so those are
        // skipped here too.
        var legacyEvents = await dbContext.Events
            .Where(e => e.EventName == _legacyEventName)
            .Where(e => !dbContext.ProcessEvents.Any(pe => pe.ProcessEventId == e.EventId))
            .OrderBy(e => e.Created)
            .ToListAsync(cancellationToken);

        foreach (var legacyEvent in legacyEvents)
        {
            var eventData = (LegacyPersonsMergedEvent)legacyEvent.ToEventBase();

            CreateProcessAndProcessEvents(legacyEvent, eventData);

            await dbContext.SaveChangesAsync(cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
    }

    private static IEvent[] CreateEvents(Guid eventId, LegacyPersonsMergedEvent mergedEvent)
    {
        // The merge only ever set the deactivated record's status and the record it was merged into, so the live
        // path flags MergedWithPersonId alone.
        var personDeactivatedEvent = new PersonDeactivatedEvent
        {
            EventId = eventId,
            PersonId = mergedEvent.SecondaryPersonId,
            MergedWithPersonId = mergedEvent.PersonId,
            Changes = PersonDeactivatedEventChanges.MergedWithPersonId,
            DateOfDeath = null
        };

        // The legacy event carries the person attribute changes shifted up by 16 bits; see PersonAttributesChanges.
        // A merge that kept every attribute of the retained record changed nothing about it, and the journey
        // published no event for that either.
        var personDetailsChanges = (PersonDetailsUpdatedEventChanges)((int)mergedEvent.Changes >> 16);

        if (personDetailsChanges is PersonDetailsUpdatedEventChanges.None)
        {
            return [personDeactivatedEvent];
        }

        return
        [
            personDeactivatedEvent,
            new PersonDetailsUpdatedEvent
            {
                EventId = Guid.NewGuid(),
                PersonId = mergedEvent.PersonId,
                PersonDetails = mergedEvent.PersonAttributes,
                OldPersonDetails = mergedEvent.OldPersonAttributes,
                Changes = personDetailsChanges
            }
        ];
    }

    private void CreateProcessAndProcessEvents(Event legacyEvent, LegacyPersonsMergedEvent mergedEvent)
    {
        var processId = Guid.NewGuid();
        var newEvents = CreateEvents(legacyEvent.EventId, mergedEvent);

        var process = new Process
        {
            ProcessId = processId,
            ProcessType = ProcessType.PersonMerging,
            CreatedOn = legacyEvent.Created,
            UpdatedOn = legacyEvent.Created,
            UserId = mergedEvent.RaisedBy.UserId,
            DqtUserId = mergedEvent.RaisedBy.DqtUserId,
            DqtUserName = mergedEvent.RaisedBy.DqtUserName,
            PersonIds = [.. newEvents.SelectMany(e => e.PersonIds).Distinct()],
            OneLoginUserSubjects = [.. newEvents.SelectMany(e => e.OneLoginUserSubjects).Distinct()],
            SupportTaskReferences = [.. newEvents.SelectMany(e => e.SupportTaskReferences).Distinct()],
            // The comments and evidence the merge was recorded with live on the process, not on an event.
            ChangeReason = new ChangeReasonWithDetailsAndEvidence
            {
                Reason = null,
                Details = mergedEvent.Comments,
                EvidenceFile = mergedEvent.EvidenceFile,
                AdditionalInformation = null
            }
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
