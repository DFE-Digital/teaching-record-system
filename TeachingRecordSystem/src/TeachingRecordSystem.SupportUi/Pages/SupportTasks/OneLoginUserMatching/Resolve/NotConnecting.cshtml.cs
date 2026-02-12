using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models.SupportTasks;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.OneLoginUserMatching.Resolve;

[Journey(JourneyNames.ResolveOneLoginUserMatching), RequireJourneyInstance]
public class NotConnecting(SupportUiLinkGenerator linkGenerator) : PageModel
{
    private InlineValidator<NotConnecting> _validator = new()
    {
        v => v.RuleFor(x => x.Reason)
            .NotNull().WithMessage("Select a reason"),
        v => v.RuleFor(x => x.AdditionalDetails)
            .NotNull().WithMessage("Enter additional detail")
            .MaximumLength(UiDefaults.ReasonDetailsMaxCharacterCount).WithMessage($"Additional detail must be {UiDefaults.ReasonDetailsMaxCharacterCount} characters or less")
            .When(x => x.Reason is OneLoginUserNotConnectingReason.AnotherReason)
    };

    private SupportTask? _supportTask;

    [FromRoute]
    public string SupportTaskReference { get; set; } = null!;

    public JourneyInstance<ResolveOneLoginUserMatchingState> JourneyInstance { get; set; } = null!;

    [BindProperty]
    public OneLoginUserNotConnectingReason? Reason { get; set; }

    [BindProperty]
    public string? AdditionalDetails { get; set; }

    public void OnGet()
    {
        Reason = JourneyInstance.State.NotConnectingReason;
        AdditionalDetails = JourneyInstance.State.NotConnectingAdditionalDetails;
    }

    public async Task<IActionResult> OnPostAsync(bool cancel)
    {
        if (cancel)
        {
            await JourneyInstance.DeleteAsync();

            if (_supportTask!.SupportTaskType is SupportTaskType.OneLoginUserIdVerification)
            {
                return Redirect(linkGenerator.SupportTasks.OneLoginUserMatching.IdVerification());
            }

            return Redirect(linkGenerator.SupportTasks.OneLoginUserMatching.RecordMatching());
        }

        await _validator.ValidateAndThrowAsync(this);

        await JourneyInstance.UpdateStateAsync(state =>
        {
            state.NotConnectingReason = Reason;
            state.NotConnectingAdditionalDetails = Reason is OneLoginUserNotConnectingReason.AnotherReason ? AdditionalDetails : null;
        });

        return Redirect(linkGenerator.SupportTasks.OneLoginUserMatching.Resolve.ConfirmNotConnecting(SupportTaskReference, JourneyInstance.InstanceId));
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

        _supportTask = context.HttpContext.GetCurrentSupportTaskFeature().SupportTask;
    }
}
