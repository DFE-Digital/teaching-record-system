using TeachingRecordSystem.SupportUi.Pages.Common;

namespace TeachingRecordSystem.SupportUi.Pages.Shared;

public class TextSearchFilterViewModel : FilterViewModel
{
    public required string? Value { get; init; }

    public static TextSearchFilterViewModel Create<T>(SingleValueFilter<T> filter)
    {
        return new()
        {
            Name = filter.Name,
            DisplayName = filter.DisplayName,
            Value = filter.Value
        };
    }
}
