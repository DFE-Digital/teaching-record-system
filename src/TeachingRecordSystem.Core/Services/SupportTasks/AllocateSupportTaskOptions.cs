namespace TeachingRecordSystem.Core.Services.SupportTasks;

public record AllocateSupportTaskOptions
{
    public required string SupportTaskReference { get; init; }
    public required SupportTaskStatus Status { get; init; }
    public required Guid? AssignToUserId { get; init; }
}
