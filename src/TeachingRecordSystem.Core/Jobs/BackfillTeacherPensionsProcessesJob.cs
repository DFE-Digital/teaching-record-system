using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using LegacyChanges = TeachingRecordSystem.Core.Events.Legacy.TeacherPensionsPotentialDuplicateSupportTaskResolvedEventChanges;
using LegacyReason = TeachingRecordSystem.Core.Events.Legacy.TeacherPensionsPotentialDuplicateSupportTaskResolvedReason;
using LegacyTeacherPensionsPotentialDuplicateSupportTaskResolvedEvent = TeachingRecordSystem.Core.Events.Legacy.TeacherPensionsPotentialDuplicateSupportTaskResolvedEvent;
using Process = TeachingRecordSystem.Core.DataStore.Postgres.Models.Process;

namespace TeachingRecordSystem.Core.Jobs;

/// <summary>
/// Back-fills <see cref="Process"/> and <see cref="ProcessEvent"/> records from the legacy
/// <see cref="LegacyTeacherPensionsPotentialDuplicateSupportTaskResolvedEvent"/> events stored in the
/// <c>events</c> table. Historically resolving a Teacher Pensions potential duplicate support task
/// wrote only the single summarising legacy event directly and did not go through the event pipeline,
/// so these resolutions have no corresponding process.
/// </summary>
/// <remarks>
/// The individual pipeline events are reconstructed from the data captured on the legacy event, in the
/// same order the live resolution flow publishes them:
/// <list type="bullet">
/// <item><see cref="PersonDeactivatedEvent"/> — merge only; the imported record is deactivated into the retained record.</item>
/// <item><see cref="TrnRequestUpdatedEvent"/> — always; the request is resolved to the kept/retained record.</item>
/// <item><see cref="PersonDetailsUpdatedEvent"/> — merge only, when the retained record's attributes were updated from the imported record.</item>
/// <item><see cref="SupportTaskUpdatedEvent"/> — always; the support task is closed.</item>
/// </list>
/// The occasionally-emitted <c>OneLoginUserUpdatedEvent</c> and <c>SupportTaskCreatedEvent</c> (further
/// checks needed) events are not reconstructed as the legacy event doesn't capture the data to do so.
/// </remarks>
public class BackfillTeacherPensionsProcessesJob(TrsDbContext dbContext)
{
    // Matches the EventName value stored in the events table for the legacy event.
    private static readonly string _legacyEventName =
        typeof(LegacyTeacherPensionsPotentialDuplicateSupportTaskResolvedEvent).Name;

