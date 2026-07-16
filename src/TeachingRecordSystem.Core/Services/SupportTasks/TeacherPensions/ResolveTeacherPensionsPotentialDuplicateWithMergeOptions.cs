using TeachingRecordSystem.Core.Models.SupportTasks;

namespace TeachingRecordSystem.Core.Services.SupportTasks.TeacherPensions;

public record ResolveTeacherPensionsPotentialDuplicateWithMergeOptions
{
    public required string SupportTaskReference { get; init; }

    /// The pre-existing record the task's record is merged into, and that the request resolves to
    public required Guid ExistingPersonId { get; init; }

    public IReadOnlyCollection<PersonMatchedAttribute> AttributesToUpdate { get; init; } = [];
    public required TeacherPensionsPotentialDuplicateAttributes? ResolvedAttributes { get; init; }
    public required TeacherPensionsPotentialDuplicateAttributes? SelectedPersonAttributes { get; init; }
    public string? Comments { get; init; }
}
