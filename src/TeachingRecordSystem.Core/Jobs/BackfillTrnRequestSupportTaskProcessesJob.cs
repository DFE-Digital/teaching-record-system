using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using LegacySupportTaskCreatedEvent = TeachingRecordSystem.Core.Events.Legacy.SupportTaskCreatedEvent;
using Process = TeachingRecordSystem.Core.DataStore.Postgres.Models.Process;

namespace TeachingRecordSystem.Core.Jobs;

/// <summary>
/// Back-fills <see cref="Process"/> and <see cref="ProcessEvent"/> records for the TRN request support tasks that
/// were created before their journeys moved onto the event pipeline. The API's <c>CreateTrnRequest</c> operation
/// and the NPQ 'request a TRN' pages wrote the legacy <c>SupportTaskCreatedEvent</c> straight to the <c>events</c>
/// table, so the task's creation was never recorded against a process.
///
/// Nothing else from those journeys survived as a legacy event — TRN request creation itself never had one — so
/// each back-filled process holds the <see cref="SupportTaskCreatedEvent"/> alone.
///
/// The <c>TeacherPensionsPotentialDuplicate</c> tasks the Teachers' Pensions import left behind are the other half
/// of the legacy <c>SupportTaskCreatedEvent</c>s;
/// <see cref="BackfillTeacherPensionsSupportTaskProcessesJob"/> back-fills those, attaching them to the import
/// process that created the person rather than to a process of their own.
/// </summary>
/// <remarks>
/// Events are migrated in batches, each committed on its own. Batches are paged through on the
/// (created, event_id) key rather than repeatedly asking for the events that haven't been migrated yet; the latter
/// would rescan everything already done on every batch.
/// </remarks>
public class BackfillTrnRequestSupportTaskProcessesJob(TrsDbContext dbContext)
{
    private const int BatchSize = 1000;

    // This matches the EventName value stored in the events table for the legacy event.
    private static readonly string _legacyEventName = typeof(LegacySupportTaskCreatedEvent).Name;

    private static readonly int[] _supportTaskTypes =
    [
        (int)SupportTaskType.TrnRequest,
        (int)SupportTaskType.NpqTrnRequest,
        (int)SupportTaskType.TrnRequestManualChecksNeeded
    ];

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        dbContext.Database.SetCommandTimeout(0);

        var lastCreated = DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc);
        var lastEventId = Guid.Empty;

        while (!cancellationToken.IsCancellationRequested)
        {
            var batch = await dbContext.Events
                .FromSql($"""
                    select e.* from events e
                    where e.event_name = {_legacyEventName}
                    and (e.payload -> 'SupportTask' ->> 'SupportTaskType')::int = any({_supportTaskTypes})
                    and (e.created > {lastCreated} or (e.created = {lastCreated} and e.event_id > {lastEventId}))
                    order by e.created, e.event_id
                    limit {BatchSize}
                    """)
                .AsNoTracking()
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
            var alreadyBackfilledEventIds = (await dbContext.ProcessEvents
                    .Where(pe => batchEventIds.Contains(pe.ProcessEventId))
                    .Select(pe => pe.ProcessEventId)
                    .ToListAsync(cancellationToken))
                .ToHashSet();

            var toBackfill = batch
                .Where(e => !alreadyBackfilledEventIds.Contains(e.EventId))
                .Select(e => (Row: e, Payload: (LegacySupportTaskCreatedEvent)e.ToEventBase()))
                .ToArray();

            if (toBackfill.Length > 0)
            {
                var applicationUserIds = await GetApplicationUserIdsAsync(toBackfill.Select(e => e.Payload), cancellationToken);

                foreach (var (row, payload) in toBackfill)
                {
                    AddProcessAndProcessEvent(row, payload, applicationUserIds);
                }

                await dbContext.SaveChangesAsync(cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);

            // Otherwise the change tracker keeps every batch's entities alive for the lifetime of the job.
            dbContext.ChangeTracker.Clear();
        }
    }

    private async Task<HashSet<Guid>> GetApplicationUserIdsAsync(
        IEnumerable<LegacySupportTaskCreatedEvent> payloads,
        CancellationToken cancellationToken)
    {
        var userIds = payloads
            .Select(e => e.RaisedBy.UserId)
            .Where(id => id is not null)
            .Select(id => id!.Value)
            .Distinct()
            .ToArray();

        return (await dbContext.ApplicationUsers
                .Where(u => userIds.Contains(u.UserId))
                .Select(u => u.UserId)
                .ToListAsync(cancellationToken))
            .ToHashSet();
    }

    // Who raised the event says which journey created the task: the API and the NPQ pages both raised it as the
    // application user the request came from, where a support user only ever created one part-way through resolving
    // a TRN request.
    private static ProcessType GetProcessType(LegacySupportTaskCreatedEvent legacyEvent, bool raisedByApplicationUser) =>
        (legacyEvent.SupportTask.SupportTaskType, raisedByApplicationUser) switch
        {
            (SupportTaskType.TrnRequest, true) => ProcessType.TrnRequestCreating,
            (SupportTaskType.TrnRequestManualChecksNeeded, true) => ProcessType.TrnRequestCreating,
            (SupportTaskType.TrnRequestManualChecksNeeded, false) => ProcessType.TrnRequestResolving,
            (SupportTaskType.NpqTrnRequest, _) => ProcessType.NpqTrnRequestTaskCreating,
            _ => throw new InvalidOperationException(
                $"Don't know which process created the {legacyEvent.SupportTask.SupportTaskType} support task " +
                $"'{legacyEvent.SupportTask.SupportTaskReference}'.")
        };

    private void AddProcessAndProcessEvent(Event row, LegacySupportTaskCreatedEvent payload, HashSet<Guid> applicationUserIds)
    {
        // These events predate support task references moving onto a database sequence, so they carry the reference
        // the caller generated. The TPS import's events, built from an unsaved task, lost theirs — don't write a
        // process event with nothing to key the support task's change history on.
        if (string.IsNullOrEmpty(payload.SupportTask.SupportTaskReference))
        {
            throw new InvalidOperationException(
                $"Legacy {nameof(SupportTaskCreatedEvent)} '{payload.EventId}' has no support task reference.");
        }

        var raisedByApplicationUser = payload.RaisedBy.UserId is { } userId && applicationUserIds.Contains(userId);
        var processType = GetProcessType(payload, raisedByApplicationUser);

        IEvent newEvent = new SupportTaskCreatedEvent
        {
            EventId = payload.EventId,
            SupportTask = payload.SupportTask
        };

        var processId = Guid.NewGuid();

        dbContext.Processes.Add(new Process
        {
            ProcessId = processId,
            ProcessType = processType,
            CreatedOn = row.Created,
            UpdatedOn = row.Created,
            UserId = payload.RaisedBy.UserId,
            DqtUserId = payload.RaisedBy.DqtUserId,
            DqtUserName = payload.RaisedBy.DqtUserName,
            PersonIds = [.. newEvent.PersonIds],
            OneLoginUserSubjects = [.. newEvent.OneLoginUserSubjects],
            SupportTaskReferences = [.. newEvent.SupportTaskReferences],
            ChangeReason = null
        });

        dbContext.ProcessEvents.Add(new ProcessEvent
        {
            ProcessEventId = newEvent.EventId,
            ProcessId = processId,
            EventName = newEvent.GetType().Name,
            Payload = newEvent,
            PersonIds = newEvent.PersonIds,
            OneLoginUserSubjects = newEvent.OneLoginUserSubjects,
            SupportTaskReferences = newEvent.SupportTaskReferences,
            CreatedOn = row.Created
        });
    }
}
