namespace TeachingRecordSystem.SupportUi.Services.SupportTasks;

public record NpqTrnRequestsSearchOptions(
    string? Search,
    NpqTrnRequestsSortByOption? SortBy,
    SortDirection? SortDirection);

public enum NpqTrnRequestsSortByOption
{
    Name,
    Email,
    RequestedOn,
    PotentialDuplicate
}
