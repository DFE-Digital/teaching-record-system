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
            request.PersonDetails,
            now);

        var createdEvent = new LegacyEvents.PersonCreatedEvent
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = now,
            RaisedBy = request.UserId,
            PersonId = person.PersonId,
            PersonAttributes = personAttributes,
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

        var updateResult = person!.UpdateDetails(
            request.PersonDetails,
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
                NameChangeReason = request.NameChangeJustification?.Reason.GetDisplayName(),
                NameChangeEvidenceFile = request.NameChangeJustification?.Evidence?.ToEventModel(),
                DetailsChangeReason = request.DetailsChangeJustification?.Reason.GetDisplayName(),
                DetailsChangeReasonDetail = request.DetailsChangeJustification?.ReasonDetail,
                DetailsChangeEvidenceFile = request.DetailsChangeJustification?.Evidence?.ToEventModel(),
                Changes = (LegacyEvents.PersonDetailsUpdatedEventChanges)updateResult.Changes
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

        person!.SetStatus(
            request.TargetStatus,
            request.TargetStatus == PersonStatus.Deactivated
                ? request.DeactivateJustification!.Reason.GetDisplayName()
                : request.ReactivateJustification!.Reason.GetDisplayName(),
            request.TargetStatus == PersonStatus.Deactivated
                ? request.DeactivateJustification!.ReasonDetail
                : request.ReactivateJustification!.ReasonDetail,
            request.TargetStatus == PersonStatus.Deactivated
                ? request.DeactivateJustification!.Evidence?.ToEventModel()
                : request.ReactivateJustification!.Evidence?.ToEventModel(),
            request.UserId,
            now,
            out var @event);

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
