namespace TeachingRecordSystem.Core.Services.MandatoryQualifications;

public record DeleteMandatoryQualificationOptions
{
    public required Guid QualificationId { get; init; }
}
