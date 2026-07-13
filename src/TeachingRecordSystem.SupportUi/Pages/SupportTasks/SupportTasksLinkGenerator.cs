using TeachingRecordSystem.SupportUi.Pages.SupportTasks.ChangeRequests;
using TeachingRecordSystem.SupportUi.Pages.SupportTasks.IntegrationTransactions;
using TeachingRecordSystem.SupportUi.Pages.SupportTasks.OneLoginUserMatching;
using TeachingRecordSystem.SupportUi.Pages.SupportTasks.SupportTaskDetail;
using TeachingRecordSystem.SupportUi.Pages.SupportTasks.TeacherPensions;
using TeachingRecordSystem.SupportUi.Pages.SupportTasks.TrnRequestManualChecksNeeded;
using TeachingRecordSystem.SupportUi.Pages.SupportTasks.TrnRequests;

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
}
