namespace TeachingRecordSystem.Core.ApiSchema.V3.V20250327.Dtos;

public record QtsInfo
{
    public required DateOnly Awarded { get; init; }
    public required string StatusDescription { get; init; }
    public required int AwardedOrApprovedCount { get; init; }
}
