using System.Diagnostics;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.Services.RoutesToProfessionalStatus;

public class RoutesToProfessionalStatusService(
    TrsDbContext dbContext,
    ReferenceDataCache referenceDataCache,
    TimeProvider timeProvider,
    IEventPublisher eventPublisher)
{
    public async Task<RouteToProfessionalStatus> CreateRouteToProfessionalStatusAsync(
        CreateRouteToProfessionalStatusOptions options,
        ProcessContext processContext)
    {
        await using var eventScope = eventPublisher.GetOrCreateEventScope(processContext);

        var person = await dbContext.Persons
            .Include(p => p.Qualifications)
            .SingleOrDefaultAsync(p => p.PersonId == options.PersonId)
            ?? throw new NotFoundException(options.PersonId, nameof(Person));

        Debug.Assert(person.Qualifications is not null);

        var allRouteTypes = await referenceDataCache.GetRouteToProfessionalStatusTypesAsync();
        var routeType = allRouteTypes.Single(r => r.RouteToProfessionalStatusTypeId == options.RouteToProfessionalStatusTypeId);
        var now = timeProvider.UtcNow;

        var route = new RouteToProfessionalStatus()
        {
            QualificationId = Guid.NewGuid(),
            CreatedOn = now,
            UpdatedOn = now,
            PersonId = person.PersonId,
            SourceApplicationUserId = options.SourceApplicationUserId,
            SourceApplicationReference = options.SourceApplicationReference,
            RouteToProfessionalStatusTypeId = options.RouteToProfessionalStatusTypeId,
            Status = options.Status,
            HoldsFrom = options.HoldsFrom,
            DegreeTypeId = options.DegreeTypeId,
            ExemptFromInduction = options.IsExemptFromInduction,
            TrainingStartDate = options.TrainingStartDate,
            TrainingEndDate = options.TrainingEndDate,
            TrainingAgeSpecialismRangeFrom = options.TrainingAgeSpecialismRangeFrom,
            TrainingAgeSpecialismRangeTo = options.TrainingAgeSpecialismRangeTo,
            TrainingAgeSpecialismType = options.TrainingAgeSpecialismType,
            TrainingCountryId = options.TrainingCountryId,
            TrainingProviderId = options.TrainingProviderId,
            TrainingSubjectIds = options.TrainingSubjectIds ?? []
        };

        var oldPersonAttributes = EventModels.ProfessionalStatusPersonAttributes.FromModel(person);

        var professionalStatusType = routeType.ProfessionalStatusType;
        var allRoutes = person.Qualifications.OfType<RouteToProfessionalStatus>().Append(route).ToArray();

        var oldInduction = EventModels.Induction.FromModel(person);
        if (professionalStatusType == ProfessionalStatusType.QualifiedTeacherStatus)
        {
            RefreshExemptFromInductionDueToQtsDate(route);
            person.RefreshInductionStatusForQtsProfessionalStatusChanged(now, allRouteTypes, allRoutes);
        }
        var newInduction = EventModels.Induction.FromModel(person);

        person.RefreshProfessionalStatusAttributes(professionalStatusType, allRouteTypes, allRoutes);

        if (options.RouteToProfessionalStatusTypeId == RouteToProfessionalStatusType.QtlsAndSetMembershipId)
        {
            person.RefreshQtlsStatus(allRoutes);
        }

        dbContext.RouteToProfessionalStatuses.Add(route);
        await dbContext.SaveChangesAsync();

        await eventScope.PublishEventAsync(
            new RouteToProfessionalStatusCreatedEvent
            {
                EventId = Guid.NewGuid(),
                PersonId = person.PersonId,
                RouteToProfessionalStatus = EventModels.RouteToProfessionalStatus.FromModel(route)
            });

        await PublishPersonChangesAsync(eventScope, person, oldPersonAttributes, newInduction, oldInduction);

        return route;
    }

    public async Task<RouteToProfessionalStatusUpdatedEventChanges> UpdateRouteToProfessionalStatusAsync(
        UpdateRouteToProfessionalStatusOptions options,
        ProcessContext processContext)
    {
        var route = await GetRouteAsync(options.QualificationId);

        await using var eventScope = eventPublisher.GetOrCreateEventScope(processContext);

        Debug.Assert(route.Person is not null);
        Debug.Assert(route.Person.Qualifications is not null);

        var person = route.Person;
        var allRouteTypes = await referenceDataCache.GetRouteToProfessionalStatusTypesAsync();
        var now = timeProvider.UtcNow;

        var oldEventModel = EventModels.RouteToProfessionalStatus.FromModel(route);
        var oldPersonAttributes = EventModels.ProfessionalStatusPersonAttributes.FromModel(person);
        var oldProfessionalStatusType = allRouteTypes
            .Single(r => r.RouteToProfessionalStatusTypeId == route.RouteToProfessionalStatusTypeId)
            .ProfessionalStatusType;

        options.RouteToProfessionalStatusTypeId.MatchSome(routeTypeId => route.RouteToProfessionalStatusTypeId = routeTypeId);
        options.Status.MatchSome(status => route.Status = status);
        options.HoldsFrom.MatchSome(holdsFrom => route.HoldsFrom = holdsFrom);
        options.TrainingStartDate.MatchSome(trainingStartDate => route.TrainingStartDate = trainingStartDate);
        options.TrainingEndDate.MatchSome(trainingEndDate => route.TrainingEndDate = trainingEndDate);
        options.TrainingSubjectIds.MatchSome(trainingSubjectIds => route.TrainingSubjectIds = trainingSubjectIds);
        options.TrainingAgeSpecialismType.MatchSome(trainingAgeSpecialismType => route.TrainingAgeSpecialismType = trainingAgeSpecialismType);
        options.TrainingAgeSpecialismRangeFrom.MatchSome(trainingAgeSpecialismRangeFrom => route.TrainingAgeSpecialismRangeFrom = trainingAgeSpecialismRangeFrom);
        options.TrainingAgeSpecialismRangeTo.MatchSome(trainingAgeSpecialismRangeTo => route.TrainingAgeSpecialismRangeTo = trainingAgeSpecialismRangeTo);
        options.TrainingCountryId.MatchSome(trainingCountryId => route.TrainingCountryId = trainingCountryId);
        options.TrainingProviderId.MatchSome(trainingProviderId => route.TrainingProviderId = trainingProviderId);
        options.DegreeTypeId.MatchSome(degreeTypeId => route.DegreeTypeId = degreeTypeId);
        options.ExemptFromInduction.MatchSome(exemptFromInduction => route.ExemptFromInduction = exemptFromInduction);

        var professionalStatusType = allRouteTypes
            .Single(r => r.RouteToProfessionalStatusTypeId == route.RouteToProfessionalStatusTypeId)
            .ProfessionalStatusType;

        if (professionalStatusType != oldProfessionalStatusType)
        {
            throw new NotSupportedException($"Cannot change the {nameof(ProfessionalStatusType)} for an existing {nameof(RouteToProfessionalStatus)}.");
        }

        var oldInduction = EventModels.Induction.FromModel(person);
        if (professionalStatusType == ProfessionalStatusType.QualifiedTeacherStatus)
        {
            RefreshExemptFromInductionDueToQtsDate(route);
            person.RefreshInductionStatusForQtsProfessionalStatusChanged(now, allRouteTypes);
        }
        var newInduction = EventModels.Induction.FromModel(person);

        person.RefreshProfessionalStatusAttributes(professionalStatusType, allRouteTypes);

        if (route.RouteToProfessionalStatusTypeId == RouteToProfessionalStatusType.QtlsAndSetMembershipId)
        {
            person.RefreshQtlsStatus();
        }

        var changes = RouteToProfessionalStatusUpdatedEventChanges.None |
            (route.RouteToProfessionalStatusTypeId != oldEventModel.RouteToProfessionalStatusTypeId ? RouteToProfessionalStatusUpdatedEventChanges.Route : RouteToProfessionalStatusUpdatedEventChanges.None) |
            (route.Status != oldEventModel.Status ? RouteToProfessionalStatusUpdatedEventChanges.Status : RouteToProfessionalStatusUpdatedEventChanges.None) |
            (route.HoldsFrom != oldEventModel.HoldsFrom ? RouteToProfessionalStatusUpdatedEventChanges.HoldsFrom : RouteToProfessionalStatusUpdatedEventChanges.None) |
            (route.TrainingStartDate != oldEventModel.TrainingStartDate ? RouteToProfessionalStatusUpdatedEventChanges.StartDate : RouteToProfessionalStatusUpdatedEventChanges.None) |
            (route.TrainingEndDate != oldEventModel.TrainingEndDate ? RouteToProfessionalStatusUpdatedEventChanges.EndDate : RouteToProfessionalStatusUpdatedEventChanges.None) |
            ((route.TrainingSubjectIds.Except(oldEventModel.TrainingSubjectIds).Any() || oldEventModel.TrainingSubjectIds.Except(route.TrainingSubjectIds).Any()) ? RouteToProfessionalStatusUpdatedEventChanges.TrainingSubjectIds : RouteToProfessionalStatusUpdatedEventChanges.None) |
            (route.TrainingAgeSpecialismType != oldEventModel.TrainingAgeSpecialismType ? RouteToProfessionalStatusUpdatedEventChanges.TrainingAgeSpecialismType : RouteToProfessionalStatusUpdatedEventChanges.None) |
            (route.TrainingAgeSpecialismRangeFrom != oldEventModel.TrainingAgeSpecialismRangeFrom ? RouteToProfessionalStatusUpdatedEventChanges.TrainingAgeSpecialismRangeFrom : RouteToProfessionalStatusUpdatedEventChanges.None) |
            (route.TrainingAgeSpecialismRangeTo != oldEventModel.TrainingAgeSpecialismRangeTo ? RouteToProfessionalStatusUpdatedEventChanges.TrainingAgeSpecialismRangeTo : RouteToProfessionalStatusUpdatedEventChanges.None) |
            (route.TrainingCountryId != oldEventModel.TrainingCountryId ? RouteToProfessionalStatusUpdatedEventChanges.TrainingCountry : RouteToProfessionalStatusUpdatedEventChanges.None) |
            (route.TrainingProviderId != oldEventModel.TrainingProviderId ? RouteToProfessionalStatusUpdatedEventChanges.TrainingProvider : RouteToProfessionalStatusUpdatedEventChanges.None) |
            (route.ExemptFromInduction != oldEventModel.ExemptFromInduction ? RouteToProfessionalStatusUpdatedEventChanges.ExemptFromInduction : RouteToProfessionalStatusUpdatedEventChanges.None) |
            (route.DegreeTypeId != oldEventModel.DegreeTypeId ? RouteToProfessionalStatusUpdatedEventChanges.DegreeType : RouteToProfessionalStatusUpdatedEventChanges.None) |
            (route.ExemptFromInductionDueToQtsDate != oldEventModel.ExemptFromInductionDueToQtsDate ? RouteToProfessionalStatusUpdatedEventChanges.ExemptFromInductionDueToQtsDate : RouteToProfessionalStatusUpdatedEventChanges.None);

        var personAttributesChanges = GetPersonAttributesChanges(EventModels.ProfessionalStatusPersonAttributes.FromModel(person), oldPersonAttributes);
        var inductionChanges = PersonInductionUpdatedEvent.GetChanges(newInduction, oldInduction);

        // Refreshing the person's attributes or their induction is a change even when no field on the route itself
        // moved, so nothing is skipped unless all three are unchanged.
        if (changes == RouteToProfessionalStatusUpdatedEventChanges.None &&
            personAttributesChanges == PersonProfessionalStatusAttributesUpdatedEventChanges.None &&
            inductionChanges == PersonInductionUpdatedEventChanges.None)
        {
            return changes;
        }

        route.UpdatedOn = now;
        await dbContext.SaveChangesAsync();

        if (changes != RouteToProfessionalStatusUpdatedEventChanges.None)
        {
            await eventScope.PublishEventAsync(
                new RouteToProfessionalStatusUpdatedEvent
                {
                    EventId = Guid.NewGuid(),
                    PersonId = route.PersonId,
                    RouteToProfessionalStatus = EventModels.RouteToProfessionalStatus.FromModel(route),
                    OldRouteToProfessionalStatus = oldEventModel,
                    Changes = changes
                });
        }

        await PublishPersonChangesAsync(eventScope, person, oldPersonAttributes, newInduction, oldInduction);

        return changes;
    }

    public async Task DeleteRouteToProfessionalStatusAsync(
        DeleteRouteToProfessionalStatusOptions options,
        ProcessContext processContext)
    {
        var route = await GetRouteAsync(options.QualificationId);

        await using var eventScope = eventPublisher.GetOrCreateEventScope(processContext);

        if (route.DeletedOn is not null)
        {
            throw new InvalidOperationException("Professional status is already deleted.");
        }

        if (route.Person is null)
        {
            throw new InvalidOperationException("Professional status is not linked to a person and cannot be deleted");
        }

        var person = route.Person;
        var allRouteTypes = await referenceDataCache.GetRouteToProfessionalStatusTypesAsync();
        var now = timeProvider.UtcNow;

        route.DeletedOn = now;
        route.UpdatedOn = now;

        var oldPersonAttributes = EventModels.ProfessionalStatusPersonAttributes.FromModel(person);

        var professionalStatusType = allRouteTypes
            .Single(r => r.RouteToProfessionalStatusTypeId == route.RouteToProfessionalStatusTypeId)
            .ProfessionalStatusType;

        var oldInduction = EventModels.Induction.FromModel(person);
        if (professionalStatusType == ProfessionalStatusType.QualifiedTeacherStatus)
        {
            RefreshExemptFromInductionDueToQtsDate(route);
            person.RefreshInductionStatusForQtsProfessionalStatusChanged(now, allRouteTypes);
        }
        var newInduction = EventModels.Induction.FromModel(person);

        person.RefreshProfessionalStatusAttributes(professionalStatusType, allRouteTypes);

        if (route.RouteToProfessionalStatusTypeId == RouteToProfessionalStatusType.QtlsAndSetMembershipId)
        {
            person.RefreshQtlsStatus();
        }

        await dbContext.SaveChangesAsync();

        await eventScope.PublishEventAsync(
            new RouteToProfessionalStatusDeletedEvent
            {
                EventId = Guid.NewGuid(),
                PersonId = route.PersonId,
                RouteToProfessionalStatus = EventModels.RouteToProfessionalStatus.FromModel(route)
            });

        await PublishPersonChangesAsync(eventScope, person, oldPersonAttributes, newInduction, oldInduction);
    }

    // The person's professional status attributes and their induction are changes to the person, not to the route,
    // so they go on the process as their own events rather than riding along on the route's.
    private static async Task PublishPersonChangesAsync(
        IEventScope eventScope,
        Person person,
        EventModels.ProfessionalStatusPersonAttributes oldPersonAttributes,
        EventModels.Induction newInduction,
        EventModels.Induction oldInduction)
    {
        var personAttributes = EventModels.ProfessionalStatusPersonAttributes.FromModel(person);
        var personAttributesChanges = GetPersonAttributesChanges(personAttributes, oldPersonAttributes);

        if (personAttributesChanges != PersonProfessionalStatusAttributesUpdatedEventChanges.None)
        {
            await eventScope.PublishEventAsync(
                new PersonProfessionalStatusAttributesUpdatedEvent
                {
                    EventId = Guid.NewGuid(),
                    PersonId = person.PersonId,
                    PersonAttributes = personAttributes,
                    OldPersonAttributes = oldPersonAttributes,
                    Changes = personAttributesChanges
                });
        }

        var inductionChanges = PersonInductionUpdatedEvent.GetChanges(newInduction, oldInduction);

        if (inductionChanges != PersonInductionUpdatedEventChanges.None)
        {
            await eventScope.PublishEventAsync(
                new PersonInductionUpdatedEvent
                {
                    EventId = Guid.NewGuid(),
                    PersonId = person.PersonId,
                    Induction = newInduction,
                    OldInduction = oldInduction,
                    Changes = inductionChanges
                });
        }
    }

    private static PersonProfessionalStatusAttributesUpdatedEventChanges GetPersonAttributesChanges(
        EventModels.ProfessionalStatusPersonAttributes personAttributes,
        EventModels.ProfessionalStatusPersonAttributes oldPersonAttributes) =>
        PersonProfessionalStatusAttributesUpdatedEventChanges.None |
        (personAttributes.QtsDate != oldPersonAttributes.QtsDate ? PersonProfessionalStatusAttributesUpdatedEventChanges.QtsDate : 0) |
        (personAttributes.EytsDate != oldPersonAttributes.EytsDate ? PersonProfessionalStatusAttributesUpdatedEventChanges.EytsDate : 0) |
        (personAttributes.HasEyps != oldPersonAttributes.HasEyps ? PersonProfessionalStatusAttributesUpdatedEventChanges.HasEyps : 0) |
        (personAttributes.PqtsDate != oldPersonAttributes.PqtsDate ? PersonProfessionalStatusAttributesUpdatedEventChanges.PqtsDate : 0) |
        (personAttributes.QtlsStatus != oldPersonAttributes.QtlsStatus ? PersonProfessionalStatusAttributesUpdatedEventChanges.QtlsStatus : 0);

    private static void RefreshExemptFromInductionDueToQtsDate(RouteToProfessionalStatus route)
    {
        if (route.HoldsFrom is null)
        {
            route.ExemptFromInductionDueToQtsDate = null;
            return;
        }

        route.ExemptFromInductionDueToQtsDate = route.HoldsFrom < new DateOnly(2000, 5, 7);
    }

    private async Task<RouteToProfessionalStatus> GetRouteAsync(Guid qualificationId) =>
        await dbContext.RouteToProfessionalStatuses
            .Include(r => r.Person)
            .ThenInclude(p => p!.Qualifications)
            .SingleOrDefaultAsync(r => r.QualificationId == qualificationId)
        ?? throw new NotFoundException(qualificationId, nameof(RouteToProfessionalStatus));
}
