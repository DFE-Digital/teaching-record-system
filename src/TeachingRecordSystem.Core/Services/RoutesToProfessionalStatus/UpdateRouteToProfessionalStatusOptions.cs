using Optional;

namespace TeachingRecordSystem.Core.Services.RoutesToProfessionalStatus;

public record UpdateRouteToProfessionalStatusOptions
{
    public required Guid QualificationId { get; init; }
    public required EventModels.RaisedByUserInfo UpdatedBy { get; init; }
    public Option<Guid> RouteToProfessionalStatusTypeId { get; init; }
    public Option<RouteToProfessionalStatusStatus> Status { get; init; }
    public Option<DateOnly?> HoldsFrom { get; init; }
    public Option<DateOnly?> TrainingStartDate { get; init; }
    public Option<DateOnly?> TrainingEndDate { get; init; }
    public Option<Guid[]> TrainingSubjectIds { get; init; }
    public Option<TrainingAgeSpecialismType?> TrainingAgeSpecialismType { get; init; }
    public Option<int?> TrainingAgeSpecialismRangeFrom { get; init; }
    public Option<int?> TrainingAgeSpecialismRangeTo { get; init; }
    public Option<string?> TrainingCountryId { get; init; }
    public Option<Guid?> TrainingProviderId { get; init; }
    public Option<Guid?> DegreeTypeId { get; init; }
    public Option<bool?> ExemptFromInduction { get; init; }
    public string? ChangeReason { get; init; }
    public string? ChangeReasonDetail { get; init; }
    public EventModels.File? EvidenceFile { get; init; }
    public string? AdditionalInformation { get; init; }
}
