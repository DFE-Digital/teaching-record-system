using System.Diagnostics;
using TeachingRecordSystem.Core.Events.Legacy;

namespace TeachingRecordSystem.Core.DataStore.Postgres.Models;

public class RouteToProfessionalStatus : Qualification
{
    public const int SourceApplicationReferenceMaxLength = 200;

    public RouteToProfessionalStatus()
    {
        QualificationType = QualificationType.RouteToProfessionalStatus;
    }

    public required Guid RouteToProfessionalStatusTypeId { get; set; }
    public Guid? SourceApplicationUserId { get; init; }
    public string? SourceApplicationReference { get; init; }
    public RouteToProfessionalStatusType? RouteToProfessionalStatusType { get; protected set; }
    public required RouteToProfessionalStatusStatus Status { get; set; }
    public DateOnly? HoldsFrom { get; set; }
    public required DateOnly? TrainingStartDate { get; set; }
    public required DateOnly? TrainingEndDate { get; set; }
    public required Guid[] TrainingSubjectIds { get; set; } = [];
    public required TrainingAgeSpecialismType? TrainingAgeSpecialismType { get; set; }
    public required int? TrainingAgeSpecialismRangeFrom { get; set; }
    public required int? TrainingAgeSpecialismRangeTo { get; set; }
    public required string? TrainingCountryId { get; set; }
    public Country? TrainingCountry { get; }
    public required Guid? TrainingProviderId { get; set; }
    public TrainingProvider? TrainingProvider { get; }
    public required bool? ExemptFromInduction { get; set; }
    public bool? ExemptFromInductionDueToQtsDate { get; set; }
    public required Guid? DegreeTypeId { get; set; }
    public DegreeType? DegreeType { get; }
    public string? DqtTeacherStatusName { get; init; }
    public string? DqtTeacherStatusValue { get; init; }
    public string? DqtEarlyYearsStatusName { get; init; }
    public string? DqtEarlyYearsStatusValue { get; init; }
    public Guid? DqtInitialTeacherTrainingId { get; init; }
    public Guid? DqtQtsRegistrationId { get; init; }
    public string? DqtAgeRangeFrom { get; init; }
    public string? DqtAgeRangeTo { get; init; }

    public static RouteToProfessionalStatus Create(
        Person person,
        IReadOnlyCollection<RouteToProfessionalStatusType> allRouteTypes,
        Guid routeToProfessionalStatusTypeId,
        Guid? sourceApplicationUserId,
        string? sourceApplicationReference,
        RouteToProfessionalStatusStatus status,
        DateOnly? holdsFrom,
        DateOnly? trainingStartDate,
        DateOnly? trainingEndDate,
        Guid[]? trainingSubjectIds,
        TrainingAgeSpecialismType? trainingAgeSpecialismType,
        int? trainingAgeSpecialismRangeFrom,
        int? trainingAgeSpecialismRangeTo,
        string? trainingCountryId,
        Guid? trainingProviderId,
        Guid? degreeTypeId,
        bool? isExemptFromInduction,
        EventModels.RaisedByUserInfo createdBy,
        DateTime now,
        string? changeReason,
        string? changeReasonDetail,
        EventModels.File? evidenceFile,
        out RouteToProfessionalStatusCreatedEvent @event)
    {
        Debug.Assert(person.Qualifications is not null);

        var routeType = allRouteTypes.Single(r => r.RouteToProfessionalStatusTypeId == routeToProfessionalStatusTypeId);
        var qualificationId = Guid.NewGuid();

        var route = new RouteToProfessionalStatus()
        {
            QualificationId = qualificationId,
            CreatedOn = now,
            UpdatedOn = now,
            PersonId = person.PersonId,
            SourceApplicationUserId = sourceApplicationUserId,
            SourceApplicationReference = sourceApplicationReference,
            RouteToProfessionalStatusTypeId = routeToProfessionalStatusTypeId,
            Status = status,
            HoldsFrom = holdsFrom,
            DegreeTypeId = degreeTypeId,
            ExemptFromInduction = isExemptFromInduction,
            TrainingStartDate = trainingStartDate,
            TrainingEndDate = trainingEndDate,
            TrainingAgeSpecialismRangeFrom = trainingAgeSpecialismRangeFrom,
            TrainingAgeSpecialismRangeTo = trainingAgeSpecialismRangeTo,
            TrainingAgeSpecialismType = trainingAgeSpecialismType,
            TrainingCountryId = trainingCountryId,
            TrainingProviderId = trainingProviderId,
            TrainingSubjectIds = trainingSubjectIds ?? []
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
        var qtlsStatusUpdated = routeToProfessionalStatusTypeId == RouteToProfessionalStatusType.QtlsAndSetMembershipId && person.RefreshQtlsStatus(allRoutes);

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

        @event = new RouteToProfessionalStatusCreatedEvent()
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = now,
            PersonId = person.PersonId,
            RaisedBy = createdBy,
            RouteToProfessionalStatus = EventModels.RouteToProfessionalStatus.FromModel(route),
            PersonAttributes = EventModels.ProfessionalStatusPersonAttributes.FromModel(person),
            ChangeReason = changeReason,
            ChangeReasonDetail = changeReasonDetail,
            EvidenceFile = evidenceFile,
            OldPersonAttributes = oldPersonAttributes,
            Changes = changes,
            Induction = newInduction,
            OldInduction = oldInduction
        };

        return route;
    }

