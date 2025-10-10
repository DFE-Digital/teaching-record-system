using TeachingRecordSystem.SupportUi.Pages.SupportTasks.ApiTrnRequests.Resolve;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.ApiTrnRequests;

public class ApiTrnRequestsLinkGenerator(LinkGenerator linkGenerator)
{
    public string Index(string? search = null, ApiTrnRequestsSortByOption? sortBy = null, SortDirection? sortDirection = null, int? pageNumber = null) =>
        linkGenerator.GetRequiredPathByPage("/SupportTasks/ApiTrnRequests/Index", routeValues: new { search, sortBy, sortDirection, pageNumber });

    public ResolveApiTrnRequestLinkGenerator Resolve { get; } = new(linkGenerator);
}
