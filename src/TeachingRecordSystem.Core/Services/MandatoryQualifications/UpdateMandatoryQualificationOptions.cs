using Optional;

namespace TeachingRecordSystem.Core.Services.MandatoryQualifications;

public record UpdateMandatoryQualificationOptions
{
    public required Guid QualificationId { get; init; }
    public Option<Guid?> ProviderId { get; init; }
    public Option<MandatoryQualificationSpecialism?> Specialism { get; init; }
    public Option<MandatoryQualificationStatus?> Status { get; init; }
    public Option<DateOnly?> StartDate { get; init; }
    public Option<DateOnly?> EndDate { get; init; }
}
