namespace TeachingRecordSystem.Core.DataStore.Postgres.Models;

public class ProfessionalStatus : Qualification
{
    public const int SourceApplicationReferenceMaxLength = 200;

    public new required QualificationType QualificationType { get; set; }
    public required Guid RouteToProfessionalStatusId { get; init; }
    public Guid? SourceApplicationUserId { get; init; }
    public string? SourceApplicationReference { get; init; }
    public RouteToProfessionalStatus Route { get; } = null!;
    public required ProfessionalStatusStatus Status { get; set; }
    public DateOnly? AwardDate { get; set; }
    public required DateOnly? StartDate { get; set; }
    public required DateOnly? EndDate { get; set; }
    public required Guid[] TrainingSubjectIds { get; set; } = [];
    public required TrainingAge? TrainingAge { get; set; }
    public required int? AgeRangeFrom { get; set; }
    public required int? AgeRangeTo { get; set; }
    public required string? CountryId { get; init; }
    public Country? Country { get; }
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
