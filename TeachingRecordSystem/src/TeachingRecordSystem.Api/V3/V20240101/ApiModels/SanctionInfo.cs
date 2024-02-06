namespace TeachingRecordSystem.Api.V3.V20240101.ApiModels;

public record SanctionInfo
{
    public required string Code { get; init; }
    public required DateOnly? StartDate { get; init; }
}
