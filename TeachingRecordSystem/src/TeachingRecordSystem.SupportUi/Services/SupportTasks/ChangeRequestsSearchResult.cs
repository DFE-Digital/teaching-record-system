namespace TeachingRecordSystem.SupportUi.Services.SupportTasks;

public record ChangeRequestsSearchResult
{
    public required int TotalRequestCount { get; init; }
    public required int NameChangeRequestCount { get; init; }
    public required int DateOfBirthChangeRequestCount { get; init; }
    public required ResultPage<ChangeRequestsSearchResultItem> SearchResults { get; init; }
}

public record ChangeRequestsSearchResultItem(
    string SupportTaskReference,
    string FirstName,
    string MiddleName,
    string LastName,
    string Name,
    DateTime CreatedOn,
    SupportTaskType SupportTaskType);
