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
}
