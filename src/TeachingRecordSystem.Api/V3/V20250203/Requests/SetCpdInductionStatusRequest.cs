using InductionStatus = TeachingRecordSystem.Core.ApiSchema.V3.V20250203.Dtos.InductionStatus;

namespace TeachingRecordSystem.Api.V3.V20250203.Requests;

public record SetCpdInductionStatusRequest
{
    public required InductionStatus Status { get; init; }
    public DateOnly? StartDate { get; init; }
    public DateOnly? CompletedDate { get; init; }
    public DateTimeOffset ModifiedOn { get; init; }
}
