using TeachingRecordSystem.SupportUi.Pages.OneLogins;

namespace TeachingRecordSystem.SupportUi.Services.OneLogins;

public class OneLoginSearchOptions
{
    public required string? Search { get; init; }
    public OneLoginSearchSortByOption? SortBy { get; init; }
    public SortDirection? SortDirection { get; init; }
}
