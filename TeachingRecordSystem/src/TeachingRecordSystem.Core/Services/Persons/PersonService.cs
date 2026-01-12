using Optional;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.Services.Persons;

public class PersonService(
    TrsDbContext dbContext,
    IClock clock,
    IEventPublisher eventPublisher)
{
    public async Task<string?> GetTrnFromPersonIdAsync(Guid personId)
    {
        return (await dbContext.Persons.SingleAsync(q => q.PersonId == personId)).Trn;
    }

    public Task<Person?> GetPersonAsync(Guid personId, bool includeDeactivatedPersons = false)
    {
        var persons = dbContext.Persons.AsQueryable();

        if (includeDeactivatedPersons)
        {
            persons = persons.IgnoreQueryFilters();
        }

        return persons.SingleOrDefaultAsync(p => p.PersonId == personId);
    }

    public async Task<Person> CreatePersonAsync(CreatePersonOptions options, ProcessContext processContext)
    {
        TrnRequestMetadata? sourceRequest = null;
        if (options.SourceTrnRequest is (var applicationUserId, var requestId))
        {
            sourceRequest = await dbContext.TrnRequestMetadata.SingleOrDefaultAsync(r => r.ApplicationUserId == applicationUserId && r.RequestId == requestId);
            if (sourceRequest is null)
            {
                throw new InvalidOperationException($"TRN request metadata {options.SourceTrnRequest} does not exist.");
            }
        }

        var now = clock.UtcNow;
        var person = new Person
        {
            PersonId = Guid.NewGuid(),
            Trn = options.Trn!,
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

    public async Task<UpdatePersonDetailsResult> UpdatePersonDetailsAsync(UpdatePersonDetailsOptions options, ProcessContext processContext)
    {
        var person = await dbContext.Persons
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(p => p.PersonId == options.PersonId);

        if (person is null)
        {
            throw new TrsPersonNotFoundException($"Person with ID {options.PersonId} not found.");
        }

        if (person.Status is PersonStatus.Deactivated)
        {
            throw new InvalidOperationException("Cannot update a person that is deactivated.");
        }

        var oldDetails = person.Details;

        (options.PersonDetails.FirstName ?? Option.None<string>()).MatchSome(firstName => person.FirstName = firstName);
        (options.PersonDetails.MiddleName ?? Option.None<string>()).MatchSome(middleName => person.MiddleName = middleName);
        (options.PersonDetails.LastName ?? Option.None<string>()).MatchSome(lastName => person.LastName = lastName);
        (options.PersonDetails.DateOfBirth ?? Option.None<DateOnly?>()).MatchSome(dateOfBirth => person.DateOfBirth = dateOfBirth);
        (options.PersonDetails.EmailAddress ?? Option.None<EmailAddress?>()).MatchSome(emailAddress => person.EmailAddress = (string?)emailAddress);
        (options.PersonDetails.NationalInsuranceNumber ?? Option.None<NationalInsuranceNumber?>()).MatchSome(nationalInsuranceNumber => person.NationalInsuranceNumber = (string?)nationalInsuranceNumber);
        (options.PersonDetails.Gender ?? Option.None<Gender?>()).MatchSome(gender => person.Gender = gender);

        var changes = 0 |
            (person.FirstName != oldDetails.FirstName ? PersonDetailsUpdatedEventChanges.FirstName : 0) |
            (person.MiddleName != oldDetails.MiddleName ? PersonDetailsUpdatedEventChanges.MiddleName : 0) |
            (person.LastName != oldDetails.LastName ? PersonDetailsUpdatedEventChanges.LastName : 0) |
            (person.DateOfBirth != oldDetails.DateOfBirth ? PersonDetailsUpdatedEventChanges.DateOfBirth : 0) |
            (person.EmailAddress != oldDetails.EmailAddress?.ToString() ? PersonDetailsUpdatedEventChanges.EmailAddress : 0) |
            (person.NationalInsuranceNumber != oldDetails.NationalInsuranceNumber?.ToString() ? PersonDetailsUpdatedEventChanges.NationalInsuranceNumber : 0) |
            (person.Gender != oldDetails.Gender ? PersonDetailsUpdatedEventChanges.Gender : 0);

        if (changes == 0)
        {
            return new(changes, oldDetails, oldDetails);
        }

        var now = clock.UtcNow;
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
                OldPersonDetails = oldDetails.ToEventModel(),
                PersonDetails = person.Details.ToEventModel(),
                NameChangeReason = options.NameChangeJustification?.Reason.GetDisplayName(),
                NameChangeEvidenceFile = options.NameChangeJustification?.Evidence?.ToEventModel(),
                DetailsChangeReason = options.DetailsChangeJustification?.Reason.GetDisplayName(),
                DetailsChangeReasonDetail = options.DetailsChangeJustification?.ReasonDetail,
                DetailsChangeEvidenceFile = options.DetailsChangeJustification?.Evidence?.ToEventModel(),
            },
            processContext);

        return new(changes, person.Details, oldDetails);
    }

    public async Task DeactivatePersonAsync(DeactivatePersonOptions options, ProcessContext processContext)
    {
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
        deactivatingPerson.UpdatedOn = clock.UtcNow;

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
        reactivatingPerson.UpdatedOn = clock.UtcNow;

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

    public async Task DeactivatePersonViaMergeAsync(DeactivatePersonViaMergeOptions options, ProcessContext processContext)
    {
        var deactivatingPerson = await dbContext.Persons
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(p => p.PersonId == options.DeactivatingPersonId);

        if (deactivatingPerson is null)
        {
            throw new TrsPersonNotFoundException($"Person with ID {options.DeactivatingPersonId} not found.");
        }

        if (deactivatingPerson.Status is PersonStatus.Deactivated)
        {
            throw new InvalidOperationException("Cannot deactivate a person that is already deactivated.");
        }

        var retainedPerson = await dbContext.Persons
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(p => p.PersonId == options.RetainedPersonId);

        if (retainedPerson is null)
        {
            throw new TrsPersonNotFoundException($"Person with ID {options.RetainedPersonId} not found.");
        }

        if (retainedPerson.Status is PersonStatus.Deactivated)
        {
            throw new InvalidOperationException("Cannot merge a person that is deactivated.");
        }

        deactivatingPerson.Status = PersonStatus.Deactivated;
        deactivatingPerson.MergedWithPersonId = options.RetainedPersonId;
        deactivatingPerson.UpdatedOn = clock.UtcNow;

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

    public async Task MergePersonsAsync(MergePersonsOptions options, ProcessContext processContext)
    {
        var deactivatingPerson = await dbContext.Persons
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(p => p.PersonId == options.DeactivatingPersonId);

        if (deactivatingPerson is null)
        {
            throw new TrsPersonNotFoundException($"Person with ID {options.DeactivatingPersonId} not found.");
        }

        if (deactivatingPerson.Status is PersonStatus.Deactivated)
        {
            throw new InvalidOperationException("Cannot deactivate a person that is already deactivated.");
        }

        var retainedPerson = await dbContext.Persons
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(p => p.PersonId == options.RetainedPersonId);

        if (retainedPerson is null)
        {
            throw new TrsPersonNotFoundException($"Person with ID {options.RetainedPersonId} not found.");
        }

        if (retainedPerson.Status is PersonStatus.Deactivated)
        {
            throw new InvalidOperationException("Cannot merge a person that is deactivated.");
        }

        var now = clock.UtcNow;
        var oldRetainedPersonDetails = retainedPerson.Details.ToEventModel();

        retainedPerson.FirstName = options.PersonDetails.FirstName;
        retainedPerson.MiddleName = options.PersonDetails.MiddleName;
        retainedPerson.LastName = options.PersonDetails.LastName;
        retainedPerson.DateOfBirth = options.PersonDetails.DateOfBirth;
        retainedPerson.EmailAddress = (string?)options.PersonDetails.EmailAddress;
        retainedPerson.NationalInsuranceNumber = (string?)options.PersonDetails.NationalInsuranceNumber;
        retainedPerson.Gender = options.PersonDetails.Gender;

        var changes = 0 |
            (retainedPerson.FirstName != oldRetainedPersonDetails.FirstName ? PersonsMergedEventChanges.FirstName : 0) |
            (retainedPerson.MiddleName != oldRetainedPersonDetails.MiddleName ? PersonsMergedEventChanges.MiddleName : 0) |
            (retainedPerson.LastName != oldRetainedPersonDetails.LastName ? PersonsMergedEventChanges.LastName : 0) |
            (retainedPerson.DateOfBirth != oldRetainedPersonDetails.DateOfBirth ? PersonsMergedEventChanges.DateOfBirth : 0) |
            (retainedPerson.EmailAddress != oldRetainedPersonDetails.EmailAddress ? PersonsMergedEventChanges.EmailAddress : 0) |
            (retainedPerson.NationalInsuranceNumber != oldRetainedPersonDetails.NationalInsuranceNumber ? PersonsMergedEventChanges.NationalInsuranceNumber : 0) |
            (retainedPerson.Gender != oldRetainedPersonDetails.Gender ? PersonsMergedEventChanges.Gender : 0);

        if (changes != 0)
        {
            retainedPerson.UpdatedOn = now;
        }

        deactivatingPerson.Status = PersonStatus.Deactivated;
        deactivatingPerson.MergedWithPersonId = options.RetainedPersonId;
        deactivatingPerson.UpdatedOn = now;

        // If the deactivated person was associated with a One Login User, transfer that association to the retained person
        var oneLoginUser = await dbContext.OneLoginUsers
            .SingleOrDefaultAsync(u => u.PersonId == deactivatingPerson.PersonId);

        if (oneLoginUser is not null)
        {
            oneLoginUser.SetMatched(
                now,
                retainedPerson.PersonId,
                OneLoginUserMatchRoute.SupportUi,
                matchedAttributes: null);

            // Events to be added in follow up work
        }

        await dbContext.SaveChangesAsync();

        await eventPublisher.PublishEventAsync(
            new PersonsMergedEvent()
            {
                EventId = Guid.NewGuid(),
                RetainedPersonId = options.RetainedPersonId,
                RetainedPersonTrn = retainedPerson.Trn,
                DeactivatedPersonId = options.DeactivatingPersonId,
                DeactivatedPersonTrn = deactivatingPerson.Trn,
                DeactivatedPersonStatus = deactivatingPerson.Status,
                RetainedPersonDetails = options.PersonDetails.ToEventModel(),
                OldRetainedPersonDetails = oldRetainedPersonDetails,
                EvidenceFile = options.Evidence?.ToEventModel(),
                Comments = options.Comments,
                Changes = changes
            },
            processContext);
    }
}
