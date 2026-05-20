namespace TeachingRecordSystem.Core.ApiSchema.V3.V20240101.Dtos;

public record SanctionInfo
{
    public required string Code { get; init; }
    public required DateOnly? StartDate { get; init; }
}
