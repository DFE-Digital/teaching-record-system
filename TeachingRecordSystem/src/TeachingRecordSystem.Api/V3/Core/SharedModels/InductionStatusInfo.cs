namespace TeachingRecordSystem.Api.V3.Core.SharedModels;

public record InductionStatusInfo
{
    public required InductionStatus Status { get; init; }
    public required string StatusDescription { get; init; }
}
