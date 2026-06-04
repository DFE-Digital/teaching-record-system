using TeachingRecordSystem.SupportUi.Pages.SupportTasks.TrnRequests.Resolve;
using TeachingRecordSystem.SupportUi.Services.SupportTasks;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.TrnRequests;

public class TrnRequestsLinkGenerator(LinkGenerator linkGenerator)
{
    public string Index(string? search = null, TrnRequestsSortByOption? sortBy = null, SortDirection? sortDirection = null, int? pageNumber = null) =>
        linkGenerator.GetRequiredPathByPage("/SupportTasks/TrnRequests/Index", routeValues: new { search, sortBy, sortDirection, pageNumber });

    public ResolveTrnRequestLinkGenerator Resolve { get; } = new(linkGenerator);
}
