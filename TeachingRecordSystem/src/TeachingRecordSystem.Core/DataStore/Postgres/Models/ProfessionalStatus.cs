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
    public RouteToProfessionalStatus? Route { get; }
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
        Guid PersonId,
        Guid RouteToProfessionalStatusId,
        ProfessionalStatusStatus Status,
        DateOnly? AwardedDate,
        DateOnly? TrainingStartDate,
        DateOnly? TrainingEndDate,
        Guid[]? TrainingSubjectIds,
        TrainingAgeSpecialismType? TrainingAgeSpecialismType,
        int? TrainingAgeSpecialismRangeFrom,
        int? TrainingAgeSpecialismRangeTo,
        string? TrainingCountryId,
        Guid? TrainingProviderId,
        Guid? DegreeTypeId,
        bool? IsExemptFromInduction,
        EventModels.RaisedByUserInfo createdBy,
        DateTime now,
        out ProfessionalStatusCreatedEvent? @event)
    {
        var qualificationId = Guid.NewGuid();

        var professionalStatus = new ProfessionalStatus()
        {
            QualificationId = qualificationId,
            CreatedOn = now,
            UpdatedOn = now,
            PersonId = PersonId,
            RouteToProfessionalStatusId = RouteToProfessionalStatusId,
            Status = Status,
            DegreeTypeId = DegreeTypeId,
            ExemptFromInduction = IsExemptFromInduction,
            TrainingStartDate = TrainingStartDate,
            TrainingEndDate = TrainingEndDate,
            TrainingAgeSpecialismRangeFrom = TrainingAgeSpecialismRangeFrom,
            TrainingAgeSpecialismRangeTo = TrainingAgeSpecialismRangeTo,
            TrainingAgeSpecialismType = TrainingAgeSpecialismType,
            TrainingCountryId = TrainingCountryId,
            TrainingProviderId = TrainingProviderId,
            TrainingSubjectIds = TrainingSubjectIds ?? [],
            AwardedDate = AwardedDate
        };
        @event = new ProfessionalStatusCreatedEvent()
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = now,
            PersonId = PersonId,
            RaisedBy = createdBy,
            ProfessionalStatus = EventModels.ProfessionalStatus.FromModel(professionalStatus)
        };

        return professionalStatus;
    }

    public void Update(
        Action<ProfessionalStatus> updateAction,
        string? changeReason,
        string? changeReasonDetail,
        EventModels.File? evidenceFile,
        EventModels.RaisedByUserInfo updatedBy,
        DateTime now,
        out ProfessionalStatusUpdatedEvent? @event)
    {
        var oldEventModel = EventModels.ProfessionalStatus.FromModel(this);

        updateAction(this);

        var changes = ProfessionalStatusUpdatedEventChanges.None |
            (Route!.RouteToProfessionalStatusId != oldEventModel.RouteToProfessionalStatusId ? ProfessionalStatusUpdatedEventChanges.Route : ProfessionalStatusUpdatedEventChanges.None) |
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
            (DegreeTypeId != oldEventModel.DegreeTypeId ? ProfessionalStatusUpdatedEventChanges.DegreeType : ProfessionalStatusUpdatedEventChanges.None);

        if (changes == ProfessionalStatusUpdatedEventChanges.None)
        {
            @event = null;
            return;
        }

        UpdatedOn = now;

        @event = new ProfessionalStatusUpdatedEvent()
        {
            EventId = Guid.NewGuid(),
            ChangeReason = changeReason,
            ChangeReasonDetail = changeReasonDetail,
            Changes = changes,
            CreatedUtc = now,
            EvidenceFile = evidenceFile,
            OldProfessionalStatus = oldEventModel,
            PersonId = PersonId,
            RaisedBy = updatedBy,
            ProfessionalStatus = EventModels.ProfessionalStatus.FromModel(this)
        };
    }
}
