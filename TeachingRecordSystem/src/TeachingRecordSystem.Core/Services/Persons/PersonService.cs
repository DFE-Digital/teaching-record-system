using System.ComponentModel.DataAnnotations;
using Optional;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Services.TrnGeneration;

namespace TeachingRecordSystem.Core.Services.Persons;

public class PersonService(
    TrsDbContext dbContext,
    IClock clock,
    ITrnGenerator trnGenerator,
    IEventPublisher eventPublisher,
    ReferenceDataCache referenceDataCache)
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

    public async Task<Guid> CreatePersonAsync(CreatePersonRequest request)
    {
        var now = clock.UtcNow;

        var trn = await trnGenerator.GenerateTrnAsync();

        var (person, personAttributes) = Person.Create(
            trn,
            request.FirstName,
            request.MiddleName,
            request.LastName,
            request.DateOfBirth,
            request.EmailAddress,
            request.NationalInsuranceNumber,
            request.Gender,
            now);

        var createdEvent = new LegacyEvents.PersonCreatedEvent
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = now,
            RaisedBy = request.UserId,
            PersonId = person.PersonId,
            PersonAttributes = personAttributes,
            CreateReason = request.CreateReason,
            CreateReasonDetail = request.CreateReasonDetail,
            EvidenceFile = request.EvidenceFile?.ToEventModel(),
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

        var updateResult = person!.UpdateDetails(
            Option.Some(request.FirstName),
            Option.Some(request.MiddleName),
            Option.Some(request.LastName),
            Option.Some(request.DateOfBirth),
            Option.Some(request.EmailAddress),
            Option.Some(request.NationalInsuranceNumber),
            Option.Some(request.Gender),
            now);

        var updatedEvent = updateResult.Changes != 0 ?
            new LegacyEvents.PersonDetailsUpdatedEvent
            {
                EventId = Guid.NewGuid(),
                CreatedUtc = now,
                RaisedBy = request.UserId,
                PersonId = request.PersonId,
                PersonAttributes = updateResult.PersonAttributes,
                OldPersonAttributes = updateResult.OldPersonAttributes,
                NameChangeReason = request.NameChangeReason?.GetDisplayName(),
                NameChangeEvidenceFile = request.NameChangeEvidenceFile?.ToEventModel(),
                DetailsChangeReason = request.DetailsChangeReason?.GetDisplayName(),
                DetailsChangeReasonDetail = request.DetailsChangeReasonDetail,
                DetailsChangeEvidenceFile = request.DetailsChangeEvidenceFile?.ToEventModel(),
                Changes = (LegacyEvents.PersonDetailsUpdatedEventChanges)updateResult.Changes
            } :
            null;

        if (updatedEvent is not null &&
            updatedEvent.Changes.HasAnyFlag(LegacyEvents.PersonDetailsUpdatedEventChanges.NameChange) &&
            request.NameChangeReason is EditDetailsNameChangeReasonOption.MarriageOrCivilPartnership or EditDetailsNameChangeReasonOption.DeedPollOrOtherLegalProcess)
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

        person!.SetStatus(
            request.TargetStatus,
            request.TargetStatus == PersonStatus.Deactivated
                ? request.DeactivateReason!.GetDisplayName()
                : request.ReactivateReason!.GetDisplayName(),
            request.TargetStatus == PersonStatus.Deactivated
                ? request.DeactivateReasonDetail
                : request.ReactivateReasonDetail,
            request.EvidenceFile?.ToEventModel(),
            request.UserId,
            now,
            out var @event);

        if (@event is not null)
        {
            await dbContext.AddEventAndBroadcastAsync(@event);
            await dbContext.SaveChangesAsync();
        }
    }


    }
}

public class TrsException(string message, Exception? innerException = null) : Exception(message, innerException);

