namespace TeachingRecordSystem.Api.V3.VNext.Requests;

public record SetInductionStatusRequest
{
    public required TeachingRecordSystem.Core.ApiSchema.V3.V20240101.Dtos.InductionStatus InductionStatus { get; init; }
    public required DateOnly StartDate { get; init; }
    public DateOnly? CompletionDate { get; init; }
}
