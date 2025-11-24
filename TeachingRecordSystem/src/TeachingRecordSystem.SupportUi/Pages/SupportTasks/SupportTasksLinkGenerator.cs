using TeachingRecordSystem.SupportUi.Pages.SupportTasks.ApiTrnRequests;
using TeachingRecordSystem.SupportUi.Pages.SupportTasks.ChangeRequests;
using TeachingRecordSystem.SupportUi.Pages.SupportTasks.ConnectOneLoginUser;
using TeachingRecordSystem.SupportUi.Pages.SupportTasks.IntegrationTransactions;
using TeachingRecordSystem.SupportUi.Pages.SupportTasks.NpqTrnRequests;
using TeachingRecordSystem.SupportUi.Pages.SupportTasks.OneLoginUserIdVerification;
using TeachingRecordSystem.SupportUi.Pages.SupportTasks.TeacherPensions;
using TeachingRecordSystem.SupportUi.Pages.SupportTasks.TrnRequestManualChecksNeeded;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks;

public class SupportTasksLinkGenerator(LinkGenerator linkGenerator)
{
    public string Index() =>
        linkGenerator.GetRequiredPathByPage("/SupportTasks/Index");

    public string Index(SupportTaskCategory[]? categories = null, IndexModel.SortByOption? sortBy = null, string? reference = null, bool? filtersApplied = null) =>
        linkGenerator.GetRequiredPathByPage("/SupportTasks/Index", routeValues: new { category = categories, sortBy, reference, _f = filtersApplied == true ? "1" : null });

    public ApiTrnRequestsLinkGenerator ApiTrnRequests { get; } = new(linkGenerator);
    public ChangeRequestsLinkGenerator ChangeRequests => new(linkGenerator);
    public ConnectOneLoginUserLinkGenerator ConnectOneLoginUser { get; } = new(linkGenerator);
    public IntegrationTransactionsLinkGenerator IntegrationTransactions { get; } = new(linkGenerator);
    public NpqTrnRequestsLinkGenerator NpqTrnRequests { get; } = new(linkGenerator);
    public OneLoginUserIdVerificationLinkGenerator OneLoginUserIdVerification { get; } = new(linkGenerator);
    public TeacherPensionsLinkGenerator TeacherPensions { get; } = new(linkGenerator);
    public TrnRequestManualChecksNeededLinkGenerator TrnRequestManualChecksNeeded { get; } = new(linkGenerator);
}
