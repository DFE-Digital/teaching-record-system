using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Services.OneLogin;

namespace TeachingRecordSystem.Core.Services.Persons;

public class PersonService(TrsDbContext dbContext, OneLoginService oneLoginService, IEventPublisher eventPublisher)
{
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
        await using var eventScope = eventPublisher.GetOrCreateEventScope(processContext);

        TrnRequestMetadata? sourceRequest = null;
        if (options.SourceTrnRequest is (var applicationUserId, var requestId))
        {
            sourceRequest = await dbContext.TrnRequestMetadata.FindAsync(applicationUserId, requestId);

            if (sourceRequest is null)
            {
                throw new NotFoundException(options.SourceTrnRequest, nameof(TrnRequestMetadata));
            }
        }

        var person = new Person
        {
            PersonId = Guid.NewGuid(),
            Trn = options.Trn.ValueOr((string?)null!),
            FirstName = options.FirstName,
            MiddleName = options.MiddleName,
            LastName = options.LastName,
            DateOfBirth = options.DateOfBirth,
            EmailAddress = (string?)options.EmailAddress,
            NationalInsuranceNumber = (string?)options.NationalInsuranceNumber,
            Gender = options.Gender,
            CreatedOn = processContext.Now,
            UpdatedOn = processContext.Now,
            CreatedByTps = processContext.ProcessType is ProcessType.TeacherPensionsRecordImporting,
            SourceApplicationUserId = options.SourceTrnRequest?.ApplicationUserId,
            SourceTrnRequestId = options.SourceTrnRequest?.RequestId
        };

        dbContext.Add(person);
        await dbContext.SaveChangesAsync();

        await eventScope.PublishEventAsync(
            new PersonCreatedEvent
            {
                EventId = Guid.NewGuid(),
                PersonId = person.PersonId,
                Details = EventModels.PersonDetails.FromModel(person),
                TrnRequestMetadata = sourceRequest is TrnRequestMetadata metadata
                    ? EventModels.TrnRequestMetadata.FromModel(metadata)
                    : null
            });

