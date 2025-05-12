using System.Diagnostics;

namespace TeachingRecordSystem.Core.DataStore.Postgres.Models;

public class ProfessionalStatus : Qualification
{
    public const int SourceApplicationReferenceMaxLength = 200;

    public ProfessionalStatus()
    {
        QualificationType = QualificationType.ProfessionalStatus;
    }

    public required Guid RouteToProfessionalStatusId { get; set; }
    public Guid? SourceApplicationUserId { get; init; }
    public string? SourceApplicationReference { get; init; }
    public RouteToProfessionalStatus? RouteToProfessionalStatus { get; }
    public required ProfessionalStatusStatus Status { get; set; }
    public DateOnly? AwardedDate { get; set; }
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
    public required Guid? DegreeTypeId { get; set; }
    public DegreeType? DegreeType { get; }
    public string? DqtTeacherStatusName { get; init; }
    public string? DqtTeacherStatusValue { get; init; }
    public string? DqtEarlyYearsStatusName { get; init; }
    public string? DqtEarlyYearsStatusValue { get; init; }
    public Guid? DqtInitialTeacherTrainingId { get; init; }
    public Guid? DqtQtsRegistrationId { get; init; }

    public static ProfessionalStatus Create(
        Person person,
        IReadOnlyCollection<RouteToProfessionalStatus> allRoutes,
        Guid routeToProfessionalStatusId,
        ProfessionalStatusStatus status,
        DateOnly? awardedDate,
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
        out ProfessionalStatusCreatedEvent @event)
    {
        Debug.Assert(person.Qualifications is not null);

        var qualificationId = Guid.NewGuid();

        var professionalStatus = new ProfessionalStatus()
        {
            QualificationId = qualificationId,
            CreatedOn = now,
            UpdatedOn = now,
            PersonId = person.PersonId,
            RouteToProfessionalStatusId = routeToProfessionalStatusId,
            Status = status,
            DegreeTypeId = degreeTypeId,
            ExemptFromInduction = isExemptFromInduction,
            TrainingStartDate = trainingStartDate,
            TrainingEndDate = trainingEndDate,
            TrainingAgeSpecialismRangeFrom = trainingAgeSpecialismRangeFrom,
            TrainingAgeSpecialismRangeTo = trainingAgeSpecialismRangeTo,
            TrainingAgeSpecialismType = trainingAgeSpecialismType,
            TrainingCountryId = trainingCountryId,
            TrainingProviderId = trainingProviderId,
            TrainingSubjectIds = trainingSubjectIds ?? [],
            AwardedDate = awardedDate
        };

        var oldPersonAttributes = EventModels.ProfessionalStatusPersonAttributes.FromModel(person);

        var route = allRoutes.Single(r => r.RouteToProfessionalStatusId == routeToProfessionalStatusId);
        var professionalStatusType = route.ProfessionalStatusType;
        var allProfessionalStatuses = person.Qualifications.OfType<ProfessionalStatus>().Append(professionalStatus);
        var personAttributesUpdated = person.RefreshProfessionalStatusAttributes(professionalStatusType, allRoutes, allProfessionalStatuses);

        var changes = ProfessionalStatusCreatedEventChanges.None |
            (professionalStatusType is ProfessionalStatusType.QualifiedTeacherStatus && personAttributesUpdated
                ? ProfessionalStatusCreatedEventChanges.PersonQtsDate
                : 0) |
            (professionalStatusType is ProfessionalStatusType.EarlyYearsTeacherStatus && personAttributesUpdated
                ? ProfessionalStatusCreatedEventChanges.PersonEytsDate
                : 0) |
            (professionalStatusType is ProfessionalStatusType.EarlyYearsProfessionalStatus && personAttributesUpdated
                ? ProfessionalStatusCreatedEventChanges.PersonHasEyps
                : 0) |
            (professionalStatusType is ProfessionalStatusType.PartialQualifiedTeacherStatus && personAttributesUpdated
                ? ProfessionalStatusCreatedEventChanges.PersonPqtsDate
                : 0);

        @event = new ProfessionalStatusCreatedEvent
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = now,
            PersonId = person.PersonId,
            RaisedBy = createdBy,
            ProfessionalStatus = EventModels.ProfessionalStatus.FromModel(professionalStatus),
            PersonAttributes = EventModels.ProfessionalStatusPersonAttributes.FromModel(person),
            OldPersonAttributes = oldPersonAttributes,
            Changes = changes
        };

