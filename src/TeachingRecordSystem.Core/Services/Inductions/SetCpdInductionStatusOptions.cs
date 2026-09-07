namespace TeachingRecordSystem.Core.Services.Inductions;

public record SetCpdInductionStatusOptions
{
    public required Guid PersonId { get; init; }
    public required InductionStatus Status { get; init; }
    public required DateOnly? StartDate { get; init; }
    public required DateOnly? CompletedDate { get; init; }
    public required DateTime CpdModifiedOn { get; init; }
}
