using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models.SupportTasks;
using TeachingRecordSystem.Core.Services.SupportTasks.OneLoginUserMatching;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.OneLoginUserMatching.Resolve;

[Journey(JourneyNames.ResolveOneLoginUserMatching), RequireJourneyInstance]
public class ConfirmNotConnecting(
    OneLoginUserMatchingSupportTaskService supportTaskService,
    IClock clock,
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

        if (_supportTask!.SupportTaskType == SupportTaskType.OneLoginUserIdVerification)
        {
            var processContext = new ProcessContext(ProcessType.OneLoginUserIdVerificationSupportTaskCompleting, clock.UtcNow, User.GetUserId());

            await supportTaskService.ResolveSupportTaskAsync(
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
            var processContext = new ProcessContext(ProcessType.OneLoginUserRecordMatchingSupportTaskCompleting, clock.UtcNow, User.GetUserId());

            await supportTaskService.ResolveSupportTaskAsync(
                new NotConnectingOutcomeOptions
                {
                    SupportTask = _supportTask!,
                    NotConnectingReason = JourneyInstance.State.NotConnectingReason!.Value,
                    NotConnectingAdditionalDetails = JourneyInstance.State.NotConnectingAdditionalDetails
                },
                processContext);
        }

        await JourneyInstance.DeleteAsync();

        var data = _supportTask!.GetData<IOneLoginUserMatchingData>();
        var firstVerifiedOrStatedName = data.VerifiedOrStatedNames!.First();
        TempData.SetFlashSuccess(
            "GOV.UK One Login not connected to a record",
            $"Request closed for {firstVerifiedOrStatedName.First()} {firstVerifiedOrStatedName.LastOrDefault()}.");

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