    public async Task ExecuteAsync(bool dryRun, CancellationToken cancellationToken)
    {
        dbContext.Database.SetCommandTimeout(0);

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        // Only migrate events that haven't already been back-filled so the job is idempotent. The
        // reconstructed SupportTaskUpdatedEvent reuses the legacy event's id (the live flow used the
        // same id for both), so an existing process event with that id means we've already run.
        var legacyEvents = await dbContext.Events
            .Where(e => e.EventName == _legacyEventName)
            .Where(e => !dbContext.ProcessEvents.Any(pe => pe.ProcessEventId == e.EventId))
            .OrderBy(e => e.Created)
            .ToListAsync(cancellationToken);

        foreach (var legacyEvent in legacyEvents)
        {
            var eventData = (LegacyTeacherPensionsPotentialDuplicateSupportTaskResolvedEvent)legacyEvent.ToEventBase();

            var (processType, isMerge) = eventData.ChangeReason switch
            {
                LegacyReason.RecordKept => (ProcessType.TeacherPensionsDuplicateSupportTaskResolvingWithoutMerge, false),
                LegacyReason.RecordMerged => (ProcessType.TeacherPensionsDuplicateSupportTaskResolvingWithMerge, true),
                _ => throw new InvalidOperationException($"Unexpected change reason: '{eventData.ChangeReason}'.")
            };

            var newEvents = BuildEvents(eventData, isMerge);

            await CreateProcessAndProcessEventsAsync(legacyEvent, newEvents, processType, cancellationToken);
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

    private static IReadOnlyList<IEvent> BuildEvents(
        LegacyTeacherPensionsPotentialDuplicateSupportTaskResolvedEvent eventData,
        bool isMerge)
    {
        var events = new List<IEvent>();

        // Merge deactivates the imported record (the support task's person) into the retained record
        // (the resolved person).
        if (isMerge)
        {
            events.Add(new PersonDeactivatedEvent
            {
                EventId = Guid.NewGuid(),
                PersonId = eventData.SupportTask.PersonId!.Value,
                Changes = PersonDeactivatedEventChanges.MergedWithPersonId,
                MergedWithPersonId = eventData.PersonId,
                DateOfDeath = null
            });
        }

        // The request is resolved to the kept/retained record. The legacy event captures the resolved
        // request metadata; the pre-resolution state had no resolved person and was still pending.
        var newTrnRequest = eventData.RequestData;
        events.Add(new TrnRequestUpdatedEvent
        {
            EventId = Guid.NewGuid(),
            SourceApplicationUserId = newTrnRequest.ApplicationUserId,
            RequestId = newTrnRequest.RequestId,
            Changes = TrnRequestUpdatedChanges.Status | TrnRequestUpdatedChanges.ResolvedPersonId,
            TrnRequest = newTrnRequest,
            OldTrnRequest = newTrnRequest with { Status = TrnRequestStatus.Pending, ResolvedPersonId = null },
            ReasonDetails = null
        });

        // On merge the retained record's attributes may have been updated from the imported record.
        var personDetailsChanges = MapPersonDetailsChanges(eventData.Changes);
        if (personDetailsChanges is not PersonDetailsUpdatedEventChanges.None)
        {
            events.Add(new PersonDetailsUpdatedEvent
            {
                EventId = Guid.NewGuid(),
                PersonId = eventData.PersonId,
                PersonDetails = eventData.PersonAttributes,
                OldPersonDetails = eventData.OldPersonAttributes!,
                Changes = personDetailsChanges
            });
        }

        // The support task is closed. Reuse the legacy event's id (as the live flow did) so the job
        // stays idempotent.
        events.Add(CreateSupportTaskUpdatedEvent(eventData));

        return events;
    }

    private static PersonDetailsUpdatedEventChanges MapPersonDetailsChanges(LegacyChanges changes) =>
        PersonDetailsUpdatedEventChanges.None |
        (changes.HasFlag(LegacyChanges.PersonFirstName) ? PersonDetailsUpdatedEventChanges.FirstName : 0) |
        (changes.HasFlag(LegacyChanges.PersonMiddleName) ? PersonDetailsUpdatedEventChanges.MiddleName : 0) |
        (changes.HasFlag(LegacyChanges.PersonLastName) ? PersonDetailsUpdatedEventChanges.LastName : 0) |
        (changes.HasFlag(LegacyChanges.PersonDateOfBirth) ? PersonDetailsUpdatedEventChanges.DateOfBirth : 0) |
        (changes.HasFlag(LegacyChanges.PersonNationalInsuranceNumber) ? PersonDetailsUpdatedEventChanges.NationalInsuranceNumber : 0) |
        (changes.HasFlag(LegacyChanges.PersonGender) ? PersonDetailsUpdatedEventChanges.Gender : 0);

    // Reconstructs the modern SupportTaskUpdatedEvent, deriving the changed fields from the old/new
    // support task snapshots stored on the legacy event.
    private static SupportTaskUpdatedEvent CreateSupportTaskUpdatedEvent(
        LegacyTeacherPensionsPotentialDuplicateSupportTaskResolvedEvent legacyEvent)
    {
        var changes = SupportTaskUpdatedEventChanges.None |
            (legacyEvent.SupportTask.Status != legacyEvent.OldSupportTask.Status ? SupportTaskUpdatedEventChanges.Status : 0) |
            (!legacyEvent.SupportTask.Data.Equals(legacyEvent.OldSupportTask.Data) ? SupportTaskUpdatedEventChanges.Data : 0) |
            (!Equals(legacyEvent.SupportTask.ResolveJourneySavedState, legacyEvent.OldSupportTask.ResolveJourneySavedState) ? SupportTaskUpdatedEventChanges.ResolveJourneySavedState : 0);

        return new SupportTaskUpdatedEvent
        {
            EventId = legacyEvent.EventId,
            SupportTaskReference = legacyEvent.SupportTask.SupportTaskReference,
            Changes = changes,
            SupportTask = legacyEvent.SupportTask,
            OldSupportTask = legacyEvent.OldSupportTask,
            Comments = legacyEvent.Comments,
            RejectionReason = null
        };
    }

    private async Task CreateProcessAndProcessEventsAsync(
        Event legacyEvent,
        IReadOnlyList<IEvent> newEvents,
        ProcessType processType,
        CancellationToken cancellationToken)
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
            // Aggregated across all of the process's events, matching the live event pipeline.
            PersonIds = [.. newEvents.SelectMany(e => e.PersonIds).Distinct()],
            OneLoginUserSubjects = [.. newEvents.SelectMany(e => e.OneLoginUserSubjects).Distinct()],
            SupportTaskReferences = [.. newEvents.SelectMany(e => e.SupportTaskReferences).Distinct()],
            ChangeReason = null
        };

        dbContext.Processes.Add(process);

        foreach (var newEvent in newEvents)
        {
            var processEvent = new ProcessEvent
            {
                ProcessEventId = newEvent.EventId,
                ProcessId = processId,
                EventName = newEvent.GetType().Name,
                Payload = newEvent,
                PersonIds = newEvent.PersonIds,
                OneLoginUserSubjects = newEvent.OneLoginUserSubjects,
                SupportTaskReferences = newEvent.SupportTaskReferences,
                CreatedOn = legacyEvent.Created
            };

            dbContext.ProcessEvents.Add(processEvent);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
