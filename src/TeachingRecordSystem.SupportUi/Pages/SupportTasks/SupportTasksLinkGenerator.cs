using TeachingRecordSystem.SupportUi.Pages.SupportTasks.ChangeRequests;
using TeachingRecordSystem.SupportUi.Pages.SupportTasks.IntegrationTransactions;
using TeachingRecordSystem.SupportUi.Pages.SupportTasks.OneLoginUserMatching;
using TeachingRecordSystem.SupportUi.Pages.SupportTasks.SupportTaskDetail;
using TeachingRecordSystem.SupportUi.Pages.SupportTasks.TeacherPensions;
using TeachingRecordSystem.SupportUi.Pages.SupportTasks.TrnRequestManualChecksNeeded;
using TeachingRecordSystem.SupportUi.Pages.SupportTasks.TrnRequests;
using TeachingRecordSystem.SupportUi.Services.SupportTasks;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks;

public class SupportTasksLinkGenerator(LinkGenerator linkGenerator)
{
    public TrnRequestsLinkGenerator TrnRequests { get; } = new(linkGenerator);
    public ChangeRequestsLinkGenerator ChangeRequests => new(linkGenerator);
    public IntegrationTransactionsLinkGenerator IntegrationTransactions { get; } = new(linkGenerator);
    public OneLoginUserMatchingLinkGenerator OneLoginUserMatching { get; } = new(linkGenerator);
    public TeacherPensionsLinkGenerator TeacherPensions { get; } = new(linkGenerator);
    public TrnRequestManualChecksNeededLinkGenerator TrnRequestManualChecksNeeded { get; } = new(linkGenerator);
    public SupportTaskDetailLinkGenerator SupportTaskDetail { get; } = new(linkGenerator);

    public string Active() => linkGenerator.GetRequiredPathByPage("/SupportTasks/Active");

    public string Active(
        SupportTaskType? type,
        Guid? assignedToUserId,
        IEnumerable<SupportTaskStatus>? statuses,
        SupportTasksSortByOption? sortBy,
        SortDirection? sortDirection,
        int? pageNumber = null) =>
        linkGenerator.GetRequiredPathByPage(
            "/SupportTasks/Active",
            routeValues: new { type, assignedToUserId, status = statuses, sortBy, sortDirection, pageNumber });

    // The selection banner is re-rendered on its own as tasks are ticked and unticked, so the request
    // for it carries the current filters - the banner links back to this page with the same filters.
    public string ActiveSelectionBanner(
        SupportTaskType? type,
        Guid? assignedToUserId,
        IEnumerable<SupportTaskStatus>? statuses,
        SupportTasksSortByOption? sortBy,
        SortDirection? sortDirection,
        int? pageNumber = null) =>
        linkGenerator.GetRequiredPathByPage(
            "/SupportTasks/Active",
            handler: "SelectionBanner",
            routeValues: new { type, assignedToUserId, status = statuses, sortBy, sortDirection, pageNumber });

    public string Completed() => linkGenerator.GetRequiredPathByPage("/SupportTasks/Completed");

    public string Completed(string? search) =>
        Completed(search, null, null, null, null);

    public string Completed(
        string? search,
        SupportTaskType? type,
        Guid? completedByUserId,
        CompletedTasksSortByOption? sortBy,
        SortDirection? sortDirection,
        int? pageNumber = null) =>
        linkGenerator.GetRequiredPathByPage(
            "/SupportTasks/Completed",
            routeValues: new { search, type, completedByUserId, sortBy, sortDirection, pageNumber });

    public string Assign(string? returnUrl = null) =>
        linkGenerator.GetRequiredPathByPage(
            "/SupportTasks/Assign",
            routeValues: new { returnUrl });
}
