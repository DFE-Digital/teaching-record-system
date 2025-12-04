namespace TeachingRecordSystem.SupportUi.Services.SupportTasks;

public record TrnRequestManualChecksSearchResult
{
    public required int TotalTaskCount { get; init; }
    public required Guid[]? Sources { get; init; }
    public required IReadOnlyDictionary<SupportTaskSearchFacet, IReadOnlyDictionary<object, int>>? Facets { get; init; }
    public required ResultPage<TrnRequestManualChecksSearchResultItem> SearchResults { get; init; }
}

public record TrnRequestManualChecksSearchResultItem(
    string SupportTaskReference,
    string FirstName,
    string MiddleName,
    string LastName,
    DateOnly DateOfBirth,
    DateTime CreatedOn,
    string SourceApplicationName);

