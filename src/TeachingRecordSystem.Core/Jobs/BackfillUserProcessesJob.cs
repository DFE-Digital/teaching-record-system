using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using LegacyApplicationUserCreatedEvent = TeachingRecordSystem.Core.Events.Legacy.ApplicationUserCreatedEvent;
using LegacyApplicationUserUpdatedEvent = TeachingRecordSystem.Core.Events.Legacy.ApplicationUserUpdatedEvent;
using LegacyUserActivatedEvent = TeachingRecordSystem.Core.Events.Legacy.UserActivatedEvent;
using LegacyUserAddedEvent = TeachingRecordSystem.Core.Events.Legacy.UserAddedEvent;
using LegacyUserDeactivatedEvent = TeachingRecordSystem.Core.Events.Legacy.UserDeactivatedEvent;
using LegacyUserUpdatedEvent = TeachingRecordSystem.Core.Events.Legacy.UserUpdatedEvent;
using Process = TeachingRecordSystem.Core.DataStore.Postgres.Models.Process;

namespace TeachingRecordSystem.Core.Jobs;

/// <summary>
/// Back-fills <see cref="Process"/> and <see cref="ProcessEvent"/> records from the legacy
/// <c>*User</c> and <c>*ApplicationUser</c> events stored in the <c>events</c> table.
/// </summary>
public class BackfillUserProcessesJob(TrsDbContext dbContext)
{
    // These match the EventName values stored in the events table for the legacy events.
    private static readonly string[] _legacyEventNames =
    [
        typeof(LegacyUserAddedEvent).Name,
        typeof(LegacyUserUpdatedEvent).Name,
        typeof(LegacyUserActivatedEvent).Name,
        typeof(LegacyUserDeactivatedEvent).Name,
        typeof(LegacyApplicationUserCreatedEvent).Name,
        typeof(LegacyApplicationUserUpdatedEvent).Name
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
                case LegacyUserAddedEvent added:
                    await CreateProcessAndProcessEventAsync(
                        legacyEvent,
                        new UserAddedEvent
                        {
                            EventId = added.EventId,
                            User = added.User
                        },
                        ProcessType.UserAdding,
                        cancellationToken);
                    break;

                case LegacyUserUpdatedEvent updated:
                    await CreateProcessAndProcessEventAsync(
                        legacyEvent,
                        new UserUpdatedEvent
                        {
                            EventId = updated.EventId,
                            User = updated.User,
                            Changes = (UserUpdatedEventChanges)(int)updated.Changes
                        },
                        ProcessType.UserUpdating,
                        cancellationToken);
                    break;

                case LegacyUserActivatedEvent activated:
                    await CreateProcessAndProcessEventAsync(
                        legacyEvent,
                        new UserActivatedEvent
                        {
                            EventId = activated.EventId,
                            User = activated.User
                        },
                        ProcessType.UserActivating,
                        cancellationToken);
                    break;

                case LegacyUserDeactivatedEvent deactivated:
                    await CreateProcessAndProcessEventAsync(
                        legacyEvent,
                        new UserDeactivatedEvent
                        {
                            EventId = deactivated.EventId,
                            User = deactivated.User,
                            DeactivatedReason = deactivated.DeactivatedReason,
                            DeactivatedReasonDetail = deactivated.DeactivatedReasonDetail,
                            EvidenceFileId = deactivated.EvidenceFileId
                        },
                        ProcessType.UserDeactivating,
                        cancellationToken);
                    break;

                case LegacyApplicationUserCreatedEvent created:
                    await CreateProcessAndProcessEventAsync(
                        legacyEvent,
                        new ApplicationUserCreatedEvent
                        {
                            EventId = created.EventId,
                            ApplicationUser = created.ApplicationUser
                        },
                        ProcessType.ApplicationUserCreating,
                        cancellationToken);
                    break;

                case LegacyApplicationUserUpdatedEvent updated:
                    await CreateProcessAndProcessEventAsync(
                        legacyEvent,
                        new ApplicationUserUpdatedEvent
                        {
                            EventId = updated.EventId,
                            ApplicationUser = updated.ApplicationUser,
                            OldApplicationUser = updated.OldApplicationUser,
                            Changes = (ApplicationUserUpdatedEventChanges)(int)updated.Changes
                        },
                        ProcessType.ApplicationUserUpdating,
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

        // *User and *ApplicationUser events are not associated with any person, One Login user or support task.
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
