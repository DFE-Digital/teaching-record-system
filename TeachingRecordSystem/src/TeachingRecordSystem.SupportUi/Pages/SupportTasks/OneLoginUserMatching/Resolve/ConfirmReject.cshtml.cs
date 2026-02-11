using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models.SupportTasks;
using TeachingRecordSystem.Core.Services.OneLogin;
using TeachingRecordSystem.Core.Services.SupportTasks.OneLoginUserMatching;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.OneLoginUserMatching.Resolve;

[Journey(JourneyNames.ResolveOneLoginUserMatching), RequireJourneyInstance]
public class ConfirmReject(
    OneLoginUserMatchingSupportTaskService supportTaskService,
    OneLoginService oneLoginService,
    IClock clock,
    SupportUiLinkGenerator linkGenerator) : PageModel
{
    private SupportTask? _supportTask;

    [FromRoute]
    public string SupportTaskReference { get; set; } = null!;

    public JourneyInstance<ResolveOneLoginUserMatchingState> JourneyInstance { get; set; } = null!;

    public string? EmailAddress { get; set; }

    public OneLoginIdVerificationRejectReason Reason { get; set; }

    public string? AdditionalDetails { get; set; }

    public string? EmailContentHtml { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(bool cancel)
    {
        if (cancel)
        {
            await JourneyInstance.DeleteAsync();

            return Redirect(linkGenerator.SupportTasks.OneLoginUserMatching.IdVerification());
        }

        var processContext = new ProcessContext(ProcessType.OneLoginUserIdVerificationSupportTaskCompleting, clock.UtcNow, User.GetUserId());

        await supportTaskService.ResolveVerificationSupportTaskAsync(
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

        return Redirect(linkGenerator.SupportTasks.OneLoginUserMatching.IdVerification());
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        if (JourneyInstance.State.Verified is not false)
        {
            context.Result = Redirect(linkGenerator.SupportTasks.OneLoginUserMatching.Resolve.Index(SupportTaskReference, JourneyInstance.InstanceId));
            return;
        }

        if (JourneyInstance.State.RejectReason is null)
        {
            context.Result = Redirect(linkGenerator.SupportTasks.OneLoginUserMatching.Resolve.Reject(SupportTaskReference, JourneyInstance.InstanceId));
            return;
        }

        _supportTask = context.HttpContext.GetCurrentSupportTaskFeature().SupportTask;

        EmailAddress = _supportTask.OneLoginUser!.EmailAddress!;
        Reason = JourneyInstance.State.RejectReason!.Value;
        AdditionalDetails = JourneyInstance.State.RejectionAdditionalDetails;

        var data = _supportTask.GetData<OneLoginUserIdVerificationData>();
        EmailContentHtml = await oneLoginService.GetNotVerifiedEmailContentHtmlAsync(
            data.StatedFirstName + " " + data.StatedLastName,
            Reason.GetDisplayName()!);

        await base.OnPageHandlerExecutionAsync(context, next);
    }
}
