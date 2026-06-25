namespace TeachingRecordSystem.Core.Services.SupportTasks.OneLoginUserMatching;

public record ResolveRecordMatchingSupportTaskResult
{
    public required bool EmailSent { get; init; }
}
