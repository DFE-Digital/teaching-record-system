namespace TeachingRecordSystem.Api.V3.VNext.Requests;

public record SetWelshInductionStatusRequest
{
    public required bool Passed { get; init; }
    public required DateOnly StartDate { get; init; }
    public required DateOnly CompletedDate { get; init; }
}
