using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.Models.SupportTasks;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.OneLoginUserIdVerification.Resolve;

[Journey(JourneyNames.ResolveOneLoginUserIdVerification), RequireJourneyInstance]
public class Reject(SupportUiLinkGenerator linkGenerator) : PageModel
{
    private readonly InlineValidator<Reject> _validator = new()
    {
        v => v.RuleFor(m => m.Reason)
            .NotNull().WithMessage("Select a reason"),
        v => v.RuleFor(m => m.AdditionalDetails)
            .NotNull().WithMessage("Enter additional detail")
            .MaximumLength(UiDefaults.ReasonDetailsMaxCharacterCount).WithMessage($"Additional detail must be {UiDefaults.ReasonDetailsMaxCharacterCount} characters or less")
            .When(m => m.Reason is OneLoginIdVerificationRejectReason.AnotherReason)
    };

    [FromRoute]
    public string SupportTaskReference { get; set; } = null!;

    [FromQuery]
    public bool FromCheckAnswers { get; set; }

    public JourneyInstance<ResolveOneLoginUserIdVerificationState> JourneyInstance { get; set; } = null!;

    [BindProperty]
    public OneLoginIdVerificationRejectReason? Reason { get; set; }

    [BindProperty]
    public string? AdditionalDetails { get; set; }

    public void OnGet()
    {
        Reason = JourneyInstance.State.RejectReason;
        AdditionalDetails = JourneyInstance.State.RejectionAdditionalDetails;
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
            state.RejectReason = Reason;
            state.RejectionAdditionalDetails = Reason is OneLoginIdVerificationRejectReason.AnotherReason ? AdditionalDetails : null;
        });

        return Redirect(linkGenerator.SupportTasks.OneLoginUserIdVerification.Resolve.ConfirmReject(SupportTaskReference, JourneyInstance.InstanceId));
    }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        if (JourneyInstance.State.Verified is not false)
        {
            context.Result = Redirect(linkGenerator.SupportTasks.OneLoginUserIdVerification.Resolve.Index(SupportTaskReference, JourneyInstance.InstanceId));
        }
    }
}
