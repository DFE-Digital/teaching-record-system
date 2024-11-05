namespace TeachingRecordSystem.Api.V3.VNext.Requests;

public record SetInductionStatusRequest
{
    public required V20240101.ApiModels.InductionStatus InductionStatus { get; init; }
    public required DateOnly StartDate { get; init; }
    public DateOnly? CompletionDate { get; init; }
}
