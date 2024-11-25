namespace TeachingRecordSystem.Api.V3.Implementation.Dtos;

public record InductionStatusInfo
{
    public required InductionStatus Status { get; init; }
    public required string StatusDescription { get; init; }
}
