using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Events.ChangeReasons;

namespace TeachingRecordSystem.Core.EventHandlers;

public class CreateLegacyPersonEvents(TrsDbContext dbContext) :
    IEventHandler<PersonDetailsUpdatedEvent>
{
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
}
