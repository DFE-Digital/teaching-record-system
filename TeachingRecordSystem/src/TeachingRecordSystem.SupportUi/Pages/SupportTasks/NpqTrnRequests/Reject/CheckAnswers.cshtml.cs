using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models.SupportTasks;
using TeachingRecordSystem.Core.Services.SupportTasks;
using TeachingRecordSystem.Core.Services.TrnRequests;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.NpqTrnRequests.Reject;

[Journey(JourneyNames.RejectNpqTrnRequest), RequireJourneyInstance]
public class CheckAnswersModel(
    TrnRequestService trnRequestService,
    SupportTaskService supportTaskService,
    SupportUiLinkGenerator linkGenerator,
    IClock clock) : PageModel
{
    public string? SourceApplicationUserName { get; set; }

    public string PersonName => ' '.JoinNonEmpty(RequestData!.Name);

    public TrnRequestMetadata? RequestData { get; set; }

    public JourneyInstance<RejectNpqTrnRequestState>? JourneyInstance { get; set; }

    public RejectionReasonOption? RejectionReason { get; set; }

    [FromRoute]
    public string SupportTaskReference { get; set; } = null!;

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var supportTask = HttpContext.GetCurrentSupportTaskFeature().SupportTask;
        var trnRequest = supportTask.TrnRequestMetadata!;

        var processContext = new ProcessContext(ProcessType.NpqTrnRequestRejecting, clock.UtcNow, User.GetUserId());

        await trnRequestService.RejectTrnRequestAsync(trnRequest, processContext);

        await supportTaskService.UpdateSupportTaskAsync(
            new UpdateSupportTaskOptions<NpqTrnRequestData>
            {
                SupportTask = SupportTaskReference,
                UpdateData = data => data with { SupportRequestOutcome = SupportRequestOutcome.Rejected },
                Status = SupportTaskStatus.Closed,
                Comments = null,
                RejectionReason = JourneyInstance!.State.RejectionReason!.GetDisplayName()
            },
            processContext);

        TempData.SetFlashSuccess(
            $"TRN request for {PersonName} rejected");

        await JourneyInstance!.CompleteAsync();
        return Redirect(linkGenerator.SupportTasks.NpqTrnRequests.Index());
    }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await JourneyInstance!.DeleteAsync();
        return Redirect(linkGenerator.SupportTasks.NpqTrnRequests.Index());
    }

    public override Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        var supportTask = HttpContext.GetCurrentSupportTaskFeature().SupportTask;
        RequestData = supportTask.TrnRequestMetadata!;
        SourceApplicationUserName = RequestData.ApplicationUser!.Name;

        RejectionReason = JourneyInstance!.State.RejectionReason;
        if (!RejectionReason.HasValue)
        {
            Redirect(linkGenerator.SupportTasks.NpqTrnRequests.Reject.Reason(supportTask.SupportTaskReference, JourneyInstance!.InstanceId));
        }

        return next();
    }
}
