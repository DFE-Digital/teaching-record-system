using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using LegacyAlertDqtDeactivatedEvent = TeachingRecordSystem.Core.Events.Legacy.AlertDqtDeactivatedEvent;
using LegacyAlertDqtImportedEvent = TeachingRecordSystem.Core.Events.Legacy.AlertDqtImportedEvent;
using LegacyAlertDqtReactivatedEvent = TeachingRecordSystem.Core.Events.Legacy.AlertDqtReactivatedEvent;
using LegacyAlertMigratedEvent = TeachingRecordSystem.Core.Events.Legacy.AlertMigratedEvent;
using Process = TeachingRecordSystem.Core.DataStore.Postgres.Models.Process;

namespace TeachingRecordSystem.Core.Jobs;

/// <summary>
/// Back-fills <see cref="Process"/> and <see cref="ProcessEvent"/> records from the legacy DQT-era alert
/// deactivated/imported/reactivated/migrated events stored in the <c>events</c> table.
/// </summary>
public class BackfillAlertDqtProcessesJob(TrsDbContext dbContext)
{
    // These match the EventName values stored in the events table for the legacy events.
    private static readonly string[] _legacyEventNames =
    [
        typeof(LegacyAlertDqtDeactivatedEvent).Name,
        typeof(LegacyAlertDqtImportedEvent).Name,
        typeof(LegacyAlertDqtReactivatedEvent).Name,
        typeof(LegacyAlertMigratedEvent).Name
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
                case LegacyAlertDqtDeactivatedEvent deactivated:
                    await CreateProcessAndProcessEventAsync(
                        legacyEvent,
                        new AlertDqtDeactivatedEvent
                        {
                            EventId = deactivated.EventId,
                            PersonId = deactivated.PersonId,
                            Alert = deactivated.Alert
                        },
                        ProcessType.AlertDeactivatingInDqt,
                        cancellationToken);
                    break;

                case LegacyAlertDqtImportedEvent imported:
                    await CreateProcessAndProcessEventAsync(
                        legacyEvent,
                        new AlertDqtImportedEvent
                        {
                            EventId = imported.EventId,
                            PersonId = imported.PersonId,
                            Alert = imported.Alert,
                            DqtState = imported.DqtState
                        },
                        ProcessType.AlertImportingIntoDqt,
                        cancellationToken);
                    break;

                case LegacyAlertDqtReactivatedEvent reactivated:
                    await CreateProcessAndProcessEventAsync(
                        legacyEvent,
                        new AlertDqtReactivatedEvent
                        {
                            EventId = reactivated.EventId,
                            PersonId = reactivated.PersonId,
                            Alert = reactivated.Alert
                        },
                        ProcessType.AlertReactivatingInDqt,
                        cancellationToken);
                    break;

                case LegacyAlertMigratedEvent migrated:
                    await CreateProcessAndProcessEventAsync(
                        legacyEvent,
                        new AlertMigratedEvent
                        {
                            EventId = migrated.EventId,
                            PersonId = migrated.PersonId,
                            Alert = migrated.Alert,
                            OldAlert = migrated.OldAlert
                        },
                        ProcessType.AlertMigratingFromDqt,
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

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
