namespace TeachingRecordSystem.SupportUi.Pages.Common;

public class MultiValueFilterValue(string name, string displayName)
{
    public string Name => name;
    public string DisplayName => displayName;

    public bool Selected { get; private set; }
    public int Count { get; private set; }

    public void UpdateSelected(string[]? selectedValues)
    {
        Selected = selectedValues?.Contains(Name) ?? false;
    }

    public void UpdateCounts(IEnumerable<FilterValueCount> valueCountsForFilter)
    {
        var countsForThisValue = valueCountsForFilter.Where(c => c.FilterValue == Name);
        var count = countsForThisValue?.Sum(c => c.Count) ?? 0;

        Count = count;
    }
}
