namespace TeachingRecordSystem.SupportUi.Services.SupportTasks;

public record TrnRequestsSearchOptions(
    string? Search = null,
    TrnRequestsSortByOption? SortBy = null,
    SortDirection? SortDirection = null);

public enum TrnRequestsSortByOption
{
    Name,
    Email,
    RequestedOn,
    Source
}
