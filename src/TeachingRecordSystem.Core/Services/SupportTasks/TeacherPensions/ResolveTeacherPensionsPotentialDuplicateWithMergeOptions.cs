using TeachingRecordSystem.Core.Models.SupportTasks;

namespace TeachingRecordSystem.Core.Services.SupportTasks.TeacherPensions;

public record ResolveTeacherPensionsPotentialDuplicateWithMergeOptions
{
    public required string SupportTaskReference { get; init; }

    /// The pre-existing record the task's record is merged into, and that the request resolves to
    public required Guid ExistingPersonId { get; init; }

    /// Where each of the record's attributes takes its value from. This journey offers no email address or
    /// middle name choice, so those are always unset and keep the existing record's value.
    public PersonAttributeSources AttributeSources { get; init; } = new();

    public string? Comments { get; init; }
}
