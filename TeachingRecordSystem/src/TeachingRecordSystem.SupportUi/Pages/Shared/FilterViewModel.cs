using TeachingRecordSystem.SupportUi.Pages.Common;

namespace TeachingRecordSystem.SupportUi.Pages.Shared;

public abstract class FilterViewModel
{
    public required string Name { get; init; }
    public required string DisplayName { get; init; }

    public static FilterViewModel Create<T>(Filter<T> filter)
    {
        return filter switch
        {
            SingleValueFilter<T> single => TextSearchFilterViewModel.Create(single),
            MultiValueFilter<T> multi => CheckboxFilterViewModel.Create(multi),

            _ => throw new NotImplementedException($"Create() not implemented for {filter.GetType()}.")
        };
    }
}
