namespace TeachingRecordSystem.SupportUi.Services.SupportTasks;

public record ApiTrnRequestsSearchOptions(
    string? Search,
    ApiTrnRequestsSortByOption? SortBy,
    SortDirection? SortDirection);

public enum ApiTrnRequestsSortByOption
{
    Name,
    Email,
    RequestedOn,
    Source
}
