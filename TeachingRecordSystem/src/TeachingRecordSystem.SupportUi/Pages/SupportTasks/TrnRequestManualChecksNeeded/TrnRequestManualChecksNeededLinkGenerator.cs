using TeachingRecordSystem.SupportUi.Pages.SupportTasks.TrnRequestManualChecksNeeded.Resolve;
using TeachingRecordSystem.SupportUi.Services.SupportTasks;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.TrnRequestManualChecksNeeded;

public class TrnRequestManualChecksNeededLinkGenerator(LinkGenerator linkGenerator)
{
    public string Index(string? search = null, TrnRequestManualChecksSortByOption? sortBy = null, SortDirection? sortDirection = null, int? pageNumber = null) =>
        linkGenerator.GetRequiredPathByPage("/SupportTasks/TrnRequestManualChecksNeeded/Index", routeValues: new { search, sortBy, sortDirection, pageNumber });

    public ResolveTrnRequestManualChecksNeededLinkGenerator Resolve { get; } = new(linkGenerator);
}
