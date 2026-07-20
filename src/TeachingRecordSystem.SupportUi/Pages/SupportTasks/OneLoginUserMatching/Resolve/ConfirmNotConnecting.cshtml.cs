using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models.SupportTasks;
using TeachingRecordSystem.Core.Services.SupportTasks.OneLoginUserMatching;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.OneLoginUserMatching.Resolve;

[TeachingRecordSystem.WebCommon.FormFlow.Journey(JourneyNames.ResolveOneLoginUserMatching), RequireJourneyInstance]
public class ConfirmNotConnecting(
    OneLoginUserMatchingSupportTaskService supportTaskService,
    TimeProvider timeProvider,
    SupportUiLinkGenerator linkGenerator) : PageModel
{
    private SupportTask? _supportTask;

    [FromRoute]
    public string SupportTaskReference { get; set; } = null!;

    public JourneyInstance<ResolveOneLoginUserMatchingState> JourneyInstance { get; set; } = null!;

    public string? EmailAddress { get; set; }

    public OneLoginUserNotConnectingReason Reason { get; set; }

    public string? AdditionalDetails { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(bool cancel)
    {
        if (cancel)
        {
            await JourneyInstance.DeleteAsync();

            if (_supportTask!.SupportTaskType == SupportTaskType.OneLoginUserIdVerification)
            {
                return Redirect(linkGenerator.SupportTasks.OneLoginUserMatching.IdVerification());
            }

            return Redirect(linkGenerator.SupportTasks.OneLoginUserMatching.RecordMatching());
        }

        bool emailSent = false;

        if (_supportTask!.SupportTaskType == SupportTaskType.OneLoginUserIdVerification)
        {
            var processContext = new ProcessContext(ProcessType.OneLoginUserIdVerificationSupportTaskCompleting, timeProvider.UtcNow, User.GetUserId());

            await supportTaskService.ResolveVerificationSupportTaskAsync(
                new VerifiedOnlyWithMatchesOutcomeOptions
                {
                    SupportTask = _supportTask!,
                    NotConnectingReason = JourneyInstance.State.NotConnectingReason!.Value,
                    NotConnectingAdditionalDetails = JourneyInstance.State.NotConnectingAdditionalDetails
                },
                processContext);
        }
        else
        {
            var processContext = new ProcessContext(ProcessType.OneLoginUserRecordMatchingSupportTaskCompleting, timeProvider.UtcNow, User.GetUserId());

            var resolveResult = await supportTaskService.ResolveRecordMatchingSupportTaskAsync(
                new NotConnectingOutcomeOptions
                {
                    SupportTask = _supportTask!,
                    NotConnectingReason = JourneyInstance.State.NotConnectingReason!.Value,
                    NotConnectingAdditionalDetails = JourneyInstance.State.NotConnectingAdditionalDetails
                },
                processContext);

            emailSent = resolveResult.EmailSent;
        }

        await JourneyInstance.DeleteAsync();

        var data = _supportTask!.GetData<IOneLoginUserMatchingData>();
        var firstVerifiedOrStatedName = data.VerifiedOrStatedNames!.First();
        var personName = $"{firstVerifiedOrStatedName.First()} {firstVerifiedOrStatedName.LastOrDefault()}";

        var appContent = await supportTaskService.GetAppContentAsync(_supportTask);

        var emailSentMessage = appContent?.OneLoginNotConnectedEmailSentFlashMessage is not null
            ? string.Format(appContent.OneLoginNotConnectedEmailSentFlashMessage, personName)
            : $"Request closed for {personName}. We’ve sent them an email confirming their GOV.UK One Login is not connected to a teaching record.";

        TempData.SetFlashNotificationBanner(
            heading: emailSent ? "Email sent" : "GOV.UK One Login not connected to a record",
            messageText: emailSent ? emailSentMessage : $"Request closed for {personName}.",
            notificationBannerType: NotificationBannerType.Default);

        if (_supportTask!.SupportTaskType == SupportTaskType.OneLoginUserIdVerification)
        {
            return Redirect(linkGenerator.SupportTasks.OneLoginUserMatching.IdVerification());
        }

        return Redirect(linkGenerator.SupportTasks.OneLoginUserMatching.RecordMatching());
    }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        if (JourneyInstance.State.Verified is not true)
        {
            context.Result = Redirect(linkGenerator.SupportTasks.OneLoginUserMatching.Resolve.Index(SupportTaskReference, JourneyInstance.InstanceId));
            return;
        }

        if (JourneyInstance.State.MatchedPersonId != ResolveOneLoginUserMatchingState.NotMatchedPersonIdSentinel)
        {
            context.Result = Redirect(linkGenerator.SupportTasks.OneLoginUserMatching.Resolve.Matches(SupportTaskReference, JourneyInstance.InstanceId));
            return;
        }

        if (JourneyInstance.State.NotConnectingReason is null)
        {
            context.Result = Redirect(linkGenerator.SupportTasks.OneLoginUserMatching.Resolve.NotConnecting(SupportTaskReference, JourneyInstance.InstanceId));
            return;
        }

        _supportTask = context.HttpContext.GetCurrentSupportTaskFeature().SupportTask;

        EmailAddress = _supportTask.OneLoginUser!.EmailAddress!;
        Reason = JourneyInstance.State.NotConnectingReason!.Value;
        AdditionalDetails = JourneyInstance.State.NotConnectingAdditionalDetails;
    }
}
