namespace TeachingRecordSystem.SupportUi.Services.SupportTasks;

public record TeachersPensionsPotentialDuplicatesSearchResult
{
    public required int TotalTaskCount { get; init; }
    public required ResultPage<TeachersPensionsPotentialDuplicatesSearchResultItem> SearchResults { get; init; }
}

public record TeachersPensionsPotentialDuplicatesSearchResultItem(
    string SupportTaskReference,
    string Filename,
    long IntegrationTransactionId,
    string Name,
    DateTime CreatedOn) : ISupportTaskSearchResult;
