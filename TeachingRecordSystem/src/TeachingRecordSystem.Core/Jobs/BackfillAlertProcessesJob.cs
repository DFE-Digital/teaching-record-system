using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Events.ChangeReasons;
using LegacyAlertCreatedEvent = TeachingRecordSystem.Core.Events.Legacy.AlertCreatedEvent;
using LegacyAlertDeletedEvent = TeachingRecordSystem.Core.Events.Legacy.AlertDeletedEvent;
using LegacyAlertUpdatedEvent = TeachingRecordSystem.Core.Events.Legacy.AlertUpdatedEvent;
using Process = TeachingRecordSystem.Core.DataStore.Postgres.Models.Process;

namespace TeachingRecordSystem.Core.Jobs;

public class BackfillAlertProcessesJob(TrsDbContext dbContext)
{
    public async Task ExecuteAsync(bool dryRun, CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        var alertEvents = await dbContext.Events
            .Where(e => e.EventName == "AlertCreatedEvent" ||
                        e.EventName == "AlertUpdatedEvent" ||
                        e.EventName == "AlertDeletedEvent")
            .Where(e => !dbContext.ProcessEvents.Any(pe => pe.ProcessEventId == e.EventId))
            .OrderBy(e => e.Created)
            .ToListAsync(cancellationToken);

        foreach (var legacyEvent in alertEvents)
        {
            var eventData = legacyEvent.ToEventBase();

            switch (eventData)
            {
                case LegacyAlertCreatedEvent createdEvent:
                    {
                        var changeReason = ExtractCreatedEventReason(legacyEvent, createdEvent);
                        var newEvent = new AlertCreatedEvent
                        {
                            EventId = createdEvent.EventId,
                            PersonId = createdEvent.PersonId,
                            Alert = createdEvent.Alert
                        };
                        await CreateProcessAndProcessEventAsync(
                            legacyEvent,
                            newEvent,
                            ProcessType.AlertCreating,
                            changeReason,
                            cancellationToken);
                        break;
                    }

                case LegacyAlertUpdatedEvent updatedEvent:
                    {
                        var changeReason = new ChangeReasonWithDetailsAndEvidence
                        {
                            Reason = updatedEvent.ChangeReason,
                            Details = updatedEvent.ChangeReasonDetail,
                            EvidenceFile = updatedEvent.EvidenceFile
                        };
                        var newEvent = new AlertUpdatedEvent
                        {
                            EventId = updatedEvent.EventId,
                            PersonId = updatedEvent.PersonId,
                            Alert = updatedEvent.Alert,
                            OldAlert = updatedEvent.OldAlert,
                            Changes = Events.Legacy.AlertUpdatedEventChangesExtensions.ToAlertUpdatedEventChanges(updatedEvent.Changes)
                        };
                        await CreateProcessAndProcessEventAsync(
                            legacyEvent,
                            newEvent,
                            ProcessType.AlertUpdating,
                            changeReason,
                            cancellationToken);
                        break;
                    }

                case LegacyAlertDeletedEvent deletedEvent:
                    {
                        var changeReason = new ChangeReasonWithDetailsAndEvidence
                        {
                            Reason = null,
                            Details = deletedEvent.DeletionReasonDetail,
                            EvidenceFile = deletedEvent.EvidenceFile
                        };
                        var newEvent = new AlertDeletedEvent
                        {
                            EventId = deletedEvent.EventId,
                            PersonId = deletedEvent.PersonId,
                            Alert = deletedEvent.Alert
                        };
                        await CreateProcessAndProcessEventAsync(
                            legacyEvent,
                            newEvent,
                            ProcessType.AlertDeleting,
                            changeReason,
                            cancellationToken);
                        break;
                    }
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
        ChangeReasonWithDetailsAndEvidence changeReason,
        CancellationToken cancellationToken)
    {
        var processId = Guid.NewGuid();
        var createdOn = legacyEvent.Created;
        var updatedOn = legacyEvent.Created;

        var legacyEventData = legacyEvent.ToEventBase();

        var process = new Process
        {
            ProcessId = processId,
            ProcessType = processType,
            CreatedOn = createdOn,
            UpdatedOn = updatedOn,
            UserId = legacyEventData.RaisedBy.UserId,
            DqtUserId = legacyEventData.RaisedBy.DqtUserId,
            DqtUserName = legacyEventData.RaisedBy.DqtUserName,
            PersonIds = legacyEvent.PersonIds.ToList(),
            ChangeReason = changeReason
        };

        dbContext.Processes.Add(process);

        var processEvent = new ProcessEvent
        {
            ProcessEventId = newEvent.EventId,
            ProcessId = processId,
            EventName = newEvent.GetType().Name,
            Payload = newEvent,
            PersonIds = legacyEvent.PersonIds,
            OneLoginUserSubjects = [],
            CreatedOn = createdOn
        };

        dbContext.ProcessEvents.Add(processEvent);

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private ChangeReasonWithDetailsAndEvidence ExtractCreatedEventReason(Event legacyEvent, LegacyAlertCreatedEvent eventData)
    {
        // Handle schema change in AlertCreatedEvent:
        // - Old schema (pre-2024): had "Reason" field only
        // - New schema: has "AddReason" and "AddReasonDetail" fields        
        string? reason = eventData.AddReason;
        string? reasonDetail = eventData.AddReasonDetail;

        if (string.IsNullOrEmpty(reason))
        {
            var payloadJson = System.Text.Json.JsonDocument.Parse(legacyEvent.Payload);
            if (payloadJson.RootElement.TryGetProperty("Reason", out var reasonElement))
            {
                reason = reasonElement.GetString();
            }
        }

        return new ChangeReasonWithDetailsAndEvidence
        {
            Reason = reason,
            Details = reasonDetail,
            EvidenceFile = eventData.EvidenceFile
        };
    }
}
