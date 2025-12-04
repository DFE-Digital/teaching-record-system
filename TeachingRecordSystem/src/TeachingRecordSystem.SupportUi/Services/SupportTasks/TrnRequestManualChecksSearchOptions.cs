namespace TeachingRecordSystem.SupportUi.Services.SupportTasks;

public record TrnRequestManualChecksSearchOptions(
    string? Search,
    TrnRequestManualChecksSortByOption? SortBy,
    SortDirection? SortDirection,
    Guid[]? Sources);

public enum TrnRequestManualChecksSortByOption
{
    Name,
    DateCreated,
    DateOfBirth,
    Source
}

