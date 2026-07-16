using TeachingRecordSystem.Core.Models.SupportTasks;

namespace TeachingRecordSystem.Core.Services.SupportTasks.TeacherPensions;

public record ResolveTeacherPensionsPotentialDuplicateWithMergeOptions
{
    public required string SupportTaskReference { get; init; }
    public required TeacherPensionsPotentialDuplicateAttributes? ResolvedAttributes { get; init; }
    public required TeacherPensionsPotentialDuplicateAttributes? SelectedPersonAttributes { get; init; }
    public string? Comments { get; init; }
}
