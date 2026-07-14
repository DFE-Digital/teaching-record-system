namespace TeachingRecordSystem.SupportUi.Services.SupportTasks;

public record SupportTasksSearchOptions(
    SupportTaskType? SupportTaskType,
    Guid? AssignedToUserId,
    IReadOnlyCollection<SupportTaskStatus>? Statuses,
    SupportTasksSortByOption? SortBy,
    SortDirection? SortDirection);

public enum SupportTasksSortByOption
{
    SupportTaskReference,
    Subject,
    TaskType,
    Status,
    AssignedTo,
    RequestedOn
}
