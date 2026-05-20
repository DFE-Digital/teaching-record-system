using TeachingRecordSystem.SupportUi.Pages.Common;

namespace TeachingRecordSystem.SupportUi.Pages.Shared;

public class CheckboxFilterValueViewModel
{
    public required string Name { get; init; }
    public required string DisplayName { get; init; }
    public required bool Selected { get; init; }
    public required int Count { get; init; }

    public static CheckboxFilterValueViewModel Create(MultiValueFilterValue value)
    {
        return new()
        {
            Name = value.Name,
            DisplayName = value.DisplayName,
            Selected = value.Selected,
            Count = value.Count
        };
    }
}
