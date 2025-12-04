using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Events.Legacy;
using TeachingRecordSystem.Core.Services.TrnGeneration;

namespace TeachingRecordSystem.Core.Services.Persons;

public class PersonService(
    TrsDbContext dbContext,
    IClock clock,
    ITrnGenerator trnGenerator)
{
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

    public async Task<Guid> CreatePersonAsync(CreatePersonRequest request)
    {
        var now = clock.UtcNow;

        var trn = await trnGenerator.GenerateTrnAsync();

        var person = new Person
        {
            PersonId = Guid.NewGuid(),
            Trn = trn,
            FirstName = request.PersonDetails.FirstName,
            MiddleName = request.PersonDetails.MiddleName,
            LastName = request.PersonDetails.LastName,
            DateOfBirth = request.PersonDetails.DateOfBirth,
            EmailAddress = (string?)request.PersonDetails.EmailAddress,
            NationalInsuranceNumber = (string?)request.PersonDetails.NationalInsuranceNumber,
            Gender = request.PersonDetails.Gender,
            CreatedOn = now,
            UpdatedOn = now
        };

        var createdEvent = new LegacyEvents.PersonCreatedEvent
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = now,
            RaisedBy = request.UserId,
            PersonId = person.PersonId,
            PersonAttributes = request.PersonDetails.ToEventModel(),
            CreateReason = request.Justification.Reason.GetDisplayName(),
            CreateReasonDetail = request.Justification.ReasonDetail,
            EvidenceFile = request.Justification.Evidence?.ToEventModel(),
            TrnRequestMetadata = null
        };

        dbContext.Add(person);
        await dbContext.AddEventAndBroadcastAsync(createdEvent);
        await dbContext.SaveChangesAsync();

        return person.PersonId;
    }

    public async Task UpdatePersonAsync(UpdatePersonRequest request)
    {
        var now = clock.UtcNow;

        var person = await dbContext.Persons.SingleOrDefaultAsync(p => p.PersonId == request.PersonId);

        if (person is null)
        {
            throw new TrsPersonNotFoundException($"Person with ID {request.PersonId} not found.");
        }

        var oldAttributes = EventModels.PersonDetails.FromModel(person);

        person.FirstName = request.PersonDetails.FirstName;
        person.MiddleName = request.PersonDetails.MiddleName;
        person.LastName = request.PersonDetails.LastName;
        person.DateOfBirth = request.PersonDetails.DateOfBirth;
        person.EmailAddress = (string?)request.PersonDetails.EmailAddress;
        person.NationalInsuranceNumber = (string?)request.PersonDetails.NationalInsuranceNumber;
        person.Gender = request.PersonDetails.Gender;

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
                RaisedBy = request.UserId,
                PersonId = request.PersonId,
                PersonAttributes = request.PersonDetails.ToEventModel(),
                OldPersonAttributes = oldAttributes,
                NameChangeReason = request.NameChangeJustification?.Reason.GetDisplayName(),
                NameChangeEvidenceFile = request.NameChangeJustification?.Evidence?.ToEventModel(),
                DetailsChangeReason = request.DetailsChangeJustification?.Reason.GetDisplayName(),
                DetailsChangeReasonDetail = request.DetailsChangeJustification?.ReasonDetail,
                DetailsChangeEvidenceFile = request.DetailsChangeJustification?.Evidence?.ToEventModel(),
                Changes = changes
            } :
            null;

        if (updatedEvent is not null &&
            updatedEvent.Changes.HasAnyFlag(LegacyEvents.PersonDetailsUpdatedEventChanges.NameChange) &&
            request.NameChangeJustification?.Reason is PersonNameChangeReason.MarriageOrCivilPartnership or PersonNameChangeReason.DeedPollOrOtherLegalProcess)
        {
            dbContext.PreviousNames.Add(new PreviousName
            {
                PreviousNameId = Guid.NewGuid(),
                PersonId = request.PersonId,
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

    public async Task SetPersonStatusAsync(SetPersonStatusRequest request)
    {
        var now = clock.UtcNow;

        var person = await dbContext.Persons
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(p => p.PersonId == request.PersonId);

        if (person is null)
        {
            throw new TrsPersonNotFoundException($"Person with ID {request.PersonId} not found.");
        }

        var oldStatus = person.Status;
        person.Status = request.TargetStatus;
        person.UpdatedOn = now;

        var @event = new PersonStatusUpdatedEvent()
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = now,
            RaisedBy = request.UserId,
            PersonId = request.PersonId,
            Status = person.Status,
            OldStatus = oldStatus,
            Reason = request.TargetStatus == PersonStatus.Deactivated
                ? request.DeactivateJustification!.Reason.GetDisplayName()
                : request.ReactivateJustification!.Reason.GetDisplayName(),
            ReasonDetail = request.TargetStatus == PersonStatus.Deactivated
                ? request.DeactivateJustification!.ReasonDetail
                : request.ReactivateJustification!.ReasonDetail,
            EvidenceFile = request.TargetStatus == PersonStatus.Deactivated
                ? request.DeactivateJustification!.Evidence?.ToEventModel()
                : request.ReactivateJustification!.Evidence?.ToEventModel(),
            DateOfDeath = null
        };

        if (@event is not null)
        {
            await dbContext.AddEventAndBroadcastAsync(@event);
            await dbContext.SaveChangesAsync();
        }
    }

    public async Task SetPersonInductionStatusAsync(SetPersonInductionStatusRequest request)
    {
        var person = await dbContext.Persons.SingleAsync(q => q.PersonId == request.PersonId);

        person.SetInductionStatus(
            request.InductionStatus,
            request.StartDate,
            request.CompletedDate,
            request.ExemptionReasonIds ?? [],
            request.Justification.Reason.GetDisplayName(),
            request.Justification.ReasonDetail,
            request.Justification.Evidence?.ToEventModel(),
            request.UserId,
            clock.UtcNow,
            out var updatedEvent);

        if (updatedEvent is not null)
        {
            await dbContext.AddEventAndBroadcastAsync(updatedEvent);
            await dbContext.SaveChangesAsync();
        }
    }
}
