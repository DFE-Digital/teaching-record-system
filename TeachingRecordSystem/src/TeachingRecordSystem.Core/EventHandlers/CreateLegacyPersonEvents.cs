using System.Diagnostics;
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
            var changeReason = (PersonDetailsChangeReasonInfo)processContext.Process.ChangeReason!;

            var legacyEvent = new LegacyEvents.PersonDetailsUpdatedEvent
            {
                EventId = @event.EventId,
                CreatedUtc = processContext.Now,
                RaisedBy = processContext.Process.UserId!,
                PersonId = @event.PersonId,
                Changes = @event.Changes.ToLegacyPersonDetailsUpdatedEventChanges(),
                OldPersonAttributes = @event.OldPersonDetails,
                PersonAttributes = @event.PersonDetails,
                NameChangeReason = changeReason.NameChangeReason,
                NameChangeEvidenceFile = changeReason.NameChangeEvidenceFile,
                DetailsChangeReason = changeReason.Reason,
                DetailsChangeReasonDetail = changeReason.Details,
                DetailsChangeEvidenceFile = changeReason.EvidenceFile
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
                DateOfDeath = null,
                Reason = changeReason.Reason,
                ReasonDetail = changeReason.Details,
                EvidenceFile = changeReason.EvidenceFile
            };

            dbContext.AddEventWithoutBroadcast(legacyEvent);

            await dbContext.SaveChangesAsync();
        }
        else if (processContext.ProcessType is ProcessType.PersonMerging)
        {
            Debug.Assert(@event.MergedWithPersonId is not null);
            var mergedWithPersonId = @event.MergedWithPersonId!.Value;
            var mergedWithPersonTrn = (await dbContext.Persons.FindAsync(mergedWithPersonId))!.Trn;

            var deactivatedPersonTrn = (await dbContext.Persons.FindAsync(@event.PersonId))!.Trn;

            var mergedWithPersonUpdatedEvent = processContext.Events.OfType<PersonDetailsUpdatedEvent>().SingleOrDefault();
            Debug.Assert(mergedWithPersonUpdatedEvent is null || mergedWithPersonUpdatedEvent.PersonId == @event.MergedWithPersonId);
            var mergedWithPerson = (await dbContext.Persons.FindAsync(mergedWithPersonId))!;

            var changeReason = (ChangeReasonWithDetailsAndEvidence)processContext.Process.ChangeReason!;

            var changes = mergedWithPersonUpdatedEvent is null ?
                LegacyEvents.PersonsMergedEventChanges.None :
                (LegacyEvents.PersonsMergedEventChanges)((int)(mergedWithPersonUpdatedEvent.Changes) << 16);

            var legacyEvent = new LegacyEvents.PersonsMergedEvent
            {
                EventId = @event.EventId,
                CreatedUtc = processContext.Now,
                RaisedBy = processContext.Process.UserId!,
                PersonId = mergedWithPersonId,
                PersonTrn = mergedWithPersonTrn,
                SecondaryPersonId = @event.PersonId,
                SecondaryPersonTrn = deactivatedPersonTrn,
                SecondaryPersonStatus = PersonStatus.Deactivated,
                Changes = changes,
                PersonAttributes = mergedWithPersonUpdatedEvent?.PersonDetails ?? EventModels.PersonDetails.FromModel(mergedWithPerson),
                OldPersonAttributes = mergedWithPersonUpdatedEvent?.OldPersonDetails ?? EventModels.PersonDetails.FromModel(mergedWithPerson),
                EvidenceFile = changeReason.EvidenceFile,
                Comments = changeReason.Details
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
                EvidenceFile = changeReason.EvidenceFile
            };

            dbContext.AddEventWithoutBroadcast(legacyEvent);

            await dbContext.SaveChangesAsync();
        }
    }
}