    public void Update(
        IReadOnlyCollection<RouteToProfessionalStatusType> allRouteTypes,
        Action<RouteToProfessionalStatus> updateAction,
        string? changeReason,
        string? changeReasonDetail,
        EventModels.File? evidenceFile,
        EventModels.RaisedByUserInfo updatedBy,
        DateTime now,
        out RouteToProfessionalStatusUpdatedEvent? @event)
    {
        Debug.Assert(Person is not null);
        Debug.Assert(Person.Qualifications is not null);

        var oldEventModel = EventModels.RouteToProfessionalStatus.FromModel(this);
        var oldPersonAttributes = EventModels.ProfessionalStatusPersonAttributes.FromModel(Person);
        var oldRoute = allRouteTypes.Single(r => r.RouteToProfessionalStatusTypeId == RouteToProfessionalStatusTypeId);
        var oldProfessionalStatusType = oldRoute.ProfessionalStatusType;

        var routeType = allRouteTypes.Single(r => r.RouteToProfessionalStatusTypeId == RouteToProfessionalStatusTypeId);
        var professionalStatusType = routeType.ProfessionalStatusType;

        updateAction(this);

        if (professionalStatusType != oldProfessionalStatusType)
        {
            throw new NotSupportedException($"Cannot change the {nameof(ProfessionalStatusType)} for an existing {nameof(RouteToProfessionalStatus)}.");
        }

        var oldInduction = EventModels.Induction.FromModel(Person);
        if (professionalStatusType == ProfessionalStatusType.QualifiedTeacherStatus)
        {
            RefreshExemptFromInductionDueToQtsDate();
            Person.RefreshInductionStatusForQtsProfessionalStatusChanged(now, allRouteTypes);
        }
        var newInduction = EventModels.Induction.FromModel(Person);

        var personAttributesUpdated = Person.RefreshProfessionalStatusAttributes(professionalStatusType, allRouteTypes);
        var qtlsStatusUpdated = RouteToProfessionalStatusTypeId == RouteToProfessionalStatusType.QtlsAndSetMembershipId && Person.RefreshQtlsStatus();

        var changes = RouteToProfessionalStatusUpdatedEventChanges.None |
            (RouteToProfessionalStatusTypeId != oldEventModel.RouteToProfessionalStatusTypeId ? RouteToProfessionalStatusUpdatedEventChanges.Route : RouteToProfessionalStatusUpdatedEventChanges.None) |
            (Status != oldEventModel.Status ? RouteToProfessionalStatusUpdatedEventChanges.Status : RouteToProfessionalStatusUpdatedEventChanges.None) |
            (HoldsFrom != oldEventModel.HoldsFrom ? RouteToProfessionalStatusUpdatedEventChanges.HoldsFrom : RouteToProfessionalStatusUpdatedEventChanges.None) |
            (TrainingStartDate != oldEventModel.TrainingStartDate ? RouteToProfessionalStatusUpdatedEventChanges.StartDate : RouteToProfessionalStatusUpdatedEventChanges.None) |
            (TrainingEndDate != oldEventModel.TrainingEndDate ? RouteToProfessionalStatusUpdatedEventChanges.EndDate : RouteToProfessionalStatusUpdatedEventChanges.None) |
            ((TrainingSubjectIds.Except(oldEventModel.TrainingSubjectIds).Any() || oldEventModel.TrainingSubjectIds.Except(TrainingSubjectIds).Any()) ? RouteToProfessionalStatusUpdatedEventChanges.TrainingSubjectIds : RouteToProfessionalStatusUpdatedEventChanges.None) |
            (TrainingAgeSpecialismType != oldEventModel.TrainingAgeSpecialismType ? RouteToProfessionalStatusUpdatedEventChanges.TrainingAgeSpecialismType : RouteToProfessionalStatusUpdatedEventChanges.None) |
            (TrainingAgeSpecialismRangeFrom != oldEventModel.TrainingAgeSpecialismRangeFrom ? RouteToProfessionalStatusUpdatedEventChanges.TrainingAgeSpecialismRangeFrom : RouteToProfessionalStatusUpdatedEventChanges.None) |
            (TrainingAgeSpecialismRangeTo != oldEventModel.TrainingAgeSpecialismRangeTo ? RouteToProfessionalStatusUpdatedEventChanges.TrainingAgeSpecialismRangeTo : RouteToProfessionalStatusUpdatedEventChanges.None) |
            (TrainingCountryId != oldEventModel.TrainingCountryId ? RouteToProfessionalStatusUpdatedEventChanges.TrainingCountry : RouteToProfessionalStatusUpdatedEventChanges.None) |
            (TrainingProviderId != oldEventModel.TrainingProviderId ? RouteToProfessionalStatusUpdatedEventChanges.TrainingProvider : RouteToProfessionalStatusUpdatedEventChanges.None) |
            (ExemptFromInduction != oldEventModel.ExemptFromInduction ? RouteToProfessionalStatusUpdatedEventChanges.ExemptFromInduction : RouteToProfessionalStatusUpdatedEventChanges.None) |
            (DegreeTypeId != oldEventModel.DegreeTypeId ? RouteToProfessionalStatusUpdatedEventChanges.DegreeType : RouteToProfessionalStatusUpdatedEventChanges.None) |
            (ExemptFromInductionDueToQtsDate != oldEventModel.ExemptFromInductionDueToQtsDate ? RouteToProfessionalStatusUpdatedEventChanges.ExemptFromInductionDueToQtsDate : RouteToProfessionalStatusUpdatedEventChanges.None) |
            (professionalStatusType is ProfessionalStatusType.QualifiedTeacherStatus && personAttributesUpdated ? RouteToProfessionalStatusUpdatedEventChanges.PersonQtsDate : 0) |
            (professionalStatusType is ProfessionalStatusType.EarlyYearsTeacherStatus && personAttributesUpdated ? RouteToProfessionalStatusUpdatedEventChanges.PersonEytsDate : 0) |
            (professionalStatusType is ProfessionalStatusType.EarlyYearsProfessionalStatus && personAttributesUpdated ? RouteToProfessionalStatusUpdatedEventChanges.PersonHasEyps : 0) |
            (professionalStatusType is ProfessionalStatusType.PartialQualifiedTeacherStatus && personAttributesUpdated ? RouteToProfessionalStatusUpdatedEventChanges.PersonPqtsDate : 0) |
            (newInduction.Status != oldInduction.Status ? RouteToProfessionalStatusUpdatedEventChanges.PersonInductionStatus : 0) |
            (newInduction.StatusWithoutExemption != oldInduction.StatusWithoutExemption ? RouteToProfessionalStatusUpdatedEventChanges.PersonInductionStatusWithoutExemption : 0) |
            (qtlsStatusUpdated ? RouteToProfessionalStatusUpdatedEventChanges.PersonQtlsStatus : 0);

        if (changes == RouteToProfessionalStatusUpdatedEventChanges.None)
        {
            @event = null;
            return;
        }

        UpdatedOn = now;

        @event = new RouteToProfessionalStatusUpdatedEvent()
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = now,
            PersonId = PersonId,
            RaisedBy = updatedBy,
            RouteToProfessionalStatus = EventModels.RouteToProfessionalStatus.FromModel(this),
            OldRouteToProfessionalStatus = oldEventModel,
            ChangeReason = changeReason,
            ChangeReasonDetail = changeReasonDetail,
            EvidenceFile = evidenceFile,
            PersonAttributes = EventModels.ProfessionalStatusPersonAttributes.FromModel(Person),
            OldPersonAttributes = oldPersonAttributes,
            Changes = changes,
            Induction = newInduction,
            OldInduction = oldInduction
        };
    }

    public void Delete(
        IReadOnlyCollection<RouteToProfessionalStatusType> allRouteTypes,
        string? deletionReason,
        string? deletionReasonDetail,
        EventModels.File? evidenceFile,
        EventModels.RaisedByUserInfo deletedBy,
        DateTime now,
        out RouteToProfessionalStatusDeletedEvent @event)
    {
        if (DeletedOn is not null)
        {
            throw new InvalidOperationException("Professional status is already deleted.");
        }
        if (Person is null)
        {
            throw new InvalidOperationException("Professional status is not linked to a person and cannot be deleted");
        }

        DeletedOn = now;
        UpdatedOn = now;

        var oldPersonAttributes = EventModels.ProfessionalStatusPersonAttributes.FromModel(Person);

        var route = allRouteTypes.Single(r => r.RouteToProfessionalStatusTypeId == RouteToProfessionalStatusTypeId);
        var professionalStatusType = route.ProfessionalStatusType;

        var oldInduction = EventModels.Induction.FromModel(Person);
        if (professionalStatusType == ProfessionalStatusType.QualifiedTeacherStatus)
        {
            RefreshExemptFromInductionDueToQtsDate();
            Person.RefreshInductionStatusForQtsProfessionalStatusChanged(now, allRouteTypes);
        }
        var newInduction = EventModels.Induction.FromModel(Person);

        var personAttributesUpdated = Person.RefreshProfessionalStatusAttributes(professionalStatusType, allRouteTypes);
        var qtlsStatusUpdated = RouteToProfessionalStatusTypeId == RouteToProfessionalStatusType.QtlsAndSetMembershipId && Person.RefreshQtlsStatus();

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

        @event = new RouteToProfessionalStatusDeletedEvent()
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = now,
            RaisedBy = deletedBy,
            PersonId = PersonId,
            RouteToProfessionalStatus = EventModels.RouteToProfessionalStatus.FromModel(this),
            PersonAttributes = EventModels.ProfessionalStatusPersonAttributes.FromModel(Person),
            OldPersonAttributes = oldPersonAttributes,
            DeletionReason = deletionReason,
            DeletionReasonDetail = deletionReasonDetail,
            EvidenceFile = evidenceFile,
            Changes = changes,
            Induction = newInduction,
            OldInduction = oldInduction
        };
    }

    private void RefreshExemptFromInductionDueToQtsDate()
    {
        if (HoldsFrom is null)
        {
            ExemptFromInductionDueToQtsDate = null;
            return;
        }

        ExemptFromInductionDueToQtsDate = HoldsFrom < new DateOnly(2000, 5, 7);
    }
}