public class TrsInvalidMergeException(Guid primaryPersonId, Guid secondaryPersonId)
    : TrsException($"Persons {primaryPersonId} and {secondaryPersonId} could not be merged (TODO: break down into invalid cases)")
{
    public Guid PrimaryPersonId { get; } = primaryPersonId;
    public Guid SecondaryPersonId { get; } = secondaryPersonId;
}

public record CreatePersonRequest(
    string FirstName,
    string MiddleName,
    string LastName,
    DateOnly? DateOfBirth,
    EmailAddress? EmailAddress,
    NationalInsuranceNumber? NationalInsuranceNumber,
    Gender? Gender,
    Guid UserId,
    string? CreateReason,
    string? CreateReasonDetail,
    File? EvidenceFile
);

public record UpdatePersonRequest
{
    public required Guid PersonId { get; init; }
    public required string FirstName { get; init; }
    public required string MiddleName { get; init; }
    public required string LastName { get; init; }
    public required DateOnly? DateOfBirth { get; init; }
    public required EmailAddress? EmailAddress { get; init; }
    public required NationalInsuranceNumber? NationalInsuranceNumber { get; init; }
    public required Gender? Gender { get; init; }
    public required Guid UserId { get; init; }
    public required EditDetailsNameChangeReasonOption? NameChangeReason { get; init; }
    public required File? NameChangeEvidenceFile { get; init; }
    public required EditDetailsOtherDetailsChangeReasonOption? DetailsChangeReason { get; init; }
    public required string? DetailsChangeReasonDetail { get; init; }
    public required File? DetailsChangeEvidenceFile { get; init; }
}

public record SetPersonStatusRequest
{
    public required Guid PersonId { get; init; }
    public required PersonStatus TargetStatus { get; init; }
    public required DeactivateReasonOption? DeactivateReason { get; init; }
    public required ReactivateReasonOption? ReactivateReason { get; init; }
    public required string? DeactivateReasonDetail { get; init; }
    public required string? ReactivateReasonDetail { get; init; }
    public required File? EvidenceFile { get; init; }
    public required Guid UserId { get; init; }
}

public enum EditDetailsNameChangeReasonOption
{
    [Display(Name = "Name has changed due to marriage or civil partnership")]
    MarriageOrCivilPartnership,
    [Display(Name = "Name has changed by deed poll or another legal process")]
    DeedPollOrOtherLegalProcess,
    [Display(Name = "Correcting an error")]
    CorrectingAnError
}

public enum EditDetailsOtherDetailsChangeReasonOption
{
    [Display(Name = "Data loss or incomplete information")]
    IncompleteDetails,
    [Display(Name = "New information received")]
    NewInformation,
    [Display(Name = "Another reason")]
    AnotherReason
}

public record File
{
    public required Guid FileId { get; init; }
    public required string Name { get; init; }

    public EventModels.File ToEventModel() => new()
    {
        FileId = FileId,
        Name = Name
    };
}

public record PersonDetails
{
    public required string FirstName { get; set; }
    public required string MiddleName { get; set; }
    public required string LastName { get; set; }
    public required DateOnly? DateOfBirth { get; set; }
    public required string? EmailAddress { get; set; }
    public required string? NationalInsuranceNumber { get; set; }
    public required Gender? Gender { get; set; }

    public EventModels.PersonDetails ToEventModel() => new()
    {
        FirstName = FirstName,
        MiddleName = MiddleName,
        LastName = LastName,
        DateOfBirth = DateOfBirth,
        EmailAddress = EmailAddress,
        NationalInsuranceNumber = NationalInsuranceNumber,
        Gender = Gender,
    };
}

public enum DeactivateReasonOption
{
    [Display(Name = "The record holder died")]
    RecordHolderDied,
    [Display(Name = "There is a problem with the record")]
    ProblemWithTheRecord,
    [Display(Name = "Another reason")]
    AnotherReason
}

public enum ReactivateReasonOption
{
    [Display(Name = "The record was deactivated by mistake")]
    DeactivatedByMistake,
    [Display(Name = "Another reason")]
    AnotherReason
}

