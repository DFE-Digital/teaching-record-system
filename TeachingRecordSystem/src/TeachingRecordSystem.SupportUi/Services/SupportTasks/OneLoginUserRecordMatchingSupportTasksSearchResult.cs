namespace TeachingRecordSystem.SupportUi.Services.SupportTasks;

public record OneLoginUserRecordMatchingSupportTasksSearchResult
{
    public required int TotalTaskCount { get; init; }
    public required ResultPage<OneLoginUserRecordMatchingSupportTasksSearchResultItem> SearchResults { get; init; }
}

public record OneLoginUserRecordMatchingSupportTasksSearchResultItem(
    string SupportTaskReference,
    SupportTaskStatus Status,
    string FirstName,
    string LastName,
    string[] OtherVerifiedNames,
    string? EmailAddress,
    DateTime CreatedOn) : ISupportTaskSearchResult
{
    public string Name => $"{FirstName} {LastName}";
}
