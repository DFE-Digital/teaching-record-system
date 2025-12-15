namespace TeachingRecordSystem.SupportUi.Services.SupportTasks;

public record ApiTrnRequestsSearchOptions(
    string? Search = null,
    ApiTrnRequestsSortByOption? SortBy = null,
    SortDirection? SortDirection = null);

public enum ApiTrnRequestsSortByOption
{
    Name,
    Email,
    RequestedOn,
    Source
}
