using TeachingRecordSystem.SupportUi.Pages.SupportTasks.ChangeRequests.EditChangeRequest;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.ChangeRequests;

public class ChangeRequestsLinkGenerator(LinkGenerator linkGenerator)
{
    public string Index(string? search = null, ChangeRequestsSortByOption? sortBy = null, SortDirection? sortDirection = null, int? pageNumber = null) =>
        linkGenerator.GetRequiredPathByPage("/SupportTasks/ChangeRequests/Index", routeValues: new { search, sortBy, sortDirection, pageNumber });

    public EditChangeRequestLinkGenerator EditChangeRequest => new(linkGenerator);
}
