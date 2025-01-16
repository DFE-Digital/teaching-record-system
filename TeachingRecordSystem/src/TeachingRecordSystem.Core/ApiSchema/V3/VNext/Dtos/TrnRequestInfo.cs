using TeachingRecordSystem.Core.ApiSchema.V3.V20240606.Dtos;

namespace TeachingRecordSystem.Core.ApiSchema.V3.VNext.Dtos;

public record TrnRequestInfo
{
    public required string RequestId { get; init; }
    public required TrnRequestStatus Status { get; init; }
    public required string? Trn { get; init; }
}
