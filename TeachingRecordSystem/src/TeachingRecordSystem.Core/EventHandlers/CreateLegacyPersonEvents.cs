using TeachingRecordSystem.Core.DataStore.Postgres;

namespace TeachingRecordSystem.Core.EventHandlers;

public class CreateLegacyPersonEvents(TrsDbContext dbContext) :
    IEventHandler<PersonCreatedEvent>,
    IEventHandler<PersonDetailsUpdatedEvent>,
    IEventHandler<PersonDeactivatedEvent>,
    IEventHandler<PersonReactivatedEvent>,
    IEventHandler<PersonsMergedEvent>
{
    public async Task HandleEventAsync(PersonCreatedEvent @event, ProcessContext processContext)
    {
        if (processContext.ProcessType is ProcessType.PersonCreating
            or ProcessType.TeacherPensionsRecordImporting)
        {
            var legacyEvent = new LegacyEvents.PersonCreatedEvent
            {
                EventId = @event.EventId,
                CreatedUtc = processContext.Now,
                RaisedBy = processContext.Process.UserId!,
                PersonId = @event.PersonId,
                PersonAttributes = @event.Details,
                CreateReason = @event.CreateReason,
                CreateReasonDetail = @event.CreateReasonDetail,
                EvidenceFile = @event.EvidenceFile,
                TrnRequestMetadata = @event.TrnRequestMetadata,
            };

            dbContext.AddEventWithoutBroadcast(legacyEvent);

            await dbContext.SaveChangesAsync();
        }
    }

    public async Task HandleEventAsync(PersonDetailsUpdatedEvent @event, ProcessContext processContext)
    {
        if (processContext.ProcessType is ProcessType.PersonDetailsUpdating)
        //or ProcessType.ApiTrnRequestResolving
        //or ProcessType.NpqTrnRequestApproving
        //or ProcessType.TeacherPensionsDuplicateSupportTaskResolvingWithMerge)
        {
            var legacyEvent = new LegacyEvents.PersonDetailsUpdatedEvent
            {
                EventId = @event.EventId,
                CreatedUtc = processContext.Now,
                RaisedBy = processContext.Process.UserId!,
                PersonId = @event.PersonId,
                Changes = @event.Changes.ToLegacyPersonDetailsUpdatedEventChanges(),
                OldPersonAttributes = @event.OldPersonDetails,
                PersonAttributes = @event.PersonDetails,
                NameChangeReason = @event.NameChangeReason,
                NameChangeEvidenceFile = @event.NameChangeEvidenceFile,
                DetailsChangeReason = @event.DetailsChangeReason,
                DetailsChangeReasonDetail = @event.DetailsChangeReasonDetail,
                DetailsChangeEvidenceFile = @event.DetailsChangeEvidenceFile,
            };

            dbContext.AddEventWithoutBroadcast(legacyEvent);

            await dbContext.SaveChangesAsync();
        }
    }

    public async Task HandleEventAsync(PersonDeactivatedEvent @event, ProcessContext processContext)
    {
        if (processContext.ProcessType is ProcessType.PersonDeactivating)
        {
            var legacyEvent = new LegacyEvents.PersonStatusUpdatedEvent
            {
                EventId = @event.EventId,
                CreatedUtc = processContext.Now,
                RaisedBy = processContext.Process.UserId!,
                PersonId = @event.PersonId,
                OldStatus = PersonStatus.Active,
                Status = PersonStatus.Deactivated,
                DateOfDeath = null,
                Reason = @event.Reason,
                ReasonDetail = @event.ReasonDetail,
                EvidenceFile = @event.EvidenceFile,
            };

            dbContext.AddEventWithoutBroadcast(legacyEvent);

            await dbContext.SaveChangesAsync();
        }
    }

    public async Task HandleEventAsync(PersonReactivatedEvent @event, ProcessContext processContext)
    {
        if (processContext.ProcessType is ProcessType.PersonReactivating)
        {
            var legacyEvent = new LegacyEvents.PersonStatusUpdatedEvent
            {
                EventId = @event.EventId,
                CreatedUtc = processContext.Now,
                RaisedBy = processContext.Process.UserId!,
                PersonId = @event.PersonId,
                OldStatus = PersonStatus.Deactivated,
                Status = PersonStatus.Active,
                DateOfDeath = null,
                Reason = @event.Reason,
                ReasonDetail = @event.ReasonDetail,
                EvidenceFile = @event.EvidenceFile,
            };

            dbContext.AddEventWithoutBroadcast(legacyEvent);

            await dbContext.SaveChangesAsync();
        }
    }

    public async Task HandleEventAsync(PersonsMergedEvent @event, ProcessContext processContext)
    {
        if (processContext.ProcessType is ProcessType.PersonMerging)
        {
            var legacyEvent = new LegacyEvents.PersonsMergedEvent()
            {
                EventId = @event.EventId,
                CreatedUtc = processContext.Now,
                RaisedBy = processContext.Process.UserId!,
                PersonId = @event.RetainedPersonId,
                PersonTrn = @event.RetainedPersonTrn,
                SecondaryPersonId = @event.DeactivatedPersonId,
                SecondaryPersonTrn = @event.SecondaryPersonTrn,
                SecondaryPersonStatus = @event.DeactivatedPersonStatus,
                PersonAttributes = @event.RetainedPersonDetails,
                Changes = @event.Changes.ToLegacyPersonsMergedEventChanges(),
                OldPersonAttributes = @event.OldRetainedPersonDetails,
                EvidenceFile = @event.EvidenceFile,
                Comments = @event.Comments,
            };

            dbContext.AddEventWithoutBroadcast(legacyEvent);

            await dbContext.SaveChangesAsync();
        }
    }
}
