using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models.SupportTasks;
using TeachingRecordSystem.Core.Services.SupportTasks.OneLoginUserIdVerification;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.OneLoginUserIdVerification.Resolve;

[Journey(JourneyNames.ResolveOneLoginUserIdVerification), RequireJourneyInstance]
public class ConfirmRejectModel(OneLoginUserIdVerificationSupportTaskService supportTaskService, IClock clock, SupportUiLinkGenerator linkGenerator) : PageModel
{
    private SupportTask? _supportTask;

    [FromRoute]
    public string SupportTaskReference { get; set; } = null!;

    public JourneyInstance<ResolveOneLoginUserIdVerificationState> JourneyInstance { get; set; } = null!;

    public string? EmailAddress { get; set; }

    public OneLoginIdVerificationRejectReason Reason { get; set; }

    public string? AdditionalDetails { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(bool cancel)
    {
        if (cancel)
        {
            await JourneyInstance.DeleteAsync();

            return Redirect(linkGenerator.SupportTasks.OneLoginUserIdVerification.Index());
        }

        var processContext = new ProcessContext(ProcessType.OneLoginUserIdVerificationSupportTaskCompleting, clock.UtcNow, User.GetUserId());

        await supportTaskService.ResolveSupportTaskAsync(
            new NotVerifiedOutcomeOptions
            {
                SupportTask = _supportTask!,
                RejectReason = JourneyInstance.State.RejectReason!.Value,
                RejectionAdditionalDetails = JourneyInstance.State.RejectionAdditionalDetails
            },
            processContext);

        await JourneyInstance.DeleteAsync();

        var data = _supportTask!.GetData<OneLoginUserIdVerificationData>();
        TempData.SetFlashSuccess(
            "GOV.UK One Login verification request rejected",
            $"Request closed for {data.StatedFirstName} {data.StatedLastName}.");

        return Redirect(linkGenerator.SupportTasks.OneLoginUserIdVerification.Index());
    }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        if (JourneyInstance.State.Verified is not false)
        {
            context.Result = Redirect(linkGenerator.SupportTasks.OneLoginUserIdVerification.Resolve.Index(SupportTaskReference, JourneyInstance.InstanceId));
            return;
        }

        if (JourneyInstance.State.RejectReason is null)
        {
            context.Result = Redirect(linkGenerator.SupportTasks.OneLoginUserIdVerification.Resolve.Reject(SupportTaskReference, JourneyInstance.InstanceId));
            return;
        }

        _supportTask = context.HttpContext.GetCurrentSupportTaskFeature().SupportTask;

        EmailAddress = _supportTask.OneLoginUser!.EmailAddress!;
        Reason = JourneyInstance.State.RejectReason!.Value;
        AdditionalDetails = JourneyInstance.State.RejectionAdditionalDetails;
    }
}