        return person;
    }

    public async Task UpdatePersonDetailsAsync(UpdatePersonDetailsOptions options, ProcessContext processContext)
    {
        var person = await dbContext.Persons.FindAsync(options.PersonId);

        if (person is null)
        {
            throw new NotFoundException(options.PersonId, nameof(Person));
        }

        if (person.Status is PersonStatus.Deactivated)
        {
            throw new InvalidOperationException("Cannot update a person that is deactivated.");
        }

        await using var eventScope = eventPublisher.GetOrCreateEventScope(processContext);

        var oldPersonEventModel = EventModels.PersonDetails.FromModel(person);

        (options.FirstName).MatchSome(firstName => person.FirstName = firstName);
        (options.MiddleName).MatchSome(middleName => person.MiddleName = middleName);
        (options.LastName).MatchSome(lastName => person.LastName = lastName);
        (options.DateOfBirth).MatchSome(dateOfBirth => person.DateOfBirth = dateOfBirth);
        (options.EmailAddress).MatchSome(emailAddress => person.EmailAddress = (string?)emailAddress);
        (options.NationalInsuranceNumber).MatchSome(nationalInsuranceNumber => person.NationalInsuranceNumber = (string?)nationalInsuranceNumber);
        (options.Gender).MatchSome(gender => person.Gender = gender);

        var changes = 0 |
            (person.FirstName != oldPersonEventModel.FirstName ? PersonDetailsUpdatedEventChanges.FirstName : 0) |
            (person.MiddleName != oldPersonEventModel.MiddleName ? PersonDetailsUpdatedEventChanges.MiddleName : 0) |
            (person.LastName != oldPersonEventModel.LastName ? PersonDetailsUpdatedEventChanges.LastName : 0) |
            (person.DateOfBirth != oldPersonEventModel.DateOfBirth ? PersonDetailsUpdatedEventChanges.DateOfBirth : 0) |
            (person.EmailAddress != oldPersonEventModel.EmailAddress ? PersonDetailsUpdatedEventChanges.EmailAddress : 0) |
            (person.NationalInsuranceNumber != oldPersonEventModel.NationalInsuranceNumber ? PersonDetailsUpdatedEventChanges.NationalInsuranceNumber : 0) |
            (person.Gender != oldPersonEventModel.Gender ? PersonDetailsUpdatedEventChanges.Gender : 0);

        if (changes is PersonDetailsUpdatedEventChanges.None)
        {
            return;
        }

        person.UpdatedOn = processContext.Now;

        if (changes.HasAnyFlag(PersonDetailsUpdatedEventChanges.NameChange) && options.CreatePreviousName)
        {
            dbContext.PreviousNames.Add(new PreviousName
            {
                PreviousNameId = Guid.NewGuid(),
                PersonId = options.PersonId,
                FirstName = oldPersonEventModel.FirstName,
                MiddleName = oldPersonEventModel.MiddleName,
                LastName = oldPersonEventModel.LastName,
                CreatedOn = processContext.Now,
                UpdatedOn = processContext.Now
            });
        }

        await dbContext.SaveChangesAsync();

        await eventScope.PublishEventAsync(
            new PersonDetailsUpdatedEvent
            {
                EventId = Guid.NewGuid(),
                PersonId = person.PersonId,
                Changes = changes,
                OldPersonDetails = oldPersonEventModel,
                PersonDetails = EventModels.PersonDetails.FromModel(person)
            });
    }

    public async Task DeactivatePersonAsync(Guid personId, ProcessContext processContext)
    {
        var deactivatingPerson = await dbContext.Persons.FindAsync(personId);

        if (deactivatingPerson is null)
        {
            throw new NotFoundException(personId, nameof(Person));
        }

        if (deactivatingPerson.Status is PersonStatus.Deactivated)
        {
            throw new InvalidOperationException("Cannot deactivate a person that is already deactivated.");
        }

        await using var eventScope = eventPublisher.GetOrCreateEventScope(processContext);

        deactivatingPerson.Status = PersonStatus.Deactivated;
        deactivatingPerson.UpdatedOn = processContext.Now;

        await dbContext.SaveChangesAsync();

        await eventScope.PublishEventAsync(new PersonDeactivatedEvent
        {
            EventId = Guid.NewGuid(),
            PersonId = deactivatingPerson.PersonId,
            MergedWithPersonId = null
        });
    }

    public async Task ReactivatePersonAsync(Guid personId, ProcessContext processContext)
    {
        var reactivatingPerson = await dbContext.Persons.FindAsync(personId);

        if (reactivatingPerson is null)
        {
            throw new NotFoundException(personId, nameof(Person));
        }

        if (reactivatingPerson.Status is PersonStatus.Active)
        {
            throw new InvalidOperationException("Cannot reactivate a person that is already active.");
        }

        await using var eventScope = eventPublisher.GetOrCreateEventScope(processContext);

        reactivatingPerson.Status = PersonStatus.Active;
        reactivatingPerson.UpdatedOn = processContext.Now;

        await dbContext.SaveChangesAsync();

        await eventScope.PublishEventAsync(new PersonReactivatedEvent
        {
            EventId = Guid.NewGuid(),
            PersonId = reactivatingPerson.PersonId
        });
    }

    public async Task DeactivatePersonViaMergeAsync(DeactivatePersonViaMergeOptions options, ProcessContext processContext)
    {
        var deactivatingPerson = await dbContext.Persons.FindAsync(options.DeactivatingPersonId);

        if (deactivatingPerson is null)
        {
            throw new NotFoundException(options.DeactivatingPersonId, nameof(Person));
        }

        if (deactivatingPerson.Status is PersonStatus.Deactivated)
        {
            throw new InvalidOperationException("Cannot deactivate a person that is already deactivated.");
        }

        await using var eventScope = eventPublisher.GetOrCreateEventScope(processContext);

        var retainedPerson = await dbContext.Persons.FindAsync(options.RetainedPersonId);

        if (retainedPerson is null)
        {
            throw new NotFoundException(options.RetainedPersonId, nameof(Person));
        }

        if (retainedPerson.Status is PersonStatus.Deactivated)
        {
            throw new InvalidOperationException("Cannot merge a person that is deactivated.");
        }

        deactivatingPerson.Status = PersonStatus.Deactivated;
        deactivatingPerson.MergedWithPersonId = options.RetainedPersonId;
        deactivatingPerson.UpdatedOn = processContext.Now;

        await dbContext.SaveChangesAsync();

        await eventScope.PublishEventAsync(
            new PersonDeactivatedEvent
            {
                EventId = Guid.NewGuid(),
                PersonId = options.DeactivatingPersonId,
                MergedWithPersonId = options.RetainedPersonId
            });
    }

    public async Task MergePersonsAsync(MergePersonsOptions options, ProcessContext processContext)
    {
        var deactivatingPerson = await dbContext.Persons.FindAsync(options.DeactivatingPersonId);

        if (deactivatingPerson is null)
        {
            throw new NotFoundException(options.DeactivatingPersonId, nameof(Person));
        }

        if (deactivatingPerson.Status is PersonStatus.Deactivated)
        {
            throw new InvalidOperationException("Cannot deactivate a person that is already deactivated.");
        }

        var retainedPerson = await dbContext.Persons.FindAsync(options.RetainedPersonId);

        if (retainedPerson is null)
        {
            throw new NotFoundException(options.RetainedPersonId, nameof(Person));
        }

        if (retainedPerson.Status is PersonStatus.Deactivated)
        {
            throw new InvalidOperationException("Cannot merge a person that is deactivated.");
        }

        await using var eventScope = eventPublisher.GetOrCreateEventScope(processContext);

        await UpdatePersonDetailsAsync(
            new UpdatePersonDetailsOptions
            {
                PersonId = options.RetainedPersonId,
                CreatePreviousName = false,
                FirstName = options.FirstName,
                MiddleName = options.MiddleName,
                LastName = options.LastName,
                DateOfBirth = options.DateOfBirth,
                EmailAddress = options.EmailAddress,
                NationalInsuranceNumber = options.NationalInsuranceNumber,
                Gender = options.Gender
            },
            processContext);

        await DeactivatePersonViaMergeAsync(
            new DeactivatePersonViaMergeOptions(options.DeactivatingPersonId, options.RetainedPersonId),
            processContext);

        // If the deactivated person was associated with any One Login Users, transfer that association to the retained person
        var oneLoginUsers = await dbContext.OneLoginUsers
            .Select(o => o.Subject)
            .ToArrayAsync();

        foreach (var user in oneLoginUsers)
        {
            await oneLoginService.SetUserMatchedAsync(
                new SetUserMatchedOptions
                {
                    OneLoginUserSubject = user,
                    MatchedPersonId = retainedPerson.PersonId,
                    MatchRoute = OneLoginUserMatchRoute.SupportUi,
                    MatchedAttributes = null
                },
                processContext);
        }
    }
}
