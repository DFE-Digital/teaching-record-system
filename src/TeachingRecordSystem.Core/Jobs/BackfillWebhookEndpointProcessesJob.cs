using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using LegacyWebhookEndpointCreatedEvent = TeachingRecordSystem.Core.Events.Legacy.WebhookEndpointCreatedEvent;
using LegacyWebhookEndpointDeletedEvent = TeachingRecordSystem.Core.Events.Legacy.WebhookEndpointDeletedEvent;
using LegacyWebhookEndpointUpdatedEvent = TeachingRecordSystem.Core.Events.Legacy.WebhookEndpointUpdatedEvent;
using Process = TeachingRecordSystem.Core.DataStore.Postgres.Models.Process;

namespace TeachingRecordSystem.Core.Jobs;

/// <summary>
/// Back-fills <see cref="Process"/> and <see cref="ProcessEvent"/> records from the legacy
/// <c>WebhookEndpoint*</c> events stored in the <c>events</c> table.
/// </summary>
public class BackfillWebhookEndpointProcessesJob(TrsDbContext dbContext)
{
    // These match the EventName values stored in the events table for the legacy events.
    private static readonly string[] _legacyEventNames =
    [
        typeof(LegacyWebhookEndpointCreatedEvent).Name,
        typeof(LegacyWebhookEndpointUpdatedEvent).Name,
        typeof(LegacyWebhookEndpointDeletedEvent).Name
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
                case LegacyWebhookEndpointCreatedEvent created:
                    await CreateProcessAndProcessEventAsync(
                        legacyEvent,
                        new WebhookEndpointCreatedEvent
                        {
                            EventId = created.EventId,
                            WebhookEndpoint = created.WebhookEndpoint
                        },
                        ProcessType.WebhookEndpointCreating,
                        cancellationToken);
                    break;

                case LegacyWebhookEndpointUpdatedEvent updated:
                    await CreateProcessAndProcessEventAsync(
                        legacyEvent,
                        new WebhookEndpointUpdatedEvent
                        {
                            EventId = updated.EventId,
                            WebhookEndpoint = updated.WebhookEndpoint,
                            Changes = (WebhookEndpointUpdatedEventChanges)(int)updated.Changes
                        },
                        ProcessType.WebhookEndpointUpdating,
                        cancellationToken);
                    break;

                case LegacyWebhookEndpointDeletedEvent deleted:
                    await CreateProcessAndProcessEventAsync(
                        legacyEvent,
                        new WebhookEndpointDeletedEvent
                        {
                            EventId = deleted.EventId,
                            WebhookEndpoint = deleted.WebhookEndpoint
                        },
                        ProcessType.WebhookEndpointDeleting,
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

        // WebhookEndpoint events are not associated with any person, One Login user or support task.
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
