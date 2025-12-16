using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.Models.SupportTasks;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.OneLoginUserIdVerification.Resolve;

[Journey(JourneyNames.ResolveOneLoginUserIdVerification), RequireJourneyInstance]
public class NotConnecting(SupportUiLinkGenerator linkGenerator) : PageModel
{
    private InlineValidator<NotConnecting> _validator = new()
    {
        v => v.RuleFor(x => x.Reason)
            .NotNull().WithMessage("Select a reason"),
        v => v.RuleFor(x => x.AdditionalDetails)
            .NotNull().WithMessage("Enter additional detail")
            .MaximumLength(UiDefaults.DetailMaxCharacterCount).WithMessage($"Additional detail must be {UiDefaults.DetailMaxCharacterCount} characters or less")
            .When(x => x.Reason is OneLoginIdVerificationNotConnectingReason.AnotherReason)
    };

    [FromRoute]
    public string SupportTaskReference { get; set; } = null!;

    public JourneyInstance<ResolveOneLoginUserIdVerificationState> JourneyInstance { get; set; } = null!;

    [BindProperty]
    public OneLoginIdVerificationNotConnectingReason? Reason { get; set; }

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

            return Redirect(linkGenerator.SupportTasks.OneLoginUserIdVerification.Index());
        }

        await _validator.ValidateAndThrowAsync(this);

        await JourneyInstance.UpdateStateAsync(state =>
        {
            state.NotConnectingReason = Reason;
            state.NotConnectingAdditionalDetails = Reason is OneLoginIdVerificationNotConnectingReason.AnotherReason ? AdditionalDetails : null;
        });

        return Redirect(linkGenerator.SupportTasks.OneLoginUserIdVerification.Resolve.ConfirmNotConnecting(SupportTaskReference, JourneyInstance.InstanceId));
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
    }
}
