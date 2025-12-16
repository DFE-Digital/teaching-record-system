using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models.SupportTasks;
using TeachingRecordSystem.Core.Services.OneLogin;
using TeachingRecordSystem.Core.Services.SupportTasks;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.OneLoginUserIdVerification.Resolve;

[Journey(JourneyNames.ResolveOneLoginUserIdVerification), RequireJourneyInstance]
public class ConfirmNotConnecting(SupportTaskService supportTaskService, OneLoginService oneLoginService, IClock clock, SupportUiLinkGenerator linkGenerator) :
    PageModel
{
    private SupportTask? _supportTask;

    [FromRoute]
    public string SupportTaskReference { get; set; } = null!;

    public JourneyInstance<ResolveOneLoginUserIdVerificationState> JourneyInstance { get; set; } = null!;

    public string? EmailAddress { get; set; }

    public OneLoginIdVerificationNotConnectingReason Reason { get; set; }

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

        var data = _supportTask!.GetData<OneLoginUserIdVerificationData>();

        await oneLoginService.SetUserVerifiedAsync(
            new SetUserVerifiedOptions
            {
                OneLoginUserSubject = _supportTask.OneLoginUserSubject!,
                VerificationRoute = OneLoginUserVerificationRoute.Support,
                VerifiedDatesOfBirth = [data.StatedDateOfBirth],
                VerifiedNames = [[data.StatedFirstName, data.StatedLastName]]
            },
            processContext);

        await supportTaskService.UpdateSupportTaskAsync(
            new UpdateSupportTaskOptions<OneLoginUserIdVerificationData>
            {
                SupportTaskReference = SupportTaskReference,
                UpdateData = data => data with
                {
                    Verified = true,
                    Outcome = OneLoginUserIdVerificationOutcome.VerifiedOnly,
                    NotConnectingReason = JourneyInstance.State.NotConnectingReason,
                    NotConnectingAdditionalDetails = JourneyInstance.State.NotConnectingAdditionalDetails
                },
                Status = SupportTaskStatus.Closed
            },
            processContext);

        await JourneyInstance.DeleteAsync();

        TempData.SetFlashSuccess(
            "GOV.UK One Login account not connected to a record",
            $"Request closed for {data.StatedFirstName} {data.StatedLastName}.");

        return Redirect(linkGenerator.SupportTasks.OneLoginUserIdVerification.Index());
    }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        if (JourneyInstance.State.Verified is not true)
        {
            context.Result = Redirect(linkGenerator.SupportTasks.OneLoginUserIdVerification.Resolve.Index(SupportTaskReference, JourneyInstance.InstanceId));
            return;
        }

        if (JourneyInstance.State.MatchedPersonId != ResolveOneLoginUserIdVerificationState.NotMatchedPersonIdSentinel)
        {
            context.Result = Redirect(linkGenerator.SupportTasks.OneLoginUserIdVerification.Resolve.Matches(SupportTaskReference, JourneyInstance.InstanceId));
            return;
        }

        if (JourneyInstance.State.NotConnectingReason is null)
        {
            context.Result = Redirect(linkGenerator.SupportTasks.OneLoginUserIdVerification.Resolve.NotConnecting(SupportTaskReference, JourneyInstance.InstanceId));
            return;
        }

        _supportTask = context.HttpContext.GetCurrentSupportTaskFeature().SupportTask;

        EmailAddress = _supportTask.OneLoginUser!.EmailAddress!;
        Reason = JourneyInstance.State.NotConnectingReason!.Value;
        AdditionalDetails = JourneyInstance.State.NotConnectingAdditionalDetails;
    }
}
