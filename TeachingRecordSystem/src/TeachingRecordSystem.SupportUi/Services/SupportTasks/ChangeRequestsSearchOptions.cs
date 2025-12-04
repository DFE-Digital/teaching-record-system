namespace TeachingRecordSystem.SupportUi.Services.SupportTasks;

public record ChangeRequestsSearchOptions(
    string? Search,
    ChangeRequestsSortByOption? SortBy,
    SortDirection? SortDirection,
    SupportTaskType[]? ChangeRequestTypes);

public enum ChangeRequestsSortByOption
{
    Name,
    RequestedOn,
    ChangeType
}

