namespace TeachingRecordSystem.SupportUi.Services.SupportTasks;

public record OneLoginUserIdVerificationSupportTasksSearchResult
{
    public required int TotalTaskCount { get; init; }
    public required ResultPage<OneLoginUserIdVerificationSupportTasksSearchResultItem> SearchResults { get; init; }
}

public record OneLoginUserIdVerificationSupportTasksSearchResultItem(
    string SupportTaskReference,
    SupportTaskStatus Status,
    string FirstName,
    string LastName,
    string? EmailAddress,
    DateTime CreatedOn) : ISupportTaskSearchResult
{
    public string Name => $"{FirstName} {LastName}";
}
