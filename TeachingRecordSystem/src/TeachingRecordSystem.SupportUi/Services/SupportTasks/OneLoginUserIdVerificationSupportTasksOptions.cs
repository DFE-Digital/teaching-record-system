namespace TeachingRecordSystem.SupportUi.Services.SupportTasks;

public record OneLoginUserIdVerificationSupportTasksOptions(
    string? Search = null,
    OneLoginIdVerificationSupportTasksSortByOption? SortBy = null,
    SortDirection? SortDirection = null);
