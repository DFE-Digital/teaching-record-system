using InductionStatus = TeachingRecordSystem.Core.ApiSchema.V3.VNext.Dtos.InductionStatus;

namespace TeachingRecordSystem.Api.V3.VNext.Requests;

public record SetCpdInductionStatusRequest
{
    public required InductionStatus Status { get; init; }
    public DateOnly? StartDate { get; init; }
    public DateOnly? CompletedDate { get; init; }
    public DateTimeOffset ModifiedOn { get; init; }
}
