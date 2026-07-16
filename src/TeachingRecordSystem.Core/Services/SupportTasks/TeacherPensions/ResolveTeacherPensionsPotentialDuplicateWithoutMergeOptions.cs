namespace TeachingRecordSystem.Core.Services.SupportTasks.TeacherPensions;

public record ResolveTeacherPensionsPotentialDuplicateWithoutMergeOptions
{
    public required string SupportTaskReference { get; init; }
    public string? Comments { get; init; }
}
