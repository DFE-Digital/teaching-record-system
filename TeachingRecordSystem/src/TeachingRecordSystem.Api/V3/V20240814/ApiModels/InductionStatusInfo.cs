using TeachingRecordSystem.Api.V3.V20240101.ApiModels;

namespace TeachingRecordSystem.Api.V3.V20240814.ApiModels;

[AutoMap(typeof(Core.SharedModels.InductionStatusInfo))]
public record InductionStatusInfo
{
    public required InductionStatus Status { get; init; }
    public required string StatusDescription { get; init; }
}
