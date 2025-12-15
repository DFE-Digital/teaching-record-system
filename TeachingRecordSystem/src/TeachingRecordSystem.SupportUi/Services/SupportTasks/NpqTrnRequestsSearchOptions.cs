namespace TeachingRecordSystem.SupportUi.Services.SupportTasks;

public record NpqTrnRequestsSearchOptions(
    string? Search = null,
    NpqTrnRequestsSortByOption? SortBy = null,
    SortDirection? SortDirection = null);

public enum NpqTrnRequestsSortByOption
{
    Name,
    Email,
    RequestedOn,
    PotentialDuplicate
}
