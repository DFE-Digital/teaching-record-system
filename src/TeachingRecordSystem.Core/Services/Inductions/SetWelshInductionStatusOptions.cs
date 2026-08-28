namespace TeachingRecordSystem.Core.Services.Inductions;

public record SetWelshInductionStatusOptions
{
    public required Guid PersonId { get; init; }
    public required bool Passed { get; init; }
    public required DateOnly? StartDate { get; init; }
    public required DateOnly? CompletedDate { get; init; }
}
