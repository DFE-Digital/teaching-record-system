namespace TeachingRecordSystem.Api.V3.Implementation.Dtos;

public record SanctionInfo
{
    public required string Code { get; init; }
    public required DateOnly? StartDate { get; init; }
}
