namespace TeachingRecordSystem.SupportUi.Services.SupportTasks;

public record TeachersPensionsPotentialDuplicatesSearchOptions(
    TeachersPensionsPotentialDuplicatesSortByOption? SortBy = null,
    SortDirection? SortDirection = null);

public enum TeachersPensionsPotentialDuplicatesSortByOption
{
    Name,
    CreatedOn,
    Filename,
    InterfaceId
}
