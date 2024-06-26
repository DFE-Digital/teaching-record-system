namespace TeachingRecordSystem.Api.V3.V20240101.ApiModels;

[AutoMap(typeof(Core.SharedModels.SanctionInfo))]
public record SanctionInfo
{
    public required string Code { get; init; }
    public required DateOnly? StartDate { get; init; }
}
