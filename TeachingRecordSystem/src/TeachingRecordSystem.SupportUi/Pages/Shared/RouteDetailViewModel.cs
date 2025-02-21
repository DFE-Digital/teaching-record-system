namespace TeachingRecordSystem.SupportUi.Pages.Shared;

public class RouteDetailViewModel
{
    public QualificationType? QualificationType { get; set; }
    public Guid RouteToProfessionalStatusId { get; set; }
    public ProfessionalStatusStatus Status { get; set; }
    public DateOnly? AwardedDate { get; set; }
    public DateOnly? TrainingStartDate { get; set; }
    public DateOnly? TrainingEndDate { get; set; }
    public Guid[]? TrainingSubjectIds { get; set; }
    public TrainingAgeSpecialismType? TrainingAgeSpecialismType { get; set; }
    public int? TrainingAgeSpecialismRangeFrom { get; set; }
    public int? TrainingAgeSpecialismRangeTo { get; set; }
    public string? TrainingAgeSpecialismRange => TrainingAgeSpecialismRangeFrom is not null ? $"From {TrainingAgeSpecialismRangeFrom} to {TrainingAgeSpecialismRangeTo}" : null;
    public string? TrainingCountryId { get; set; }
    public Guid? TrainingProviderId { get; set; }
    public Guid? InductionExemptionReasonId { get; set; }
    public string? ExemptionReason { get; set; }
    public string? TrainingProvider { get; set; }
    public string? TrainingCountry { get; set; }
    public string[]? TrainingSubjects { get; set; }
    public string? RouteToProfessionalStatusName { get; set; }
}
