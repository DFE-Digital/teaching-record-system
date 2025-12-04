namespace TeachingRecordSystem.SupportUi.Services.SupportTasks;

public record TrnRequestManualChecksSearchOptions(
    string? Search,
    TrnRequestManualChecksSortByOption? SortBy,
    SortDirection? SortDirection,
    bool FormSubmitted,
    Guid[]? Sources);

public enum TrnRequestManualChecksSortByOption
{
    Name,
    DateCreated,
    DateOfBirth,
    Source
}

