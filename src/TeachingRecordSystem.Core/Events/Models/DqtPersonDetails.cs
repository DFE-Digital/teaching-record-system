namespace TeachingRecordSystem.Core.Events.Models;

public record DqtPersonDetails : PersonDetails
{
    public required string? Trn { get; init; }
    public required DateOnly? DateOfDeath { get; init; }
    public required DateOnly? QtsDate { get; init; }
    public required DateOnly? EytsDate { get; init; }
    public required DateOnly? QtlsDate { get; init; }
    public required QtlsStatus QtlsStatus { get; init; }
    public required InductionStatus? InductionStatus { get; init; }
    public required string? DqtInductionStatus { get; init; }
}
