using System.Diagnostics;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Events.Legacy;

namespace TeachingRecordSystem.Core.Services.RoutesToProfessionalStatus;

public class RoutesToProfessionalStatusService(
    TrsDbContext dbContext,
    ReferenceDataCache referenceDataCache,
    TimeProvider timeProvider)
{
    public async Task<RouteToProfessionalStatus> CreateRouteToProfessionalStatusAsync(CreateRouteToProfessionalStatusOptions options)
    {
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
            route.RefreshExemptFromInductionDueToQtsDate();
            person.RefreshInductionStatusForQtsProfessionalStatusChanged(now, allRouteTypes, allRoutes);
        }
        var newInduction = EventModels.Induction.FromModel(person);

        var personAttributesUpdated = person.RefreshProfessionalStatusAttributes(professionalStatusType, allRouteTypes, allRoutes);
        var qtlsStatusUpdated = options.RouteToProfessionalStatusTypeId == RouteToProfessionalStatusType.QtlsAndSetMembershipId && person.RefreshQtlsStatus(allRoutes);

        var changes = RouteToProfessionalStatusCreatedEventChanges.None |
            (professionalStatusType is ProfessionalStatusType.QualifiedTeacherStatus && personAttributesUpdated
                ? RouteToProfessionalStatusCreatedEventChanges.PersonQtsDate
                : 0) |
            (professionalStatusType is ProfessionalStatusType.EarlyYearsTeacherStatus && personAttributesUpdated
                ? RouteToProfessionalStatusCreatedEventChanges.PersonEytsDate
                : 0) |
            (professionalStatusType is ProfessionalStatusType.EarlyYearsProfessionalStatus && personAttributesUpdated
                ? RouteToProfessionalStatusCreatedEventChanges.PersonHasEyps
                : 0) |
            (professionalStatusType is ProfessionalStatusType.PartialQualifiedTeacherStatus && personAttributesUpdated
                ? RouteToProfessionalStatusCreatedEventChanges.PersonPqtsDate
                : 0) |
            (newInduction.Status != oldInduction.Status
                ? RouteToProfessionalStatusCreatedEventChanges.PersonInductionStatus
                : 0) |
            (newInduction.StatusWithoutExemption != oldInduction.StatusWithoutExemption
                ? RouteToProfessionalStatusCreatedEventChanges.PersonInductionStatusWithoutExemption
                : 0) |
            (qtlsStatusUpdated ? RouteToProfessionalStatusCreatedEventChanges.PersonQtlsStatus : 0);

        var @event = new RouteToProfessionalStatusCreatedEvent()
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = now,
            PersonId = person.PersonId,
            RaisedBy = options.CreatedBy,
            RouteToProfessionalStatus = EventModels.RouteToProfessionalStatus.FromModel(route),
            PersonAttributes = EventModels.ProfessionalStatusPersonAttributes.FromModel(person),
            ChangeReason = options.ChangeReason,
            ChangeReasonDetail = options.ChangeReasonDetail,
            EvidenceFile = options.EvidenceFile,
            OldPersonAttributes = oldPersonAttributes,
            Changes = changes,
            Induction = newInduction,
            OldInduction = oldInduction,
            AdditionalInformation = options.AdditionalInformation
        };

        dbContext.RouteToProfessionalStatuses.Add(route);
        dbContext.AddEventWithoutBroadcast(@event);
        await dbContext.SaveChangesAsync();

        return route;
    }

    public async Task<RouteToProfessionalStatusUpdatedEventChanges> UpdateRouteToProfessionalStatusAsync(UpdateRouteToProfessionalStatusOptions options)
    {
        var route = await GetRouteAsync(options.QualificationId);

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
            route.RefreshExemptFromInductionDueToQtsDate();
            person.RefreshInductionStatusForQtsProfessionalStatusChanged(now, allRouteTypes);
        }
        var newInduction = EventModels.Induction.FromModel(person);

        var personAttributesUpdated = person.RefreshProfessionalStatusAttributes(professionalStatusType, allRouteTypes);
        var qtlsStatusUpdated = route.RouteToProfessionalStatusTypeId == RouteToProfessionalStatusType.QtlsAndSetMembershipId && person.RefreshQtlsStatus();

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
            (route.ExemptFromInductionDueToQtsDate != oldEventModel.ExemptFromInductionDueToQtsDate ? RouteToProfessionalStatusUpdatedEventChanges.ExemptFromInductionDueToQtsDate : RouteToProfessionalStatusUpdatedEventChanges.None) |
            (professionalStatusType is ProfessionalStatusType.QualifiedTeacherStatus && personAttributesUpdated ? RouteToProfessionalStatusUpdatedEventChanges.PersonQtsDate : 0) |
            (professionalStatusType is ProfessionalStatusType.EarlyYearsTeacherStatus && personAttributesUpdated ? RouteToProfessionalStatusUpdatedEventChanges.PersonEytsDate : 0) |
            (professionalStatusType is ProfessionalStatusType.EarlyYearsProfessionalStatus && personAttributesUpdated ? RouteToProfessionalStatusUpdatedEventChanges.PersonHasEyps : 0) |
            (professionalStatusType is ProfessionalStatusType.PartialQualifiedTeacherStatus && personAttributesUpdated ? RouteToProfessionalStatusUpdatedEventChanges.PersonPqtsDate : 0) |
            (newInduction.Status != oldInduction.Status ? RouteToProfessionalStatusUpdatedEventChanges.PersonInductionStatus : 0) |
            (newInduction.StatusWithoutExemption != oldInduction.StatusWithoutExemption ? RouteToProfessionalStatusUpdatedEventChanges.PersonInductionStatusWithoutExemption : 0) |
            (qtlsStatusUpdated ? RouteToProfessionalStatusUpdatedEventChanges.PersonQtlsStatus : 0);

        if (changes == RouteToProfessionalStatusUpdatedEventChanges.None)
        {
            return changes;
        }

        route.UpdatedOn = now;

        var @event = new RouteToProfessionalStatusUpdatedEvent()
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = now,
            PersonId = route.PersonId,
            RaisedBy = options.UpdatedBy,
            RouteToProfessionalStatus = EventModels.RouteToProfessionalStatus.FromModel(route),
            OldRouteToProfessionalStatus = oldEventModel,
            ChangeReason = options.ChangeReason,
            ChangeReasonDetail = options.ChangeReasonDetail,
            EvidenceFile = options.EvidenceFile,
            PersonAttributes = EventModels.ProfessionalStatusPersonAttributes.FromModel(person),
            OldPersonAttributes = oldPersonAttributes,
            Changes = changes,
            Induction = newInduction,
            OldInduction = oldInduction,
            AdditionalInformation = options.AdditionalInformation
        };

        dbContext.AddEventWithoutBroadcast(@event);
        await dbContext.SaveChangesAsync();

        return changes;
    }

    public async Task DeleteRouteToProfessionalStatusAsync(DeleteRouteToProfessionalStatusOptions options)
    {
        var route = await GetRouteAsync(options.QualificationId);

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
            route.RefreshExemptFromInductionDueToQtsDate();
            person.RefreshInductionStatusForQtsProfessionalStatusChanged(now, allRouteTypes);
        }
        var newInduction = EventModels.Induction.FromModel(person);

        var personAttributesUpdated = person.RefreshProfessionalStatusAttributes(professionalStatusType, allRouteTypes);
        var qtlsStatusUpdated = route.RouteToProfessionalStatusTypeId == RouteToProfessionalStatusType.QtlsAndSetMembershipId && person.RefreshQtlsStatus();

        var changes = RouteToProfessionalStatusDeletedEventChanges.None |
            (professionalStatusType is ProfessionalStatusType.QualifiedTeacherStatus && personAttributesUpdated
                ? RouteToProfessionalStatusDeletedEventChanges.PersonQtsDate
                : 0) |
            (professionalStatusType is ProfessionalStatusType.EarlyYearsTeacherStatus && personAttributesUpdated
                ? RouteToProfessionalStatusDeletedEventChanges.PersonEytsDate
                : 0) |
            (professionalStatusType is ProfessionalStatusType.EarlyYearsProfessionalStatus && personAttributesUpdated
                ? RouteToProfessionalStatusDeletedEventChanges.PersonHasEyps
                : 0) |
            (professionalStatusType is ProfessionalStatusType.PartialQualifiedTeacherStatus && personAttributesUpdated
                ? RouteToProfessionalStatusDeletedEventChanges.PersonPqtsDate
                : 0) |
            (newInduction.Status != oldInduction.Status
                ? RouteToProfessionalStatusDeletedEventChanges.PersonInductionStatus
                : 0) |
            (newInduction.StatusWithoutExemption != oldInduction.StatusWithoutExemption
                ? RouteToProfessionalStatusDeletedEventChanges.PersonInductionStatusWithoutExemption
                : 0) |
            (qtlsStatusUpdated ? RouteToProfessionalStatusDeletedEventChanges.PersonQtlsStatus : 0);

        var @event = new RouteToProfessionalStatusDeletedEvent()
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = now,
            RaisedBy = options.DeletedBy,
            PersonId = route.PersonId,
            RouteToProfessionalStatus = EventModels.RouteToProfessionalStatus.FromModel(route),
            PersonAttributes = EventModels.ProfessionalStatusPersonAttributes.FromModel(person),
            OldPersonAttributes = oldPersonAttributes,
            DeletionReason = options.DeletionReason,
            DeletionReasonDetail = options.DeletionReasonDetail,
            EvidenceFile = options.EvidenceFile,
            Changes = changes,
            Induction = newInduction,
            OldInduction = oldInduction,
            AdditionalInformation = options.AdditionalInformation
        };

        dbContext.AddEventWithoutBroadcast(@event);
        await dbContext.SaveChangesAsync();
    }

    private async Task<RouteToProfessionalStatus> GetRouteAsync(Guid qualificationId) =>
        await dbContext.RouteToProfessionalStatuses
            .Include(r => r.Person)
            .ThenInclude(p => p!.Qualifications)
            .SingleOrDefaultAsync(r => r.QualificationId == qualificationId)
        ?? throw new NotFoundException(qualificationId, nameof(RouteToProfessionalStatus));
}
