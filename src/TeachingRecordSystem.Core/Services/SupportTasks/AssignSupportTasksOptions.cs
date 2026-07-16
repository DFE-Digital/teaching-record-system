namespace TeachingRecordSystem.Core.Services.SupportTasks;

public record AssignSupportTasksOptions
{
    public required IEnumerable<string> SupportTaskReferences { get; init; }
    public required Guid UserId { get; init; }
}
