namespace TeachingRecordSystem.SupportUi.Pages.Shared;

public record ChoosePersonAttributeViewModel
{
    public required string Label { get; init; }
    public required bool Different { get; init; }
    public required string Name { get; init; }
    public required object LeftValue { get; init; }
    public required string? LeftLabel { get; init; }
    public required object RightValue { get; init; }
    public required string? RightLabel { get; init; }
    public required object? SelectedValue { get; init; }
}
