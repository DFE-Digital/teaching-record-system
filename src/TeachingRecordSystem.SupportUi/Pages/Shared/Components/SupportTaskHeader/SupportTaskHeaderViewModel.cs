namespace TeachingRecordSystem.SupportUi.Pages.Shared.Components.SupportTaskHeader;

public record SupportTaskHeaderViewModel
{
    public required string PageHeader { get; init; }
    public required string SupportTaskReference { get; init; }
    public required SupportTaskType Type { get; init; }
    public required SupportTaskStatus Status { get; init; }
    public required string? AssignedToUserName { get; init; }
    public required IReadOnlyCollection<SupportTaskHeaderViewModelNote> Notes { get; init; }
}

public record SupportTaskHeaderViewModelNote
{
    public required string Content { get; init; }
    public required DateTime CreatedOn { get; init; }
    public required string CreatedBy { get; init; }
}
