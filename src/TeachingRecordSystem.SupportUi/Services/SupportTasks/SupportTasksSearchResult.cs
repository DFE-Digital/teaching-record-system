namespace TeachingRecordSystem.SupportUi.Services.SupportTasks;

public record SupportTasksSearchResult
{
    public required int TotalTaskCount { get; init; }
    public required ResultPage<SupportTasksSearchResultItem> SearchResults { get; init; }
}

public record SupportTasksSearchResultItem(
    string SupportTaskReference,
    string Subject,
    SupportTaskType SupportTaskType,
    SupportTaskStatus Status,
    Guid? AssignedToUserId,
    string? AssignedToName,
    DateTime CreatedOn) : ISupportTaskSearchResult;
