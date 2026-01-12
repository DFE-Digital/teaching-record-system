namespace TeachingRecordSystem.SupportUi.Services.SupportTasks;

public record TeachersPensionsPotentialDuplicatesSearchOptions(
    string? Search = null,
    TeachersPensionsPotentialDuplicatesSortByOption? SortBy = null,
    SortDirection? SortDirection = null);

public enum TeachersPensionsPotentialDuplicatesSortByOption
{
    Name,
    CreatedOn,
    Filename,
    InterfaceId
}
