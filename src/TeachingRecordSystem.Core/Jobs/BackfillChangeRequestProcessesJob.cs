using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using LegacyChangeDateOfBirthRequestSupportTaskCancelledEvent = TeachingRecordSystem.Core.Events.Legacy.ChangeDateOfBirthRequestSupportTaskCancelledEvent;
using LegacyChangeDateOfBirthRequestSupportTaskRejectedEvent = TeachingRecordSystem.Core.Events.Legacy.ChangeDateOfBirthRequestSupportTaskRejectedEvent;
using LegacyChangeNameRequestSupportTaskCancelledEvent = TeachingRecordSystem.Core.Events.Legacy.ChangeNameRequestSupportTaskCancelledEvent;
using LegacyChangeNameRequestSupportTaskRejectedEvent = TeachingRecordSystem.Core.Events.Legacy.ChangeNameRequestSupportTaskRejectedEvent;
using LegacySupportTaskUpdatedEvent = TeachingRecordSystem.Core.Events.Legacy.SupportTaskUpdatedEvent;
using Process = TeachingRecordSystem.Core.DataStore.Postgres.Models.Process;

namespace TeachingRecordSystem.Core.Jobs;

/// <summary>
/// Back-fills <see cref="Process"/> and <see cref="ProcessEvent"/> records from the legacy change
/// request reject/cancel events stored in the <c>events</c> table. Historically these operations
/// wrote only the legacy event directly and did not go through the event pipeline, so — unlike the
/// approve path — they have no corresponding process.
/// </summary>
public class BackfillChangeRequestProcessesJob(TrsDbContext dbContext)
{
    // These match the EventName values stored in the events table for the legacy events.
    private static readonly string[] _legacyEventNames =
    [
        typeof(LegacyChangeNameRequestSupportTaskRejectedEvent).Name,
        typeof(LegacyChangeDateOfBirthRequestSupportTaskRejectedEvent).Name,
        typeof(LegacyChangeNameRequestSupportTaskCancelledEvent).Name,
        typeof(LegacyChangeDateOfBirthRequestSupportTaskCancelledEvent).Name
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

            var (newEvent, processType) = eventData switch
            {
                LegacyChangeNameRequestSupportTaskRejectedEvent rejected =>
                    (CreateSupportTaskUpdatedEvent(rejected, rejected.RejectionReason), ProcessType.ChangeOfNameRequestRejecting),
                LegacyChangeDateOfBirthRequestSupportTaskRejectedEvent rejected =>
                    (CreateSupportTaskUpdatedEvent(rejected, rejected.RejectionReason), ProcessType.ChangeOfDateOfBirthRequestRejecting),
                LegacyChangeNameRequestSupportTaskCancelledEvent cancelled =>
                    (CreateSupportTaskUpdatedEvent(cancelled, rejectionReason: null), ProcessType.ChangeOfNameRequestCancelling),
                LegacyChangeDateOfBirthRequestSupportTaskCancelledEvent cancelled =>
                    (CreateSupportTaskUpdatedEvent(cancelled, rejectionReason: null), ProcessType.ChangeOfDateOfBirthRequestCancelling),
                _ => throw new InvalidOperationException($"Unexpected event type: '{eventData.GetType().Name}'.")
            };

            await CreateProcessAndProcessEventAsync(legacyEvent, newEvent, processType, cancellationToken);
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

    // The reject/cancel support task update always closes the task (Status) and stamps the outcome (Data).
    private static SupportTaskUpdatedEvent CreateSupportTaskUpdatedEvent(
        LegacySupportTaskUpdatedEvent legacyEvent,
        string? rejectionReason) =>
        new()
        {
            EventId = legacyEvent.EventId,
            SupportTaskReference = legacyEvent.SupportTask.SupportTaskReference,
            Changes = SupportTaskUpdatedEventChanges.Status | SupportTaskUpdatedEventChanges.Data,
            SupportTask = legacyEvent.SupportTask,
            OldSupportTask = legacyEvent.OldSupportTask,
            Comments = null,
            RejectionReason = rejectionReason
        };

    private async Task CreateProcessAndProcessEventAsync(
        Event legacyEvent,
        IEvent newEvent,
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
            PersonIds = [.. newEvent.PersonIds],
            OneLoginUserSubjects = [.. newEvent.OneLoginUserSubjects],
            SupportTaskReferences = [.. newEvent.SupportTaskReferences],
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

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
