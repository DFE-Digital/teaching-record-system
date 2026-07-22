using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models.SupportTasks;
using TeachingRecordSystem.Core.Services.SupportTasks.OneLoginUserMatching;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.OneLoginUserMatching.Resolve;

[Journey(JourneyNames.ResolveOneLoginUserMatching)]
public class ConfirmNotConnecting(
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

    public OneLoginUserNotConnectingReason Reason { get; set; }

    public string? AdditionalDetails { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Cancel)
        {
            journey.DeleteInstance();

            return Redirect(GetListPageUrl());
        }

        bool emailSent = false;

        if (_supportTask!.SupportTaskType == SupportTaskType.OneLoginUserIdVerification)
        {
            var processContext = new ProcessContext(ProcessType.OneLoginUserIdVerificationSupportTaskCompleting, timeProvider.UtcNow, User.GetUserId());

            await supportTaskService.ResolveVerificationSupportTaskAsync(
                new VerifiedOnlyWithMatchesOutcomeOptions
                {
                    SupportTask = _supportTask!,
                    NotConnectingReason = journey.State.NotConnectingReason!.Value,
                    NotConnectingAdditionalDetails = journey.State.NotConnectingAdditionalDetails
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
                    NotConnectingReason = journey.State.NotConnectingReason!.Value,
                    NotConnectingAdditionalDetails = journey.State.NotConnectingAdditionalDetails
                },
                processContext);

            emailSent = resolveResult.EmailSent;
        }

        journey.DeleteInstance();

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

        return Redirect(GetListPageUrl());
    }

    private string GetListPageUrl() =>
        _supportTask!.SupportTaskType == SupportTaskType.OneLoginUserIdVerification ?
            linkGenerator.SupportTasks.OneLoginUserMatching.IdVerification() :
            linkGenerator.SupportTasks.OneLoginUserMatching.RecordMatching();

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        _supportTask = context.HttpContext.GetCurrentSupportTaskFeature().SupportTask;

        BackLink = journey.GetBackLink();

        EmailAddress = _supportTask.OneLoginUser!.EmailAddress!;
        Reason = journey.State.NotConnectingReason!.Value;
        AdditionalDetails = journey.State.NotConnectingAdditionalDetails;
    }
}
