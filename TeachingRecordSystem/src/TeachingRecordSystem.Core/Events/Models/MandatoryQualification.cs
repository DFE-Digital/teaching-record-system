namespace TeachingRecordSystem.Core.Events.Models;

public record MandatoryQualification
{
    public required Guid QualificationId { get; init; }
    public required MandatoryQualificationSpecialism? Specialism { get; init; }
    public required MandatoryQualificationStatus? Status { get; init; }
    public required DateOnly? StartDate { get; init; }
    public required DateOnly? EndDate { get; init; }
}
