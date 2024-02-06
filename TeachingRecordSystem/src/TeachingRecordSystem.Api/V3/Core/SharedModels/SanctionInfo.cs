namespace TeachingRecordSystem.Api.V3.Core.SharedModels;

public record SanctionInfo
{
    public required string Code { get; init; }
    public required DateOnly? StartDate { get; init; }
}
