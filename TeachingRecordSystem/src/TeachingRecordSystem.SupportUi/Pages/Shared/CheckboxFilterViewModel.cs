using TeachingRecordSystem.SupportUi.Pages.Common;

namespace TeachingRecordSystem.SupportUi.Pages.Shared;

public class CheckboxFilterViewModel : FilterViewModel
{
    public required IEnumerable<CheckboxFilterValueViewModel> Values { get; init; }

    public static CheckboxFilterViewModel Create<T>(MultiValueFilter<T> filter)
    {
        return new()
        {
            Name = filter.Name,
            DisplayName = filter.DisplayName,
            Values = filter.Values.Select(CheckboxFilterValueViewModel.Create),
        };
    }
}
