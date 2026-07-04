using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using LegacyApiKeyCreatedEvent = TeachingRecordSystem.Core.Events.Legacy.ApiKeyCreatedEvent;
using LegacyApiKeyUpdatedEvent = TeachingRecordSystem.Core.Events.Legacy.ApiKeyUpdatedEvent;
using Process = TeachingRecordSystem.Core.DataStore.Postgres.Models.Process;

namespace TeachingRecordSystem.Core.Jobs;

/// <summary>
/// Back-fills <see cref="Process"/> and <see cref="ProcessEvent"/> records from the legacy
/// <c>ApiKey*</c> events stored in the <c>events</c> table.
/// </summary>
public class BackfillApiKeyProcessesJob(TrsDbContext dbContext)
{
    // These match the EventName values stored in the events table for the legacy events.
    private static readonly string[] _legacyEventNames =
    [
        typeof(LegacyApiKeyCreatedEvent).Name,
        typeof(LegacyApiKeyUpdatedEvent).Name
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

            switch (eventData)
            {
                case LegacyApiKeyCreatedEvent created:
                    await CreateProcessAndProcessEventAsync(
                        legacyEvent,
                        new ApiKeyCreatedEvent
                        {
                            EventId = created.EventId,
                            ApiKey = created.ApiKey
                        },
                        ProcessType.ApiKeyCreating,
                        cancellationToken);
                    break;

                case LegacyApiKeyUpdatedEvent updated:
                    await CreateProcessAndProcessEventAsync(
                        legacyEvent,
                        new ApiKeyUpdatedEvent
                        {
                            EventId = updated.EventId,
                            ApiKey = updated.ApiKey,
                            OldApiKey = updated.OldApiKey,
                            Changes = (ApiKeyUpdatedEventChanges)(int)updated.Changes
                        },
                        ProcessType.ApiKeyUpdating,
                        cancellationToken);
                    break;
            }
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

    private async Task CreateProcessAndProcessEventAsync(
        Event legacyEvent,
        IEvent newEvent,
        ProcessType processType,
        CancellationToken cancellationToken)
    {
        var legacyEventData = legacyEvent.ToEventBase();
        var processId = Guid.NewGuid();

        // ApiKey events are not associated with any person, One Login user or support task.
        var process = new Process
        {
            ProcessId = processId,
            ProcessType = processType,
            CreatedOn = legacyEvent.Created,
            UpdatedOn = legacyEvent.Created,
            UserId = legacyEventData.RaisedBy.UserId,
            DqtUserId = legacyEventData.RaisedBy.DqtUserId,
            DqtUserName = legacyEventData.RaisedBy.DqtUserName,
            PersonIds = [],
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
            PersonIds = [],
            OneLoginUserSubjects = [],
            SupportTaskReferences = [],
            CreatedOn = legacyEvent.Created
        };

        dbContext.ProcessEvents.Add(processEvent);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
