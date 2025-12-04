namespace TeachingRecordSystem.SupportUi.Services.SupportTasks;

public record ChangeRequestsSearchOptions(
    string? Search,
    ChangeRequestsSortByOption? SortBy,
    SortDirection? SortDirection,
    bool FormSubmitted,
    SupportTaskType[]? ChangeRequestTypes);

public enum ChangeRequestsSortByOption
{
    Name,
    RequestedOn,
    ChangeType
}

