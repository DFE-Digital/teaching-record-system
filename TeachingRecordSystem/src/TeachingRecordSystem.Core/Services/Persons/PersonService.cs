using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Services.TrnGeneration;

namespace TeachingRecordSystem.Core.Services.Persons;

public class PersonService(
    TrsDbContext dbContext,
    IClock clock,
    ITrnGenerator trnGenerator,
    IEventPublisher eventPublisher)
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

    public async Task<Person> CreatePersonAsync(CreatePersonOptions options, ProcessContext processContext)
    {
        var now = clock.UtcNow;

        var trn = options.Trn ?? await trnGenerator.GenerateTrnAsync();

        TrnRequestMetadata? sourceRequest = null;
        if (options.SourceTrnRequest is (var applicationUserId, var requestId))
        {
            sourceRequest = await dbContext.TrnRequestMetadata.SingleOrDefaultAsync(r => r.ApplicationUserId == applicationUserId && r.RequestId == requestId);
            if (sourceRequest is null)
            {
                throw new InvalidOperationException($"TRN request metadata {options.SourceTrnRequest} does not exist.");
            }
        }

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
            UpdatedOn = now,
            CreatedByTps = processContext.ProcessType == ProcessType.TeacherPensionsRecordImporting,
            SourceApplicationUserId = options.SourceTrnRequest?.ApplicationUserId,
            SourceTrnRequestId = options.SourceTrnRequest?.RequestId
        };

        dbContext.Add(person);
        await dbContext.SaveChangesAsync();

        await eventPublisher.PublishEventAsync(
            new PersonCreatedEvent
            {
                EventId = Guid.NewGuid(),
                PersonId = person.PersonId,
                Details = options.PersonDetails.ToEventModel(),
                CreateReason = options.Justification?.Reason.GetDisplayName(),
                CreateReasonDetail = options.Justification?.ReasonDetail,
                EvidenceFile = options.Justification?.Evidence?.ToEventModel(),
                TrnRequestMetadata = sourceRequest is TrnRequestMetadata metadata
                    ? EventModels.TrnRequestMetadata.FromModel(metadata)
                    : null
            },
            processContext);

        return person;
    }

    public async Task UpdatePersonDetailsAsync(UpdatePersonDetailsOptions options, ProcessContext processContext)
    {
        var now = clock.UtcNow;

        var person = await dbContext.Persons.SingleOrDefaultAsync(p => p.PersonId == options.PersonId);

        if (person is null)
        {
            throw new TrsPersonNotFoundException($"Person with ID {options.PersonId} not found.");
        }

        var oldDetails = EventModels.PersonDetails.FromModel(person);

        person.FirstName = options.PersonDetails.FirstName;
        person.MiddleName = options.PersonDetails.MiddleName;
        person.LastName = options.PersonDetails.LastName;
        person.DateOfBirth = options.PersonDetails.DateOfBirth;
        person.EmailAddress = (string?)options.PersonDetails.EmailAddress;
        person.NationalInsuranceNumber = (string?)options.PersonDetails.NationalInsuranceNumber;
        person.Gender = options.PersonDetails.Gender;

        var changes = 0 |
            (person.FirstName != oldDetails.FirstName ? PersonDetailsUpdatedEventChanges.FirstName : 0) |
            (person.MiddleName != oldDetails.MiddleName ? PersonDetailsUpdatedEventChanges.MiddleName : 0) |
            (person.LastName != oldDetails.LastName ? PersonDetailsUpdatedEventChanges.LastName : 0) |
            (person.DateOfBirth != oldDetails.DateOfBirth ? PersonDetailsUpdatedEventChanges.DateOfBirth : 0) |
            (person.EmailAddress != oldDetails.EmailAddress ? PersonDetailsUpdatedEventChanges.EmailAddress : 0) |
            (person.NationalInsuranceNumber != oldDetails.NationalInsuranceNumber ? PersonDetailsUpdatedEventChanges.NationalInsuranceNumber : 0) |
            (person.Gender != oldDetails.Gender ? PersonDetailsUpdatedEventChanges.Gender : 0);

        if (changes == 0)
        {
            return;
        }

        person.UpdatedOn = now;

        if (changes.HasAnyFlag(PersonDetailsUpdatedEventChanges.NameChange) &&
            options.NameChangeJustification?.Reason is PersonNameChangeReason.MarriageOrCivilPartnership or PersonNameChangeReason.DeedPollOrOtherLegalProcess)
        {
            dbContext.PreviousNames.Add(new PreviousName
            {
                PreviousNameId = Guid.NewGuid(),
                PersonId = options.PersonId,
                FirstName = oldDetails.FirstName,
                MiddleName = oldDetails.MiddleName,
                LastName = oldDetails.LastName,
                CreatedOn = now,
                UpdatedOn = now
            });
        }

        await dbContext.SaveChangesAsync();

        await eventPublisher.PublishEventAsync(
            new PersonDetailsUpdatedEvent
            {
                EventId = Guid.NewGuid(),
                PersonId = person.PersonId,
                Changes = changes,
                OldPersonDetails = oldDetails,
                PersonDetails = options.PersonDetails.ToEventModel(),
                NameChangeReason = options.NameChangeJustification?.Reason.GetDisplayName(),
                NameChangeEvidenceFile = options.NameChangeJustification?.Evidence?.ToEventModel(),
                DetailsChangeReason = options.DetailsChangeJustification?.Reason.GetDisplayName(),
                DetailsChangeReasonDetail = options.DetailsChangeJustification?.ReasonDetail,
                DetailsChangeEvidenceFile = options.DetailsChangeJustification?.Evidence?.ToEventModel(),
            },
            processContext);
    }

    public async Task DeactivatePersonAsync(DeactivatePersonOptions options, ProcessContext processContext)
    {
        var now = clock.UtcNow;

        var deactivatingPerson = await dbContext.Persons
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(p => p.PersonId == options.PersonId);

        if (deactivatingPerson is null)
        {
            throw new TrsPersonNotFoundException($"Person with ID {options.PersonId} not found.");
        }

        if (deactivatingPerson.Status is PersonStatus.Deactivated)
        {
            throw new InvalidOperationException("Cannot deactivate a person that is already deactivated.");
        }

        deactivatingPerson.Status = PersonStatus.Deactivated;
        deactivatingPerson.UpdatedOn = now;

        await dbContext.SaveChangesAsync();

        await eventPublisher.PublishEventAsync(new PersonDeactivatedEvent
        {
            EventId = Guid.NewGuid(),
            PersonId = deactivatingPerson.PersonId,
            MergedWithPersonId = null,
            Reason = options.Justification.Reason.GetDisplayName(),
            ReasonDetail = options.Justification.ReasonDetail,
            EvidenceFile = options.Justification.Evidence?.ToEventModel()
        }, processContext);
    }

    public async Task ReactivatePersonAsync(ReactivatePersonOptions options, ProcessContext processContext)
    {
        var now = clock.UtcNow;

        var reactivatingPerson = await dbContext.Persons
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(p => p.PersonId == options.PersonId);

        if (reactivatingPerson is null)
        {
            throw new TrsPersonNotFoundException($"Person with ID {options.PersonId} not found.");
        }

        if (reactivatingPerson.Status is PersonStatus.Active)
        {
            throw new InvalidOperationException("Cannot reactivate a person that is already active.");
        }

        reactivatingPerson.Status = PersonStatus.Active;
        reactivatingPerson.UpdatedOn = now;

        await dbContext.SaveChangesAsync();

        await eventPublisher.PublishEventAsync(new PersonReactivatedEvent
        {
            EventId = Guid.NewGuid(),
            PersonId = reactivatingPerson.PersonId,
            Reason = options.Justification.Reason.GetDisplayName(),
            ReasonDetail = options.Justification.ReasonDetail,
            EvidenceFile = options.Justification.Evidence?.ToEventModel()
        }, processContext);
    }

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
                MergedWithPersonId = options.RetainedPersonId,
                Reason = null,
                ReasonDetail = null,
                EvidenceFile = null
            },
            processContext);
    }
}
