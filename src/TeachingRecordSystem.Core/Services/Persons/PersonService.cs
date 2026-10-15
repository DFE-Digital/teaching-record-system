using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Services.OneLogin;
using TeachingRecordSystem.Core.Services.RoutesToProfessionalStatus;

namespace TeachingRecordSystem.Core.Services.Persons;

public class PersonService(
    TrsDbContext dbContext,
    OneLoginService oneLoginService,
    RoutesToProfessionalStatusService routesToProfessionalStatusService,
    IEventPublisher eventPublisher)
{
    public Task<Person?> GetPersonAsync(Guid personId, bool includeDeactivatedPersons = false, CancellationToken cancellationToken = default)
    {
        var persons = dbContext.Persons.AsQueryable();

        if (includeDeactivatedPersons)
        {
            persons = persons.IgnoreQueryFilters([QueryFilterNames.Person.Deactivated]);
        }

        return persons.SingleOrDefaultAsync(p => p.PersonId == personId, cancellationToken);
    }

    public async Task<Person> CreatePersonAsync(CreatePersonOptions options, ProcessContext processContext, CancellationToken cancellationToken = default)
    {
        await using var eventScope = eventPublisher.GetOrCreateEventScope(processContext);

        TrnRequestMetadata? sourceRequest = null;
        if (options.SourceTrnRequest is (var applicationUserId, var requestId))
        {
            sourceRequest = await dbContext.TrnRequestMetadata.FindAsync(new object?[] { applicationUserId, requestId }, cancellationToken);

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
        await dbContext.SaveChangesAsync(cancellationToken);

        await eventScope.PublishEventAsync(
            new PersonCreatedEvent
            {
                EventId = Guid.NewGuid(),
                PersonId = person.PersonId,
                Details = EventModels.PersonDetails.FromModel(person),
                TrnRequestMetadata = sourceRequest is TrnRequestMetadata metadata
                    ? EventModels.TrnRequestMetadata.FromModel(metadata)
                    : null
            },
            cancellationToken);

        return person;
    }

    public async Task UpdatePersonDetailsAsync(UpdatePersonDetailsOptions options, ProcessContext processContext, CancellationToken cancellationToken = default)
    {
        var person = await dbContext.Persons.IgnoreQueryFilters([QueryFilterNames.Person.Deactivated]).SingleOrDefaultAsync(p => p.PersonId == options.PersonId, cancellationToken);

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

        await dbContext.SaveChangesAsync(cancellationToken);

        await eventScope.PublishEventAsync(
            new PersonDetailsUpdatedEvent
            {
                EventId = Guid.NewGuid(),
                PersonId = person.PersonId,
                Changes = changes,
                OldPersonDetails = oldPersonEventModel,
                PersonDetails = EventModels.PersonDetails.FromModel(person)
            },
            cancellationToken);
    }

    public async Task DeactivatePersonAsync(DeactivatePersonOptions options, ProcessContext processContext, CancellationToken cancellationToken = default)
    {
        var deactivatingPerson = await dbContext.Persons.IgnoreQueryFilters([QueryFilterNames.Person.Deactivated]).SingleOrDefaultAsync(p => p.PersonId == options.PersonId, cancellationToken);

        if (deactivatingPerson is null)
        {
            throw new NotFoundException(options.PersonId, nameof(Person));
        }

        if (deactivatingPerson.Status is PersonStatus.Deactivated)
        {
            throw new InvalidOperationException("Cannot deactivate a person that is already deactivated.");
        }

        await using var eventScope = eventPublisher.GetOrCreateEventScope(processContext);

        deactivatingPerson.DateOfDeath = options.DateOfDeath;
        deactivatingPerson.Status = PersonStatus.Deactivated;
        deactivatingPerson.UpdatedOn = processContext.Now;

        await dbContext.SaveChangesAsync(cancellationToken);

        var changes = PersonDeactivatedEventChanges.PersonStatus | (options.DateOfDeath.HasValue ? PersonDeactivatedEventChanges.DateOfDeath : 0);

        await eventScope.PublishEventAsync(new PersonDeactivatedEvent
        {
            EventId = Guid.NewGuid(),
            PersonId = deactivatingPerson.PersonId,
            Changes = changes,
            MergedWithPersonId = null,
            DateOfDeath = options.DateOfDeath
        }, cancellationToken);
    }

    public async Task ReactivatePersonAsync(Guid personId, ProcessContext processContext, CancellationToken cancellationToken = default)
    {
        var reactivatingPerson = await dbContext.Persons.IgnoreQueryFilters([QueryFilterNames.Person.Deactivated]).SingleOrDefaultAsync(p => p.PersonId == personId, cancellationToken);

        if (reactivatingPerson is null)
        {
            throw new NotFoundException(personId, nameof(Person));
        }

        if (reactivatingPerson.Status is PersonStatus.Active)
        {
            throw new InvalidOperationException("Cannot reactivate a person that is already active.");
        }

        if (reactivatingPerson.MergedWithPersonId.HasValue)
        {
            throw new InvalidOperationException("Cannot reactivate a person that has been merged with another person.");
        }

        await using var eventScope = eventPublisher.GetOrCreateEventScope(processContext);

        var oldDateOfDeath = reactivatingPerson.DateOfDeath;

        reactivatingPerson.DateOfDeath = null;
        reactivatingPerson.Status = PersonStatus.Active;
        reactivatingPerson.UpdatedOn = processContext.Now;

        await dbContext.SaveChangesAsync(cancellationToken);

        var changes = PersonReactivatedEventChanges.PersonStatus | (oldDateOfDeath.HasValue ? PersonReactivatedEventChanges.DateOfDeath : 0);

        await eventScope.PublishEventAsync(new PersonReactivatedEvent
        {
            EventId = Guid.NewGuid(),
            PersonId = reactivatingPerson.PersonId,
            Changes = changes
        }, cancellationToken);
    }

    public async Task DeactivatePersonViaMergeAsync(DeactivatePersonViaMergeOptions options, ProcessContext processContext, CancellationToken cancellationToken = default)
    {
        var deactivatingPerson = await dbContext.Persons.IgnoreQueryFilters([QueryFilterNames.Person.Deactivated]).SingleOrDefaultAsync(p => p.PersonId == options.DeactivatingPersonId, cancellationToken);

        if (deactivatingPerson is null)
        {
            throw new NotFoundException(options.DeactivatingPersonId, nameof(Person));
        }

        if (deactivatingPerson.Status is PersonStatus.Deactivated)
        {
            throw new InvalidOperationException("Cannot deactivate a person that is already deactivated.");
        }

        await using var eventScope = eventPublisher.GetOrCreateEventScope(processContext);

        var retainedPerson = await dbContext.Persons.IgnoreQueryFilters([QueryFilterNames.Person.Deactivated]).SingleOrDefaultAsync(p => p.PersonId == options.RetainedPersonId, cancellationToken);

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

        // If the deactivated person was associated with any One Login Users, transfer that association to the retained person
        var oneLoginUsers = await dbContext.OneLoginUsers
            .Where(o => o.PersonId == options.DeactivatingPersonId)
            .Select(o => o.Subject)
            .ToArrayAsync(cancellationToken);

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
                processContext,
                cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        await eventScope.PublishEventAsync(
            new PersonDeactivatedEvent
            {
                EventId = Guid.NewGuid(),
                PersonId = options.DeactivatingPersonId,
                Changes = PersonDeactivatedEventChanges.MergedWithPersonId,
                MergedWithPersonId = options.RetainedPersonId,
                DateOfDeath = null
            },
            cancellationToken);
    }

    public async Task MergePersonsAsync(MergePersonsOptions options, ProcessContext processContext, CancellationToken cancellationToken = default)
    {
        var deactivatingPerson = await dbContext.Persons.IgnoreQueryFilters([QueryFilterNames.Person.Deactivated]).SingleOrDefaultAsync(p => p.PersonId == options.DeactivatingPersonId, cancellationToken);

        if (deactivatingPerson is null)
        {
            throw new NotFoundException(options.DeactivatingPersonId, nameof(Person));
        }

        var retainedPerson = await dbContext.Persons.IgnoreQueryFilters([QueryFilterNames.Person.Deactivated]).SingleOrDefaultAsync(p => p.PersonId == options.RetainedPersonId, cancellationToken);

        if (retainedPerson is null)
        {
            throw new NotFoundException(options.RetainedPersonId, nameof(Person));
        }

        var blockers = GetMergeBlockers(
            await GetMergeDetailsAsync(retainedPerson, cancellationToken),
            await GetMergeDetailsAsync(deactivatingPerson, cancellationToken));

        if (blockers is not PersonMergeBlockers.None)
        {
            throw new InvalidOperationException($"Person '{deactivatingPerson.PersonId}' cannot be merged into person '{retainedPerson.PersonId}': {blockers}.");
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
            processContext,
            cancellationToken);

        await CopyRoutesToRetainedPersonAsync(options.DeactivatingPersonId, options.RetainedPersonId, processContext, cancellationToken);

        await MergeInductionIntoRetainedPersonAsync(deactivatingPerson, retainedPerson, processContext, cancellationToken);

        await DeactivatePersonViaMergeAsync(
            new DeactivatePersonViaMergeOptions(options.DeactivatingPersonId, options.RetainedPersonId),
            processContext,
            cancellationToken);

        // If the deactivated person was associated with any One Login Users, transfer that association to the retained person
        var oneLoginUsers = await dbContext.OneLoginUsers
            .Where(o => o.PersonId == options.DeactivatingPersonId)
            .Select(o => o.Subject)
            .ToArrayAsync(cancellationToken);

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
                processContext,
                cancellationToken);
        }
    }

    private async Task CopyRoutesToRetainedPersonAsync(
        Guid deactivatingPersonId,
        Guid retainedPersonId,
        ProcessContext processContext,
        CancellationToken cancellationToken)
    {
        // The Deleted query filter leaves out any routes that have been deleted
        var routes = await dbContext.RouteToProfessionalStatuses
            .Where(r => r.PersonId == deactivatingPersonId)
            .OrderBy(r => r.CreatedOn)
            .ToArrayAsync(cancellationToken);

        foreach (var route in routes)
        {
            await routesToProfessionalStatusService.CreateRouteToProfessionalStatusAsync(
                new CreateRouteToProfessionalStatusOptions
                {
                    PersonId = retainedPersonId,
                    RouteToProfessionalStatusTypeId = route.RouteToProfessionalStatusTypeId,
                    Status = route.Status,
                    SourceApplicationUserId = route.SourceApplicationUserId,
                    SourceApplicationReference = route.SourceApplicationReference,
                    HoldsFrom = route.HoldsFrom,
                    TrainingStartDate = route.TrainingStartDate,
                    TrainingEndDate = route.TrainingEndDate,
                    TrainingSubjectIds = route.TrainingSubjectIds,
                    TrainingAgeSpecialismType = route.TrainingAgeSpecialismType,
                    TrainingAgeSpecialismRangeFrom = route.TrainingAgeSpecialismRangeFrom,
                    TrainingAgeSpecialismRangeTo = route.TrainingAgeSpecialismRangeTo,
                    TrainingCountryId = route.TrainingCountryId,
                    TrainingProviderId = route.TrainingProviderId,
                    DegreeTypeId = route.DegreeTypeId,
                    IsExemptFromInduction = route.ExemptFromInduction
                },
                processContext,
                cancellationToken);
        }
    }

    private async Task MergeInductionIntoRetainedPersonAsync(
        Person deactivatingPerson,
        Person retainedPerson,
        ProcessContext processContext,
        CancellationToken cancellationToken)
    {
        await using var eventScope = eventPublisher.GetOrCreateEventScope(processContext);

        // Includes any routes just copied from the deactivating person
        var retainedPersonRoutes = await dbContext.RouteToProfessionalStatuses
            .Include(r => r.RouteToProfessionalStatusType)
            .Where(r => r.PersonId == retainedPerson.PersonId)
            .ToArrayAsync(cancellationToken);

        var oldInduction = EventModels.Induction.FromModel(retainedPerson);

        if (!retainedPerson.MergeInductionFrom(deactivatingPerson, retainedPersonRoutes, processContext.Now))
        {
            return;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        var induction = EventModels.Induction.FromModel(retainedPerson);

        await eventScope.PublishEventAsync(
            new PersonInductionUpdatedEvent
            {
                EventId = Guid.NewGuid(),
                PersonId = retainedPerson.PersonId,
                Induction = induction,
                OldInduction = oldInduction,
                Changes = PersonInductionUpdatedEvent.GetChanges(induction, oldInduction)
            },
            cancellationToken);
    }

    private async Task<PersonMergeDetails> GetMergeDetailsAsync(Person person, CancellationToken cancellationToken) =>
        new(
            person.Status,
            person.InductionStatus,
            person.InductionStatusWithoutExemption,
            person.InductionStartDate,
            person.InductionCompletedDate,
            HasOpenAlert: await dbContext.Alerts.AnyAsync(a => a.PersonId == person.PersonId && a.IsOpen, cancellationToken));

    public static PersonMergeBlockers GetMergeBlockers(PersonMergeDetails retainedPerson, PersonMergeDetails deactivatingPerson)
    {
        var blockers = PersonMergeBlockers.None;

        if (retainedPerson.Status is PersonStatus.Deactivated || deactivatingPerson.Status is PersonStatus.Deactivated)
        {
            blockers |= PersonMergeBlockers.Deactivated;
        }

        // An alert on the retained person is fine; it's only an alert on the person being deactivated that would be lost
        if (deactivatingPerson.HasOpenAlert)
        {
            blockers |= PersonMergeBlockers.OpenAlert;
        }

        // Check the status without exemption too, otherwise an exempt record could bring a fallback of failed in Wales with it
        if (deactivatingPerson.InductionStatus is InductionStatus.FailedInWales ||
            deactivatingPerson.InductionStatusWithoutExemption is InductionStatus.FailedInWales)
        {
            blockers |= PersonMergeBlockers.InductionStatusFailedInWales;
        }

        if ((retainedPerson.InductionStatus is InductionStatus.Failed && deactivatingPerson.InductionStatus is not InductionStatus.None) ||
            (deactivatingPerson.InductionStatus is InductionStatus.Failed && retainedPerson.InductionStatus is not InductionStatus.None))
        {
            blockers |= PersonMergeBlockers.InductionStatusFailed;
        }

        if (retainedPerson.InductionStartDate is DateOnly retainedStartDate &&
            deactivatingPerson.InductionStartDate is DateOnly deactivatingStartDate &&
            retainedStartDate != deactivatingStartDate)
        {
            blockers |= PersonMergeBlockers.InductionStartDateMismatch;
        }

        if (retainedPerson.InductionCompletedDate is DateOnly retainedCompletedDate &&
            deactivatingPerson.InductionCompletedDate is DateOnly deactivatingCompletedDate &&
            retainedCompletedDate != deactivatingCompletedDate)
        {
            blockers |= PersonMergeBlockers.InductionCompletedDateMismatch;
        }

        return blockers;
    }
}
