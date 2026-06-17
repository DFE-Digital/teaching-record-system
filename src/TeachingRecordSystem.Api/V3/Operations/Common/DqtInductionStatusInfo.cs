namespace TeachingRecordSystem.Api.V3.Operations.Common;

public record DqtInductionStatusInfo
{
    public required DqtInductionStatus Status { get; init; }
    public required string StatusDescription { get; init; }
}
