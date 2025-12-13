using TeachingRecordSystem.SupportUi.Pages.SupportTasks.NpqTrnRequests.NoMatches;
using TeachingRecordSystem.SupportUi.Pages.SupportTasks.NpqTrnRequests.Reject;
using TeachingRecordSystem.SupportUi.Pages.SupportTasks.NpqTrnRequests.Resolve;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.NpqTrnRequests;

public class NpqTrnRequestsLinkGenerator(LinkGenerator linkGenerator)
{
    public string Index(string? search = null, SortByOption? sortBy = null, SortDirection? sortDirection = null, int? pageNumber = null) =>
        linkGenerator.GetRequiredPathByPage("/SupportTasks/NpqTrnRequests/Index", routeValues: new { search, sortBy, sortDirection, pageNumber });

    public string Details(string supportTaskReference, bool? createRecord = null) =>
        linkGenerator.GetRequiredPathByPage("/SupportTasks/NpqTrnRequests/Details", routeValues: new { supportTaskReference, createRecord });

    public string DetailsCancel(string supportTaskReference) =>
        linkGenerator.GetRequiredPathByPage("/SupportTasks/NpqTrnRequests/Details", handler: "Cancel", routeValues: new { supportTaskReference });

    public NoMatchesNpqTrnRequestLinkGenerator NoMatches { get; } = new(linkGenerator);
    public RejectNpqTrnRequestLinkGenerator Reject { get; } = new(linkGenerator);
    public ResolveNpqTrnRequesLinkGenerator Resolve { get; } = new(linkGenerator);
}
