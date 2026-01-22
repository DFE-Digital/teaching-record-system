namespace TeachingRecordSystem.SupportUi.Services.SupportTasks;

public record OneLoginUserRecordMatchingSupportTasksOptions(
    string? Search = null,
    OneLoginUserRecordMatchingSupportTasksSortByOption? SortBy = null,
    SortDirection? SortDirection = null);
