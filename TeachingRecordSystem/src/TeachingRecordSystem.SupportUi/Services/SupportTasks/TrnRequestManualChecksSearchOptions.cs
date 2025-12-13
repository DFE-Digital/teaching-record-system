namespace TeachingRecordSystem.SupportUi.Services.SupportTasks;

public record TrnRequestManualChecksSearchOptions(
    string? Search = null,
    TrnRequestManualChecksSortByOption? SortBy = null,
    SortDirection? SortDirection = null,
    IEnumerable<Guid>? Sources = null);

public enum TrnRequestManualChecksSortByOption
{
    Name,
    DateCreated,
    DateOfBirth,
    Source
}

