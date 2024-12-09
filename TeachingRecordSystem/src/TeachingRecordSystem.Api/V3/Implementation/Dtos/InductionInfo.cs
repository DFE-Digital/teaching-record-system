namespace TeachingRecordSystem.Api.V3.Implementation.Dtos;

public record InductionInfo
{
    public required InductionStatus Status { get; init; }
    public required DateOnly? StartDate { get; init; }
    public required DateOnly? CompletedDate { get; init; }
}
