namespace TeachingRecordSystem.Api.V3.Implementation.Dtos;

public record DqtInductionStatusInfo
{
    public required DqtInductionStatus Status { get; init; }
    public required string StatusDescription { get; init; }
}
