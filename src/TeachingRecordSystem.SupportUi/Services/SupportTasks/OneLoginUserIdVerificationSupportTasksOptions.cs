namespace TeachingRecordSystem.SupportUi.Services.SupportTasks;

public record OneLoginUserIdVerificationSupportTasksOptions(
    string? Search = null,
    OneLoginUserIdVerificationSupportTasksSortByOption? SortBy = null,
    SortDirection? SortDirection = null);
