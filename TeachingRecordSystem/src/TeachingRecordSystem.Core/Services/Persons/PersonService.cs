using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Events.Legacy;
using TeachingRecordSystem.Core.Services.TrnGeneration;

namespace TeachingRecordSystem.Core.Services.Persons;

public class PersonService(
    TrsDbContext dbContext,
    IClock clock,
    ITrnGenerator trnGenerator,
    IEventPublisher eventPublisher)
{
    public async Task MergePersonsAsync(MergePersonsOptions options, ProcessContext processContext)
    {
        var deactivatingPerson = await dbContext.Persons.IgnoreQueryFilters().SingleAsync(p => p.PersonId == options.DeactivatingPersonId);

        if (deactivatingPerson.Status is PersonStatus.Deactivated)
        {
            throw new InvalidOperationException("Cannot merge a person that is already deactivated.");
        }

        deactivatingPerson.Status = PersonStatus.Deactivated;
        deactivatingPerson.MergedWithPersonId = options.RetainedPersonId;
        await dbContext.SaveChangesAsync();

        await eventPublisher.PublishEventAsync(
            new PersonDeactivatedEvent
            {
                EventId = Guid.NewGuid(),
                PersonId = options.DeactivatingPersonId,
                MergedWithPersonId = options.RetainedPersonId
            },
            processContext);
    }

    public async Task<string?> GetTrnFromPersonIdAsync(Guid personId)
    {
        return (await dbContext.Persons.SingleAsync(q => q.PersonId == personId)).Trn;
    }

    public async Task<Person?> GetPersonAsync(Guid personId, bool includeDeactivatedPersons = false)
    {
        var persons = dbContext.Persons.AsQueryable();

        if (includeDeactivatedPersons)
        {
            persons = persons.IgnoreQueryFilters();
        }

        return await persons.SingleOrDefaultAsync(p => p.PersonId == personId);
    }

    public async Task<Guid> CreatePersonAsync(CreatePersonOptions options)
    {
        var now = clock.UtcNow;

        var trn = await trnGenerator.GenerateTrnAsync();

        var person = new Person
        {
            PersonId = Guid.NewGuid(),
            Trn = trn,
            FirstName = options.PersonDetails.FirstName,
            MiddleName = options.PersonDetails.MiddleName,
            LastName = options.PersonDetails.LastName,
            DateOfBirth = options.PersonDetails.DateOfBirth,
            EmailAddress = (string?)options.PersonDetails.EmailAddress,
            NationalInsuranceNumber = (string?)options.PersonDetails.NationalInsuranceNumber,
            Gender = options.PersonDetails.Gender,
            CreatedOn = now,
            UpdatedOn = now
        };

        var createdEvent = new LegacyEvents.PersonCreatedEvent
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = now,
            RaisedBy = options.UserId,
            PersonId = person.PersonId,
            PersonAttributes = options.PersonDetails.ToEventModel(),
            CreateReason = options.Justification.Reason.GetDisplayName(),
            CreateReasonDetail = options.Justification.ReasonDetail,
            EvidenceFile = options.Justification.Evidence?.ToEventModel(),
            TrnRequestMetadata = null
        };

        dbContext.Add(person);
        await dbContext.AddEventAndBroadcastAsync(createdEvent);
        await dbContext.SaveChangesAsync();

        return person.PersonId;
    }

    public async Task UpdatePersonAsync(UpdatePersonOptions options)
    {
        var now = clock.UtcNow;

        var person = await dbContext.Persons.SingleOrDefaultAsync(p => p.PersonId == options.PersonId);

        if (person is null)
        {
            throw new TrsPersonNotFoundException($"Person with ID {options.PersonId} not found.");
        }

        var oldAttributes = EventModels.PersonDetails.FromModel(person);

        person.FirstName = options.PersonDetails.FirstName;
        person.MiddleName = options.PersonDetails.MiddleName;
        person.LastName = options.PersonDetails.LastName;
        person.DateOfBirth = options.PersonDetails.DateOfBirth;
        person.EmailAddress = (string?)options.PersonDetails.EmailAddress;
        person.NationalInsuranceNumber = (string?)options.PersonDetails.NationalInsuranceNumber;
        person.Gender = options.PersonDetails.Gender;

        var changes = 0 |
            (person.FirstName != oldAttributes.FirstName ? LegacyEvents.PersonDetailsUpdatedEventChanges.FirstName : 0) |
            (person.MiddleName != oldAttributes.MiddleName ? LegacyEvents.PersonDetailsUpdatedEventChanges.MiddleName : 0) |
            (person.LastName != oldAttributes.LastName ? LegacyEvents.PersonDetailsUpdatedEventChanges.LastName : 0) |
            (person.DateOfBirth != oldAttributes.DateOfBirth ? LegacyEvents.PersonDetailsUpdatedEventChanges.DateOfBirth : 0) |
            (person.EmailAddress != oldAttributes.EmailAddress ? LegacyEvents.PersonDetailsUpdatedEventChanges.EmailAddress : 0) |
            (person.NationalInsuranceNumber != oldAttributes.NationalInsuranceNumber ? LegacyEvents.PersonDetailsUpdatedEventChanges.NationalInsuranceNumber : 0) |
            (person.Gender != oldAttributes.Gender ? LegacyEvents.PersonDetailsUpdatedEventChanges.Gender : 0);

        if (changes != 0)
        {
            person.UpdatedOn = now;
        }

        var updatedEvent = changes != 0 ?
            new LegacyEvents.PersonDetailsUpdatedEvent
            {
                EventId = Guid.NewGuid(),
                CreatedUtc = now,
                RaisedBy = options.UserId,
                PersonId = options.PersonId,
                PersonAttributes = options.PersonDetails.ToEventModel(),
                OldPersonAttributes = oldAttributes,
                NameChangeReason = options.NameChangeJustification?.Reason.GetDisplayName(),
                NameChangeEvidenceFile = options.NameChangeJustification?.Evidence?.ToEventModel(),
                DetailsChangeReason = options.DetailsChangeJustification?.Reason.GetDisplayName(),
                DetailsChangeReasonDetail = options.DetailsChangeJustification?.ReasonDetail,
                DetailsChangeEvidenceFile = options.DetailsChangeJustification?.Evidence?.ToEventModel(),
                Changes = changes
            } :
            null;

        if (updatedEvent is not null &&
            updatedEvent.Changes.HasAnyFlag(LegacyEvents.PersonDetailsUpdatedEventChanges.NameChange) &&
            options.NameChangeJustification?.Reason is PersonNameChangeReason.MarriageOrCivilPartnership or PersonNameChangeReason.DeedPollOrOtherLegalProcess)
        {
            dbContext.PreviousNames.Add(new PreviousName
            {
                PreviousNameId = Guid.NewGuid(),
                PersonId = options.PersonId,
                FirstName = updatedEvent.OldPersonAttributes.FirstName,
                MiddleName = updatedEvent.OldPersonAttributes.MiddleName,
                LastName = updatedEvent.OldPersonAttributes.LastName,
                CreatedOn = now,
                UpdatedOn = now
            });
        }

        if (updatedEvent is not null)
        {
            await dbContext.AddEventAndBroadcastAsync(updatedEvent);
            await dbContext.SaveChangesAsync();
        }
    }

    public async Task SetPersonStatusAsync(SetPersonStatusOptions options)
    {
        var now = clock.UtcNow;

        var person = await dbContext.Persons
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(p => p.PersonId == options.PersonId);

        if (person is null)
        {
            throw new TrsPersonNotFoundException($"Person with ID {options.PersonId} not found.");
        }

        var oldStatus = person.Status;
        person.Status = options.TargetStatus;
        person.UpdatedOn = now;

        var @event = new PersonStatusUpdatedEvent()
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = now,
            RaisedBy = options.UserId,
            PersonId = options.PersonId,
            Status = person.Status,
            OldStatus = oldStatus,
            Reason = options.TargetStatus == PersonStatus.Deactivated
                ? options.DeactivateJustification!.Reason.GetDisplayName()
                : options.ReactivateJustification!.Reason.GetDisplayName(),
            ReasonDetail = options.TargetStatus == PersonStatus.Deactivated
                ? options.DeactivateJustification!.ReasonDetail
                : options.ReactivateJustification!.ReasonDetail,
            EvidenceFile = options.TargetStatus == PersonStatus.Deactivated
                ? options.DeactivateJustification!.Evidence?.ToEventModel()
                : options.ReactivateJustification!.Evidence?.ToEventModel(),
            DateOfDeath = null
        };

        if (@event is not null)
        {
            await dbContext.AddEventAndBroadcastAsync(@event);
            await dbContext.SaveChangesAsync();
        }
    }

    public async Task SetPersonInductionStatusAsync(SetPersonInductionStatusOptions options)
    {
        var person = await dbContext.Persons.SingleAsync(q => q.PersonId == options.PersonId);

        person.SetInductionStatus(
            options.InductionStatus,
            options.StartDate,
            options.CompletedDate,
            options.ExemptionReasonIds ?? [],
            options.Justification.Reason.GetDisplayName(),
            options.Justification.ReasonDetail,
            options.Justification.Evidence?.ToEventModel(),
            options.UserId,
            clock.UtcNow,
            out var updatedEvent);

        if (updatedEvent is not null)
        {
            await dbContext.AddEventAndBroadcastAsync(updatedEvent);
            await dbContext.SaveChangesAsync();
        }
    }
}
