using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models.SupportTasks;
using TeachingRecordSystem.Core.Services.SupportTasks.OneLoginUserMatching;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.OneLoginUserMatching.Resolve;

[Journey(JourneyNames.ResolveOneLoginUserMatching)]
public class ConfirmReject(
    ResolveOneLoginUserMatchingJourneyCoordinator journey,
    OneLoginUserMatchingSupportTaskService supportTaskService,
    TimeProvider timeProvider,
    SupportUiLinkGenerator linkGenerator) : PageModel
{
    private SupportTask? _supportTask;

    [BindProperty]
    public bool Cancel { get; set; }

    public JourneyInstanceId InstanceId => journey.InstanceId;

    public string? BackLink { get; set; }

    public string? EmailAddress { get; set; }

    public string? Name { get; set; }

    public OneLoginIdVerificationRejectReason Reason { get; set; }

    public string? AdditionalDetails { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Cancel)
        {
            journey.DeleteInstance();

            return Redirect(linkGenerator.SupportTasks.OneLoginUserMatching.IdVerification());
        }

        var processContext = new ProcessContext(ProcessType.OneLoginUserIdVerificationSupportTaskCompleting, timeProvider.UtcNow, User.GetUserId());

        await supportTaskService.ResolveVerificationSupportTaskAsync(
            new NotVerifiedOutcomeOptions
            {
                SupportTask = _supportTask!,
                RejectReason = journey.State.RejectReason!.Value,
                RejectionAdditionalDetails = journey.State.RejectionAdditionalDetails
            },
            processContext);

        journey.DeleteInstance();

        var data = _supportTask!.GetData<OneLoginUserIdVerificationData>();
        TempData.SetFlashNotificationBanner(
            "GOV.UK One Login verification request rejected",
            $"Request closed for {data.StatedFirstName} {data.StatedLastName}. We’ve sent them an email confirming we could not verify their identity.",
            notificationBannerType: NotificationBannerType.Default);

        return Redirect(linkGenerator.SupportTasks.OneLoginUserMatching.IdVerification());
    }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        BackLink = journey.GetBackLink();

        _supportTask = context.HttpContext.GetCurrentSupportTaskFeature().SupportTask;
        var data = _supportTask.GetData<OneLoginUserIdVerificationData>();

        EmailAddress = _supportTask.OneLoginUser!.EmailAddress!;
        Name = $"{data.StatedFirstName} {data.StatedLastName}";
        Reason = journey.State.RejectReason!.Value;
        AdditionalDetails = journey.State.RejectionAdditionalDetails;
    }
}
