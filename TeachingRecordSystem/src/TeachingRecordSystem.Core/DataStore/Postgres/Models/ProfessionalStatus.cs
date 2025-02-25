namespace TeachingRecordSystem.Core.DataStore.Postgres.Models;

public class ProfessionalStatus : Qualification
{
    public const int SourceApplicationReferenceMaxLength = 200;

    public ProfessionalStatus()
    {
        QualificationType = QualificationType.ProfessionalStatus;
    }

    public required ProfessionalStatusType ProfessionalStatusType { get; set; }
    public required Guid RouteToProfessionalStatusId { get; init; }
    public Guid? SourceApplicationUserId { get; init; }
    public string? SourceApplicationReference { get; init; }
    public RouteToProfessionalStatus Route { get; } = null!;
    public required ProfessionalStatusStatus Status { get; set; }
    public DateOnly? AwardedDate { get; set; }
    public required DateOnly? TrainingStartDate { get; set; }
    public required DateOnly? TrainingEndDate { get; set; }
    public required Guid[] TrainingSubjectIds { get; set; } = [];
    public required TrainingAgeSpecialismType? TrainingAgeSpecialismType { get; set; }
    public required int? TrainingAgeSpecialismRangeFrom { get; set; }
    public required int? TrainingAgeSpecialismRangeTo { get; set; }
    public required string? TrainingCountryId { get; init; }
    public Country? TrainingCountry { get; }
    public required Guid? TrainingProviderId { get; set; }
    public TrainingProvider? TrainingProvider { get; }
    public required Guid? InductionExemptionReasonId { get; set; }
    public InductionExemptionReason? InductionExemptionReason { get; }
    public string? DqtTeacherStatusName { get; init; }
    public string? DqtTeacherStatusValue { get; init; }
    public string? DqtEarlyYearsStatusName { get; init; }
    public string? DqtEarlyYearsStatusValue { get; init; }
    public Guid? DqtInitialTeacherTrainingId { get; init; }
    public Guid? DqtQtsRegistrationId { get; init; }
}
