namespace TeachingRecordSystem.Core.ApiSchema.V3.V20240814.Dtos;

public record DqtInductionStatusInfo
{
    public required V20240101.Dtos.DqtInductionStatus Status { get; init; }
    public required string StatusDescription { get; init; }
}
