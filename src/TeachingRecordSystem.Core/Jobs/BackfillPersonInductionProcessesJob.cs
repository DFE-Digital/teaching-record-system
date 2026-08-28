using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Events.ChangeReasons;
using Process = TeachingRecordSystem.Core.DataStore.Postgres.Models.Process;

namespace TeachingRecordSystem.Core.Jobs;

/// <summary>
/// Back-fills <see cref="Process"/> and <see cref="ProcessEvent"/> records from the legacy person induction updated
/// events stored in the <c>events</c> table.
/// </summary>
/// <remarks>
/// There are a lot of these events, so they're migrated in batches, each committed on its own. Batches are paged
/// through on the (created, event_id) key rather than repeatedly asking for the events that haven't been migrated
/// yet; the latter would rescan everything already done on every batch.
/// </remarks>
public class BackfillPersonInductionProcessesJob(TrsDbContext dbContext)
{
    private const int BatchSize = 1000;

    // This matches the EventName value stored in the events table for the legacy event.
    private static readonly string _legacyEventName = typeof(LegacyEvents.PersonInductionUpdatedEvent).Name;

    public async Task ExecuteAsync(bool dryRun, CancellationToken cancellationToken)
    {
        dbContext.Database.SetCommandTimeout(0);

        var lastCreated = DateTime.MinValue;
        var lastEventId = Guid.Empty;

        while (!cancellationToken.IsCancellationRequested)
        {
            var batch = await dbContext.Events
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

            // Skip events that have already been back-filled so the job is idempotent.
            var batchEventIds = batch.Select(e => e.EventId).ToArray();
            var alreadyMigratedEventIds = await dbContext.ProcessEvents
                .Where(pe => batchEventIds.Contains(pe.ProcessEventId))
                .Select(pe => pe.ProcessEventId)
                .ToListAsync(cancellationToken);

            foreach (var legacyEvent in batch.Where(e => !alreadyMigratedEventIds.Contains(e.EventId)))
            {
                if (legacyEvent.ToEventBase() is not LegacyEvents.PersonInductionUpdatedEvent updated)
                {
                    continue;
                }

                AddProcessAndProcessEvent(legacyEvent, updated);
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

    private void AddProcessAndProcessEvent(Event legacyEvent, LegacyEvents.PersonInductionUpdatedEvent updated)
    {
        var processId = Guid.NewGuid();

        IEvent newEvent = new PersonInductionUpdatedEvent
        {
            EventId = updated.EventId,
            PersonId = updated.PersonId,
            Induction = updated.Induction,
            OldInduction = updated.OldInduction,
            Changes = (PersonInductionUpdatedEventChanges)(int)updated.Changes
        };

        var process = new Process
        {
            ProcessId = processId,
            ProcessType = ProcessType.PersonInductionUpdating,
            CreatedOn = legacyEvent.Created,
            UpdatedOn = legacyEvent.Created,
            UserId = updated.RaisedBy.UserId,
            DqtUserId = updated.RaisedBy.DqtUserId,
            DqtUserName = updated.RaisedBy.DqtUserName,
            PersonIds = [.. newEvent.PersonIds],
            OneLoginUserSubjects = [],
            SupportTaskReferences = [],
            ChangeReason = updated.ChangeReason is null &&
                updated.ChangeReasonDetail is null &&
                updated.EvidenceFile is null &&
                updated.AdditionalInformation is null
                ? null
                : new ChangeReasonWithDetailsAndEvidence
                {
                    Reason = updated.ChangeReason,
                    Details = updated.ChangeReasonDetail,
                    EvidenceFile = updated.EvidenceFile,
                    AdditionalInformation = updated.AdditionalInformation
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
