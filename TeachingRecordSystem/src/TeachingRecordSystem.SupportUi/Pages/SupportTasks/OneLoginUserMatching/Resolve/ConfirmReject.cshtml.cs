using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models.SupportTasks;
using TeachingRecordSystem.Core.Services.SupportTasks.OneLoginUserMatching;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.OneLoginUserMatching.Resolve;

[Journey(JourneyNames.ResolveOneLoginUserMatching), RequireJourneyInstance]
public class ConfirmReject(OneLoginUserMatchingSupportTaskService supportTaskService, TimeProvider timeProvider, SupportUiLinkGenerator linkGenerator) : PageModel
{
    private SupportTask? _supportTask;

    [FromRoute]
    public string SupportTaskReference { get; set; } = null!;

    public JourneyInstance<ResolveOneLoginUserMatchingState> JourneyInstance { get; set; } = null!;

    public string? EmailAddress { get; set; }

    public string? Name { get; set; }

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

            return Redirect(linkGenerator.SupportTasks.OneLoginUserMatching.IdVerification());
        }

        var processContext = new ProcessContext(ProcessType.OneLoginUserIdVerificationSupportTaskCompleting, timeProvider.UtcNow, User.GetUserId());

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
            $"Request closed for {data.StatedFirstName} {data.StatedLastName}. We’ve sent them an email confirming we could not verify their identity.");

        return Redirect(linkGenerator.SupportTasks.OneLoginUserMatching.IdVerification());
    }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
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
        var data = _supportTask.GetData<OneLoginUserIdVerificationData>();

        EmailAddress = _supportTask.OneLoginUser!.EmailAddress!;
        Name = $"{data.StatedFirstName} {data.StatedLastName}";
        Reason = JourneyInstance.State.RejectReason!.Value;
        AdditionalDetails = JourneyInstance.State.RejectionAdditionalDetails;
    }
}
