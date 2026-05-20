using TeachingRecordSystem.Core.Models.SupportTasks;

namespace TeachingRecordSystem.Core.Services.SupportTasks;

public record CreateSupportTaskOptions
{
    public required SupportTaskType SupportTaskType { get; init; }
    public required ISupportTaskData Data { get; init; }
    public required Guid? PersonId { get; init; }
    public required string? OneLoginUserSubject { get; init; }
    public required (Guid ApplicationUserId, string RequestId)? TrnRequest { get; init; }
}
