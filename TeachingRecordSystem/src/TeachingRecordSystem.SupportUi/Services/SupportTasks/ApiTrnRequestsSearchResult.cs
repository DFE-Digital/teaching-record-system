namespace TeachingRecordSystem.SupportUi.Services.SupportTasks;

public record ApiTrnRequestsSearchResult
{
    public required int TotalTaskCount { get; init; }
    public required ResultPage<ApiTrnRequestsSearchResultItem> SearchResults { get; init; }
}

public record ApiTrnRequestsSearchResultItem(
    string SupportTaskReference,
    string FirstName,
    string MiddleName,
    string LastName,
    string? EmailAddress,
    DateTime CreatedOn,
    string SourceApplicationName) : ISupportTaskSearchResult;
