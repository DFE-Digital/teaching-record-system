using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using Process = TeachingRecordSystem.Core.DataStore.Postgres.Models.Process;

namespace TeachingRecordSystem.Core.Jobs;

/// <summary>
/// Back-fills <see cref="Process"/> and <see cref="ProcessEvent"/> records from the legacy DQT-era induction events
/// stored in the <c>events</c> table.
/// </summary>
/// <remarks>
/// There are several million of these events, so they're migrated in batches, each committed on its own. Batches are
/// paged through on the (created, event_id) key rather than repeatedly asking for the events that haven't been
/// migrated yet; the latter would rescan everything already done on every batch.
/// </remarks>
public class BackfillDqtInductionProcessesJob(TrsDbContext dbContext)
{
    private const int BatchSize = 1000;

    // These match the EventName values stored in the events table for the legacy events.
    private static readonly string[] _legacyEventNames =
    [
        typeof(LegacyEvents.DqtContactInductionStatusChangedEvent).Name,
        typeof(LegacyEvents.DqtInductionCreatedEvent).Name,
        typeof(LegacyEvents.DqtInductionDeactivatedEvent).Name,
        typeof(LegacyEvents.DqtInductionImportedEvent).Name,
        typeof(LegacyEvents.DqtInductionReactivatedEvent).Name,
        typeof(LegacyEvents.DqtInductionUpdatedEvent).Name
    ];

    public async Task ExecuteAsync(bool dryRun, CancellationToken cancellationToken)
    {
        dbContext.Database.SetCommandTimeout(0);

        var lastCreated = DateTime.MinValue;
        var lastEventId = Guid.Empty;

        while (!cancellationToken.IsCancellationRequested)
        {
            var batch = await dbContext.Events
                .Where(e => _legacyEventNames.Contains(e.EventName))
                .Where(e => e.Created > lastCreated || (e.Created == lastCreated && e.EventId.CompareTo(lastEventId) > 0))
                .OrderBy(e => e.Created)
                .ThenBy(e => e.EventId)
                .Take(BatchSize)
                .ToListAsync(cancellationToken);

            if (batch.Count == 0)
            {
                return;
            }

            var last = batch[^1];
            lastCreated = last.Created;
            lastEventId = last.EventId;

            await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

            // Skip events that have already been back-filled so the job is idempotent.
            var batchEventIds = batch.Select(e => e.EventId).ToArray();
            var alreadyMigratedEventIds = await dbContext.ProcessEvents
                .Where(pe => batchEventIds.Contains(pe.ProcessEventId))
                .Select(pe => pe.ProcessEventId)
                .ToListAsync(cancellationToken);

            foreach (var legacyEvent in batch.Where(e => !alreadyMigratedEventIds.Contains(e.EventId)))
            {
                if (MapEvent(legacyEvent.ToEventBase()) is not var (newEvent, processType))
                {
                    continue;
                }

                AddProcessAndProcessEvent(legacyEvent, newEvent, processType);
            }

            await dbContext.SaveChangesAsync(cancellationToken);

            if (dryRun)
            {
                // Rolling back every batch would leave the loop with the same work to do forever, so a dry run
                // covers the first batch only.
                await transaction.RollbackAsync(cancellationToken);
                return;
            }

            await transaction.CommitAsync(cancellationToken);

            // Otherwise the change tracker keeps every batch's entities alive for the lifetime of the job.
            dbContext.ChangeTracker.Clear();
        }
    }

    private static (IEvent Event, ProcessType ProcessType)? MapEvent(LegacyEvents.EventBase legacyEvent) => legacyEvent switch
    {
        LegacyEvents.DqtContactInductionStatusChangedEvent e => (
            new DqtContactInductionStatusChangedEvent
            {
                EventId = e.EventId,
                PersonId = e.PersonId,
                InductionStatus = e.InductionStatus,
                OldInductionStatus = e.OldInductionStatus
            },
            ProcessType.PersonInductionStatusChangingInDqt),

        LegacyEvents.DqtInductionCreatedEvent e => (
            new DqtInductionCreatedEvent
            {
                EventId = e.EventId,
                PersonId = e.PersonId,
                Induction = e.Induction
            },
            ProcessType.InductionCreatingInDqt),

        LegacyEvents.DqtInductionDeactivatedEvent e => (
            new DqtInductionDeactivatedEvent
            {
                EventId = e.EventId,
                PersonId = e.PersonId,
                Induction = e.Induction
            },
            ProcessType.InductionDeactivatingInDqt),

        LegacyEvents.DqtInductionImportedEvent e => (
            new DqtInductionImportedEvent
            {
                EventId = e.EventId,
                PersonId = e.PersonId,
                Induction = e.Induction,
                DqtState = e.DqtState
            },
            ProcessType.InductionImportingIntoDqt),

        LegacyEvents.DqtInductionReactivatedEvent e => (
            new DqtInductionReactivatedEvent
            {
                EventId = e.EventId,
                PersonId = e.PersonId,
                Induction = e.Induction
            },
            ProcessType.InductionReactivatingInDqt),

        LegacyEvents.DqtInductionUpdatedEvent e => (
            new DqtInductionUpdatedEvent
            {
                EventId = e.EventId,
                PersonId = e.PersonId,
                Induction = e.Induction,
                OldInduction = e.OldInduction,
                Changes = (DqtInductionUpdatedEventChanges)(int)e.Changes
            },
            ProcessType.InductionUpdatingInDqt),

        _ => null
    };

    private void AddProcessAndProcessEvent(Event legacyEvent, IEvent newEvent, ProcessType processType)
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
            PersonIds = [.. newEvent.PersonIds],
            OneLoginUserSubjects = [],
            SupportTaskReferences = [],
            ChangeReason = null
        };

        dbContext.Processes.Add(process);

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
}
