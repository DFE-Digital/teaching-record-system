using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Events.ChangeReasons;
using LegacyPersonStatusUpdatedEvent = TeachingRecordSystem.Core.Events.Legacy.PersonStatusUpdatedEvent;
using Process = TeachingRecordSystem.Core.DataStore.Postgres.Models.Process;

namespace TeachingRecordSystem.Core.Jobs;

/// <summary>
/// Back-fills <see cref="Process"/> and <see cref="ProcessEvent"/> records from the legacy
/// <c>PersonStatusUpdatedEvent</c>s stored in the <c>events</c> table.
/// </summary>
/// <remarks>
/// The legacy event covered both directions, so it maps onto a <see cref="ProcessType.PersonDeactivating"/> process
/// holding a <see cref="PersonDeactivatedEvent"/> or a <see cref="ProcessType.PersonReactivating"/> process holding a
/// <see cref="PersonReactivatedEvent"/>, depending on the status it recorded. The comments and evidence the change was
/// made with live on the process, not on an event.
/// </remarks>
public class BackfillPersonStatusProcessesJob(TrsDbContext dbContext)
{
    private const int BatchSize = 1000;

    // This matches the EventName value stored in the events table for the legacy event.
    private static readonly string _legacyEventName = typeof(LegacyPersonStatusUpdatedEvent).Name;

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        dbContext.Database.SetCommandTimeout(0);

        var lastCreated = DateTime.MinValue;
        var lastEventId = Guid.Empty;

        while (!cancellationToken.IsCancellationRequested)
        {
            var batch = await dbContext.Events
                .AsNoTracking()
                .Where(e => e.EventName == _legacyEventName)
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

            // Skip events that have already been back-filled so the job is idempotent. The dual-write era gave the
            // legacy event the same id as the process event it was written alongside, so those are skipped here too.
            var batchEventIds = batch.Select(e => e.EventId).ToArray();
            var alreadyMigratedEventIds = await dbContext.ProcessEvents
                .Where(pe => batchEventIds.Contains(pe.ProcessEventId))
                .Select(pe => pe.ProcessEventId)
                .ToListAsync(cancellationToken);

            foreach (var legacyEvent in batch.Where(e => !alreadyMigratedEventIds.Contains(e.EventId)))
            {
                AddProcessAndProcessEvent(legacyEvent, (LegacyPersonStatusUpdatedEvent)legacyEvent.ToEventBase());
            }

            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            // Otherwise the change tracker keeps every batch's entities alive for the lifetime of the job.
            dbContext.ChangeTracker.Clear();
        }
    }

    private static (IEvent Event, ProcessType ProcessType) CreateEvent(LegacyPersonStatusUpdatedEvent statusUpdatedEvent)
    {
        if (statusUpdatedEvent.Status is PersonStatus.Deactivated)
        {
            var changes = PersonDeactivatedEventChanges.PersonStatus |
                (statusUpdatedEvent.DateOfDeath.HasValue ? PersonDeactivatedEventChanges.DateOfDeath : 0);

            return (
                new PersonDeactivatedEvent
                {
                    EventId = statusUpdatedEvent.EventId,
                    PersonId = statusUpdatedEvent.PersonId,
                    Changes = changes,
                    MergedWithPersonId = null,
                    DateOfDeath = statusUpdatedEvent.DateOfDeath
                },
                ProcessType.PersonDeactivating);
        }

        // The legacy event never recorded the date of death the reactivation cleared, so only the status is flagged.
        return (
            new PersonReactivatedEvent
            {
                EventId = statusUpdatedEvent.EventId,
                PersonId = statusUpdatedEvent.PersonId,
                Changes = PersonReactivatedEventChanges.PersonStatus
            },
            ProcessType.PersonReactivating);
    }

    private void AddProcessAndProcessEvent(Event legacyEvent, LegacyPersonStatusUpdatedEvent statusUpdatedEvent)
    {
        var processId = Guid.NewGuid();
        var (newEvent, processType) = CreateEvent(statusUpdatedEvent);

        var process = new Process
        {
            ProcessId = processId,
            ProcessType = processType,
            CreatedOn = legacyEvent.Created,
            UpdatedOn = legacyEvent.Created,
            UserId = statusUpdatedEvent.RaisedBy.UserId,
            DqtUserId = statusUpdatedEvent.RaisedBy.DqtUserId,
            DqtUserName = statusUpdatedEvent.RaisedBy.DqtUserName,
            PersonIds = [.. newEvent.PersonIds],
            OneLoginUserSubjects = [],
            SupportTaskReferences = [],
            ChangeReason = new ChangeReasonWithDetailsAndEvidence
            {
                Reason = statusUpdatedEvent.Reason,
                Details = statusUpdatedEvent.ReasonDetail,
                EvidenceFile = statusUpdatedEvent.EvidenceFile,
                AdditionalInformation = statusUpdatedEvent.AdditionalInformation
            }
        };

        dbContext.Processes.Add(process);

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
