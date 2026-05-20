namespace TeachingRecordSystem.SupportUi.Services.SupportTasks;

public record ChangeRequestsSearchOptions(
    string? Search = null,
    ChangeRequestsSortByOption? SortBy = null,
    SortDirection? SortDirection = null,
    SupportTaskType[]? ChangeRequestTypes = null);

public enum ChangeRequestsSortByOption
{
    Name,
    RequestedOn,
    ChangeType
}

