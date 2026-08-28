namespace TeachingRecordSystem.Core.Services.Inductions;

public record SetInductionStatusOptions
{
    public required Guid PersonId { get; init; }
    public required InductionStatus Status { get; init; }
    public required DateOnly? StartDate { get; init; }
    public required DateOnly? CompletedDate { get; init; }
    public required Guid[] ExemptionReasonIds { get; init; }
}
