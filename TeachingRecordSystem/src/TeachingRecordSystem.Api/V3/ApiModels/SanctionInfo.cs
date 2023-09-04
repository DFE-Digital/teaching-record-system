namespace TeachingRecordSystem.Api.V3.Responses;

public record SanctionInfo
{
    public required string Code { get; init; }
    public required DateOnly? StartDate { get; init; }
}
