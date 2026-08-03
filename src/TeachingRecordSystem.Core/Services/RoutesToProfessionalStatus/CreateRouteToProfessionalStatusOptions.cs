namespace TeachingRecordSystem.Core.Services.RoutesToProfessionalStatus;

public record CreateRouteToProfessionalStatusOptions
{
    public required Guid PersonId { get; init; }
    public required Guid RouteToProfessionalStatusTypeId { get; init; }
    public required RouteToProfessionalStatusStatus Status { get; init; }
    public required EventModels.RaisedByUserInfo CreatedBy { get; init; }
    public Guid? SourceApplicationUserId { get; init; }
    public string? SourceApplicationReference { get; init; }
    public DateOnly? HoldsFrom { get; init; }
    public DateOnly? TrainingStartDate { get; init; }
    public DateOnly? TrainingEndDate { get; init; }
    public Guid[]? TrainingSubjectIds { get; init; }
    public TrainingAgeSpecialismType? TrainingAgeSpecialismType { get; init; }
    public int? TrainingAgeSpecialismRangeFrom { get; init; }
    public int? TrainingAgeSpecialismRangeTo { get; init; }
    public string? TrainingCountryId { get; init; }
    public Guid? TrainingProviderId { get; init; }
    public Guid? DegreeTypeId { get; init; }
    public bool? IsExemptFromInduction { get; init; }
    public string? ChangeReason { get; init; }
    public string? ChangeReasonDetail { get; init; }
    public EventModels.File? EvidenceFile { get; init; }
    public string? AdditionalInformation { get; init; }
}