        return professionalStatus;
    }

    public void Update(
        IReadOnlyCollection<RouteToProfessionalStatus> allRoutes,
        Action<ProfessionalStatus> updateAction,
        string? changeReason,
        string? changeReasonDetail,
        EventModels.File? evidenceFile,
        EventModels.RaisedByUserInfo updatedBy,
        DateTime now,
        out ProfessionalStatusUpdatedEvent? @event)
    {
        Debug.Assert(Person is not null);
        Debug.Assert(Person.Qualifications is not null);

        var oldEventModel = EventModels.ProfessionalStatus.FromModel(this);
        var oldPersonAttributes = EventModels.ProfessionalStatusPersonAttributes.FromModel(Person);
        var oldRoute = allRoutes.Single(r => r.RouteToProfessionalStatusId == RouteToProfessionalStatusId);
        var oldProfessionalStatusType = oldRoute.ProfessionalStatusType;

        updateAction(this);

        var route = allRoutes.Single(r => r.RouteToProfessionalStatusId == RouteToProfessionalStatusId);
        var professionalStatusType = route.ProfessionalStatusType;

        if (professionalStatusType != oldProfessionalStatusType)
        {
            throw new NotSupportedException($"Cannot change the {nameof(ProfessionalStatusType)} for an existing {nameof(ProfessionalStatus)}.");
        }

        var personAttributesUpdated = Person.RefreshProfessionalStatusAttributes(professionalStatusType, allRoutes);

        var changes = ProfessionalStatusUpdatedEventChanges.None |
            (RouteToProfessionalStatus!.RouteToProfessionalStatusId != oldEventModel.RouteToProfessionalStatusId ? ProfessionalStatusUpdatedEventChanges.Route : ProfessionalStatusUpdatedEventChanges.None) |
            (Status != oldEventModel.Status ? ProfessionalStatusUpdatedEventChanges.Status : ProfessionalStatusUpdatedEventChanges.None) |
            (AwardedDate != oldEventModel.AwardedDate ? ProfessionalStatusUpdatedEventChanges.AwardedDate : ProfessionalStatusUpdatedEventChanges.None) |
            (TrainingStartDate != oldEventModel.TrainingStartDate ? ProfessionalStatusUpdatedEventChanges.StartDate : ProfessionalStatusUpdatedEventChanges.None) |
            (TrainingEndDate != oldEventModel.TrainingEndDate ? ProfessionalStatusUpdatedEventChanges.EndDate : ProfessionalStatusUpdatedEventChanges.None) |
            (TrainingSubjectIds != oldEventModel.TrainingSubjectIds ? ProfessionalStatusUpdatedEventChanges.TrainingSubjectIds : ProfessionalStatusUpdatedEventChanges.None) |
            (TrainingAgeSpecialismType != oldEventModel.TrainingAgeSpecialismType ? ProfessionalStatusUpdatedEventChanges.TrainingAgeSpecialismType : ProfessionalStatusUpdatedEventChanges.None) |
            (TrainingAgeSpecialismRangeFrom != oldEventModel.TrainingAgeSpecialismRangeFrom ? ProfessionalStatusUpdatedEventChanges.TrainingAgeSpecialismRangeFrom : ProfessionalStatusUpdatedEventChanges.None) |
            (TrainingAgeSpecialismRangeTo != oldEventModel.TrainingAgeSpecialismRangeTo ? ProfessionalStatusUpdatedEventChanges.TrainingAgeSpecialismRangeTo : ProfessionalStatusUpdatedEventChanges.None) |
            (TrainingCountryId != oldEventModel.TrainingCountryId ? ProfessionalStatusUpdatedEventChanges.TrainingCountry : ProfessionalStatusUpdatedEventChanges.None) |
            (TrainingProviderId != oldEventModel.TrainingProviderId ? ProfessionalStatusUpdatedEventChanges.TrainingProvider : ProfessionalStatusUpdatedEventChanges.None) |
            (ExemptFromInduction != oldEventModel.ExemptFromInduction ? ProfessionalStatusUpdatedEventChanges.ExemptFromInduction : ProfessionalStatusUpdatedEventChanges.None) |
            (DegreeTypeId != oldEventModel.DegreeTypeId ? ProfessionalStatusUpdatedEventChanges.DegreeType : ProfessionalStatusUpdatedEventChanges.None) |
            (professionalStatusType is ProfessionalStatusType.QualifiedTeacherStatus && personAttributesUpdated ? ProfessionalStatusUpdatedEventChanges.PersonQtsDate : 0) |
            (professionalStatusType is ProfessionalStatusType.EarlyYearsTeacherStatus && personAttributesUpdated ? ProfessionalStatusUpdatedEventChanges.PersonEytsDate : 0) |
            (professionalStatusType is ProfessionalStatusType.EarlyYearsProfessionalStatus && personAttributesUpdated ? ProfessionalStatusUpdatedEventChanges.PersonHasEyps : 0) |
            (professionalStatusType is ProfessionalStatusType.PartialQualifiedTeacherStatus && personAttributesUpdated ? ProfessionalStatusUpdatedEventChanges.PersonPqtsDate : 0);

        if (changes == ProfessionalStatusUpdatedEventChanges.None)
        {
            @event = null;
            return;
        }

        UpdatedOn = now;

        @event = new ProfessionalStatusUpdatedEvent
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = now,
            PersonId = PersonId,
            RaisedBy = updatedBy,
            ProfessionalStatus = EventModels.ProfessionalStatus.FromModel(this),
            OldProfessionalStatus = oldEventModel,
            ChangeReason = changeReason,
            ChangeReasonDetail = changeReasonDetail,
            EvidenceFile = evidenceFile,
            PersonAttributes = EventModels.ProfessionalStatusPersonAttributes.FromModel(Person),
            OldPersonAttributes = oldPersonAttributes,
            Changes = changes,
        };
    }

    public void Delete(
        string? deletionReason,
        string? deletionReasonDetail,
        EventModels.File? evidenceFile,
        EventModels.RaisedByUserInfo deletedBy,
        DateTime now,
        out ProfessionalStatusDeletedEvent @event)
    {
        if (DeletedOn is not null)
        {
            throw new InvalidOperationException("Professional status is already deleted.");
        }

        DeletedOn = now;
        UpdatedOn = now;

        @event = new ProfessionalStatusDeletedEvent()
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = now,
            RaisedBy = deletedBy,
            PersonId = PersonId,
            ProfessionalStatus = EventModels.ProfessionalStatus.FromModel(this),
            DeletionReason = deletionReason,
            DeletionReasonDetail = deletionReasonDetail,
            EvidenceFile = evidenceFile,
        };
    }

}
