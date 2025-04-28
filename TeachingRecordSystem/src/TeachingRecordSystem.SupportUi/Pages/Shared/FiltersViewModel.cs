using TeachingRecordSystem.SupportUi.Pages.Common;

namespace TeachingRecordSystem.SupportUi.Pages.Shared;

public class FiltersViewModel
{
    public required IEnumerable<FilterViewModel> Filters { get; init; }
    public required string SubmitButtonText { get; init; }
    public required string ClearUrl { get; init; }

    public static FiltersViewModel Create<T>(FilterCollection<T> filters, string submitButtonText, string clearUrl = "?")
    {
        return new()
        {
            Filters = filters.Select(FilterViewModel.Create),
            SubmitButtonText = submitButtonText,
            ClearUrl = clearUrl
        };
    }
}
