using TeachingRecordSystem.Core.DataStore.Postgres;

namespace TeachingRecordSystem.Core.EventHandlers;

public class CreateLegacyPersonEvents(TrsDbContext dbContext) :
    IEventHandler<PersonCreatedEvent>,
    IEventHandler<PersonDetailsUpdatedEvent>,
    IEventHandler<PersonDeactivatedEvent>,
    IEventHandler<PersonReactivatedEvent>
{
    public async Task HandleEventAsync(PersonCreatedEvent @event, ProcessContext processContext)
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

    public async Task HandleEventAsync(PersonDetailsUpdatedEvent @event, ProcessContext processContext)
    {
        var legacyEvent = new LegacyEvents.PersonDetailsUpdatedEvent
        {
            EventId = @event.EventId,
            CreatedUtc = processContext.Now,
            RaisedBy = processContext.Process.UserId!,
            PersonId = @event.PersonId,
            Changes = ToLegacyChanges(@event.Changes),
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

        LegacyEvents.PersonDetailsUpdatedEventChanges ToLegacyChanges(PersonDetailsUpdatedEventChanges changes) =>
            LegacyEvents.PersonDetailsUpdatedEventChanges.None
            | (changes.HasFlag(PersonDetailsUpdatedEventChanges.FirstName) ? LegacyEvents.PersonDetailsUpdatedEventChanges.FirstName : 0)
            | (changes.HasFlag(PersonDetailsUpdatedEventChanges.MiddleName) ? LegacyEvents.PersonDetailsUpdatedEventChanges.MiddleName : 0)
            | (changes.HasFlag(PersonDetailsUpdatedEventChanges.LastName) ? LegacyEvents.PersonDetailsUpdatedEventChanges.LastName : 0)
            | (changes.HasFlag(PersonDetailsUpdatedEventChanges.DateOfBirth) ? LegacyEvents.PersonDetailsUpdatedEventChanges.DateOfBirth : 0)
            | (changes.HasFlag(PersonDetailsUpdatedEventChanges.EmailAddress) ? LegacyEvents.PersonDetailsUpdatedEventChanges.EmailAddress : 0)
            | (changes.HasFlag(PersonDetailsUpdatedEventChanges.NationalInsuranceNumber) ? LegacyEvents.PersonDetailsUpdatedEventChanges.NationalInsuranceNumber : 0)
            | (changes.HasFlag(PersonDetailsUpdatedEventChanges.Gender) ? LegacyEvents.PersonDetailsUpdatedEventChanges.Gender : 0);
    }

    public async Task HandleEventAsync(PersonDeactivatedEvent @event, ProcessContext processContext)
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

    public async Task HandleEventAsync(PersonReactivatedEvent @event, ProcessContext processContext)
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
