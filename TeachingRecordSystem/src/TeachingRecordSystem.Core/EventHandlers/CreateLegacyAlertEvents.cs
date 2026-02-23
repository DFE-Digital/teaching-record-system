using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Events.ChangeReasons;

namespace TeachingRecordSystem.Core.EventHandlers;

public class CreateLegacyAlertEvents(TrsDbContext dbContext) :
    IEventHandler<AlertCreatedEvent>,
    IEventHandler<AlertUpdatedEvent>,
    IEventHandler<AlertDeletedEvent>
{
    public async Task HandleEventAsync(AlertCreatedEvent @event, ProcessContext processContext, IEventScope eventScope)
    {
        if (processContext.ProcessType is ProcessType.AlertCreating)
        {
            var changeReason = (ChangeReasonWithDetailsAndEvidence)processContext.Process.ChangeReason!;

            var legacyEvent = new LegacyEvents.AlertCreatedEvent
            {
                EventId = @event.EventId,
                CreatedUtc = processContext.Now,
                RaisedBy = EventModels.RaisedByUserInfo.FromUserId(processContext.Process.UserId!.Value),
                PersonId = @event.PersonId,
                Alert = @event.Alert,
                AddReason = changeReason?.Reason,
                AddReasonDetail = changeReason?.Details,
                EvidenceFile = changeReason?.EvidenceFile
            };

            dbContext.AddEventWithoutBroadcast(legacyEvent);

            await dbContext.SaveChangesAsync();
        }
    }

    public async Task HandleEventAsync(AlertUpdatedEvent @event, ProcessContext processContext, IEventScope eventScope)
    {
        if (processContext.ProcessType is ProcessType.AlertUpdating)
        {
            var changeReason = (ChangeReasonWithDetailsAndEvidence)processContext.Process.ChangeReason!;

            var legacyEvent = new LegacyEvents.AlertUpdatedEvent
            {
                EventId = @event.EventId,
                CreatedUtc = processContext.Now,
                RaisedBy = EventModels.RaisedByUserInfo.FromUserId(processContext.Process.UserId!.Value),
                PersonId = @event.PersonId,
                Alert = @event.Alert,
                OldAlert = @event.OldAlert,
                ChangeReason = changeReason?.Reason,
                ChangeReasonDetail = changeReason?.Details,
                EvidenceFile = changeReason?.EvidenceFile,
                Changes = @event.Changes.ToLegacyAlertUpdatedEventChanges()
            };

            dbContext.AddEventWithoutBroadcast(legacyEvent);

            await dbContext.SaveChangesAsync();
        }
    }

    public async Task HandleEventAsync(AlertDeletedEvent @event, ProcessContext processContext, IEventScope eventScope)
    {
        if (processContext.ProcessType is ProcessType.AlertDeleting)
        {
            var changeReason = (ChangeReasonWithDetailsAndEvidence)processContext.Process.ChangeReason!;

            var legacyEvent = new LegacyEvents.AlertDeletedEvent
            {
                EventId = @event.EventId,
                CreatedUtc = processContext.Now,
                RaisedBy = EventModels.RaisedByUserInfo.FromUserId(processContext.Process.UserId!.Value),
                PersonId = @event.PersonId,
                Alert = @event.Alert,
                DeletionReasonDetail = changeReason?.Details,
                EvidenceFile = changeReason?.EvidenceFile
            };

            dbContext.AddEventWithoutBroadcast(legacyEvent);

            await dbContext.SaveChangesAsync();
        }
    }
}
