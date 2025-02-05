namespace TeachingRecordSystem.Core.ApiSchema.V3.V20250203.Dtos;

public record InductionInfo
{
    public required InductionStatus Status { get; init; }
    public required DateOnly? StartDate { get; init; }
    public required DateOnly? CompletedDate { get; init; }
}
