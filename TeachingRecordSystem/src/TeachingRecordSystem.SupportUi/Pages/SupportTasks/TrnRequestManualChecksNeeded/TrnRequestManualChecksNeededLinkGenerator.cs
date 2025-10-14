using TeachingRecordSystem.SupportUi.Pages.SupportTasks.TrnRequestManualChecksNeeded.Resolve;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.TrnRequestManualChecksNeeded;

public class TrnRequestManualChecksNeededLinkGenerator(LinkGenerator linkGenerator)
{
    public string Index(string? search = null, TrnRequestManualChecksNeededSortByOption? sortBy = null, SortDirection? sortDirection = null, int? pageNumber = null) =>
        linkGenerator.GetRequiredPathByPage("/SupportTasks/TrnRequestManualChecksNeeded/Index", routeValues: new { search, sortBy, sortDirection, pageNumber });

    public ResolveTrnRequestManualChecksNeededLinkGenerator Resolve { get; } = new(linkGenerator);
}
