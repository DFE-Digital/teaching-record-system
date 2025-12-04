namespace TeachingRecordSystem.SupportUi.Services.SupportTasks;

public record NpqTrnRequestsSearchResult
{
    public required int TotalTaskCount { get; init; }
    public required ResultPage<NpqTrnRequestsSearchResultItem> SearchResults { get; init; }
}

public record NpqTrnRequestsSearchResultItem(
    string SupportTaskReference,
    string FirstName,
    string MiddleName,
    string LastName,
    string? EmailAddress,
    DateTime CreatedOn,
    string SourceApplicationName,
    bool? PotentialDuplicate);
