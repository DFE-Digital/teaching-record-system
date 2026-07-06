namespace TeachingRecordSystem.Core.Services.MandatoryQualifications;

public record CreateMandatoryQualificationOptions
{
    public required Guid PersonId { get; init; }
    public required Guid ProviderId { get; init; }
    public required MandatoryQualificationSpecialism Specialism { get; init; }
    public required MandatoryQualificationStatus Status { get; init; }
    public required DateOnly StartDate { get; init; }
    public required DateOnly? EndDate { get; init; }
}
