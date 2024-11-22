namespace TeachingRecordSystem.Core.ApiSchema.V3.V20240814.Dtos;

public record InductionStatusInfo
{
    public required V20240101.Dtos.InductionStatus Status { get; init; }
    public required string StatusDescription { get; init; }
}
