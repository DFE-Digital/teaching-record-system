namespace TeachingRecordSystem.SupportUi.Services.SupportTasks;

public record TrnRequestsSearchResult
{
    public required int TotalTaskCount { get; init; }
    public required ResultPage<TrnRequestsSearchResultItem> SearchResults { get; init; }
    public required IReadOnlyCollection<TrnRequestsSearchResultBySourceApplication> BySourceApplication { get; init; }
}

public record TrnRequestsSearchResultItem(
    string SupportTaskReference,
    string FirstName,
    string MiddleName,
    string LastName,
    string? EmailAddress,
    DateTime CreatedOn,
    string SourceApplicationName) : ISupportTaskSearchResult;

public record TrnRequestsSearchResultBySourceApplication(Guid ApplicationUserId, int Count);
