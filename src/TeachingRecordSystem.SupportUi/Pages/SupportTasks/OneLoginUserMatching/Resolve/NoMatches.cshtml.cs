using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models.SupportTasks;
using TeachingRecordSystem.Core.Services.SupportTasks.OneLoginUserMatching;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.OneLoginUserMatching.Resolve;

[TeachingRecordSystem.WebCommon.FormFlow.Journey(JourneyNames.ResolveOneLoginUserMatching), RequireJourneyInstance]
public class NoMatches(
    SupportUiLinkGenerator linkGenerator,
    OneLoginUserMatchingSupportTaskService supportTaskService,
    TimeProvider timeProvider) : PageModel
{
    private SupportTask? _supportTask;

    [FromRoute]
    public string SupportTaskReference { get; set; } = null!;

    public JourneyInstance<ResolveOneLoginUserMatchingState> JourneyInstance { get; set; } = null!;

    public string? Name { get; set; }

    public bool? IsRecordMatchingOnly { get; set; }

    public string? NoMatchesPageContent { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(bool cancel)
    {
        if (cancel)
        {
            await JourneyInstance.DeleteAsync();

            return Redirect(_supportTask!.SupportTaskType is SupportTaskType.OneLoginUserIdVerification ?
                linkGenerator.SupportTasks.OneLoginUserMatching.IdVerification() :
                linkGenerator.SupportTasks.OneLoginUserMatching.RecordMatching());
        }

        bool emailSent;

        if (_supportTask!.SupportTaskType == SupportTaskType.OneLoginUserIdVerification)
        {
            var processContext = new ProcessContext(ProcessType.OneLoginUserIdVerificationSupportTaskCompleting, timeProvider.UtcNow, User.GetUserId());

            await supportTaskService.ResolveVerificationSupportTaskAsync(
                new VerifiedOnlyWithoutMatchesOutcomeOptions
                {
                    SupportTask = _supportTask!
                },
                processContext);

            emailSent = true;
        }
        else
        {
            var processContext = new ProcessContext(ProcessType.OneLoginUserRecordMatchingSupportTaskCompleting, timeProvider.UtcNow, User.GetUserId());

            var resolveResult = await supportTaskService.ResolveRecordMatchingSupportTaskAsync(
                new NoMatchesOutcomeOptions
                {
                    SupportTask = _supportTask!
                },
                processContext);

            emailSent = resolveResult.EmailSent;
        }

        await JourneyInstance.DeleteAsync();

        var appContent = await supportTaskService.GetAppContentAsync(_supportTask);

        var emailSentMessage = appContent?.OneLoginNoMatchesEmailSentFlashMessage is not null
            ? string.Format(appContent.OneLoginNoMatchesEmailSentFlashMessage, Name)
            : $"Request closed for {Name}. We’ve sent them an email confirming we could not find a teaching record matching their GOV.UK One Login.";

        TempData.SetFlashNotificationBanner(
            heading: emailSent ? "Email sent" : "Request closed",
            messageText: emailSent ? emailSentMessage : $"Request closed for {Name}.",
            notificationBannerType: NotificationBannerType.Default);

        return Redirect(_supportTask!.SupportTaskType is SupportTaskType.OneLoginUserIdVerification ?
            linkGenerator.SupportTasks.OneLoginUserMatching.IdVerification() :
            linkGenerator.SupportTasks.OneLoginUserMatching.RecordMatching());
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        if (JourneyInstance.State.Verified is not true)
        {
            context.Result = Redirect(linkGenerator.SupportTasks.OneLoginUserMatching.Resolve.Index(SupportTaskReference, JourneyInstance.InstanceId));
            return;
        }

        if (JourneyInstance.State.MatchedPersons.Count > 0)
        {
            context.Result = Redirect(linkGenerator.SupportTasks.OneLoginUserMatching.Resolve.Matches(SupportTaskReference, JourneyInstance.InstanceId));
            return;
        }

        _supportTask = context.HttpContext.GetCurrentSupportTaskFeature().SupportTask;
        IsRecordMatchingOnly = _supportTask!.SupportTaskType == SupportTaskType.OneLoginUserRecordMatching;
        var data = _supportTask.GetData<IOneLoginUserMatchingData>();

        // For the time being only display first verified name and dob if there are multiples (but still match on both)
        var firstVerifiedOrStatedName = data.VerifiedOrStatedNames!.First();
        Name = $"{firstVerifiedOrStatedName.First()} {firstVerifiedOrStatedName.LastOrDefault()}";

        var appContent = await supportTaskService.GetAppContentAsync(_supportTask);
        NoMatchesPageContent = appContent?.OneLoginNoMatchesPageContentHtml;

        await base.OnPageHandlerExecutionAsync(context, next);
    }
}
