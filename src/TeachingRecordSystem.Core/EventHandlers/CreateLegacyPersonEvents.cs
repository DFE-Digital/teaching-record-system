using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Events.ChangeReasons;

namespace TeachingRecordSystem.Core.EventHandlers;

public class CreateLegacyPersonEvents(TrsDbContext dbContext) :
    IEventHandler<PersonCreatedEvent>,
    IEventHandler<PersonDetailsUpdatedEvent>,
    IEventHandler<PersonDeactivatedEvent>,
    IEventHandler<PersonReactivatedEvent>
{
    public async Task HandleEventAsync(PersonCreatedEvent @event, ProcessContext processContext, IEventScope eventScope)
    {
        if (processContext.ProcessType is ProcessType.PersonCreating
            or ProcessType.TeacherPensionsRecordImporting)
        {
            var changeReason = processContext.Process.ChangeReason as ChangeReasonWithDetailsAndEvidence;

            var legacyEvent = new LegacyEvents.PersonCreatedEvent
            {
                EventId = @event.EventId,
                CreatedUtc = processContext.Now,
                RaisedBy = processContext.Process.UserId!,
                PersonId = @event.PersonId,
                PersonAttributes = @event.Details,
                CreateReason = changeReason?.Reason,
                CreateReasonDetail = changeReason?.Details,
                CreateAdditionalInformation = changeReason?.AdditionalInformation,
                EvidenceFile = changeReason?.EvidenceFile,
                TrnRequestMetadata = @event.TrnRequestMetadata
            };

            dbContext.AddEventWithoutBroadcast(legacyEvent);

            await dbContext.SaveChangesAsync();
        }
    }

    public async Task HandleEventAsync(PersonDetailsUpdatedEvent @event, ProcessContext processContext, IEventScope eventScope)
    {
        if (processContext.ProcessType is ProcessType.PersonDetailsUpdating)
        {
            var changeReason = processContext.Process.ChangeReason as PersonDetailsChangeReasonInfo;

            var legacyEvent = new LegacyEvents.PersonDetailsUpdatedEvent
            {
                EventId = @event.EventId,
                CreatedUtc = processContext.Now,
                RaisedBy = processContext.Process.UserId!,
                PersonId = @event.PersonId,
                Changes = (LegacyEvents.PersonDetailsUpdatedEventChanges)((int)@event.Changes << 16),
                OldPersonAttributes = @event.OldPersonDetails,
                PersonAttributes = @event.PersonDetails,
                NameChangeReason = changeReason?.NameChangeReason,
                NameChangeEvidenceFile = changeReason?.NameChangeEvidenceFile,
                DetailsChangeReason = changeReason?.Reason,
                DetailsChangeReasonDetail = changeReason?.Details,
                DetailsChangeEvidenceFile = changeReason?.EvidenceFile
            };

            dbContext.AddEventWithoutBroadcast(legacyEvent);

            await dbContext.SaveChangesAsync();
        }
    }

    public async Task HandleEventAsync(PersonDeactivatedEvent @event, ProcessContext processContext, IEventScope eventScope)
    {
        if (processContext.ProcessType is ProcessType.PersonDeactivating)
        {
            var changeReason = (ChangeReasonWithDetailsAndEvidence)processContext.Process.ChangeReason!;

            var legacyEvent = new LegacyEvents.PersonStatusUpdatedEvent
            {
                EventId = @event.EventId,
                CreatedUtc = processContext.Now,
                RaisedBy = processContext.Process.UserId!,
                PersonId = @event.PersonId,
                OldStatus = PersonStatus.Active,
                Status = PersonStatus.Deactivated,
                DateOfDeath = @event.DateOfDeath,
                Reason = changeReason.Reason,
                ReasonDetail = changeReason.Details,
                EvidenceFile = changeReason.EvidenceFile,
                AdditionalInformation = changeReason.AdditionalInformation
            };

            dbContext.AddEventWithoutBroadcast(legacyEvent);

            await dbContext.SaveChangesAsync();
        }
    }

    public async Task HandleEventAsync(PersonReactivatedEvent @event, ProcessContext processContext, IEventScope eventScope)
    {
        if (processContext.ProcessType is ProcessType.PersonReactivating)
        {
            var changeReason = (ChangeReasonWithDetailsAndEvidence)processContext.Process.ChangeReason!;

            var legacyEvent = new LegacyEvents.PersonStatusUpdatedEvent
            {
                EventId = @event.EventId,
                CreatedUtc = processContext.Now,
                RaisedBy = processContext.Process.UserId!,
                PersonId = @event.PersonId,
                OldStatus = PersonStatus.Deactivated,
                Status = PersonStatus.Active,
                DateOfDeath = null,
                Reason = changeReason.Reason,
                ReasonDetail = changeReason.Details,
                EvidenceFile = changeReason.EvidenceFile,
                AdditionalInformation = changeReason.AdditionalInformation
            };

            dbContext.AddEventWithoutBroadcast(legacyEvent);

            await dbContext.SaveChangesAsync();
        }
    }
}
