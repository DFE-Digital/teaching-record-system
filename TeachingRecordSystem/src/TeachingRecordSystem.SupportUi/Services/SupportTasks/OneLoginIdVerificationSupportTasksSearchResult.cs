namespace TeachingRecordSystem.SupportUi.Services.SupportTasks;

public record OneLoginIdVerificationSupportTasksSearchResult
{
    public required int TotalTaskCount { get; init; }
    public required ResultPage<OneLoginIdVerificationSupportTasksSearchResultItem> SearchResults { get; init; }
}

public record OneLoginIdVerificationSupportTasksSearchResultItem(
        string SupportTaskReference,
        string FirstName,
        string LastName,
        string? EmailAddress,
        DateTime CreatedOn) : ISupportTaskSearchResult
{
    public string Name => $"{FirstName} {LastName}";
}
