using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.Models.SupportTasks;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.OneLoginUserMatching.Resolve;

[Journey(JourneyNames.ResolveOneLoginUserMatching)]
public class Reject(
    ResolveOneLoginUserMatchingJourneyCoordinator journey,
    SupportUiLinkGenerator linkGenerator) : PageModel
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

    [BindProperty]
    public OneLoginIdVerificationRejectReason? Reason { get; set; }

    [BindProperty]
    public string? AdditionalDetails { get; set; }

    [BindProperty]
    public bool Cancel { get; set; }

    public string? BackLink { get; set; }

    public void OnGet()
    {
        Reason = journey.State.RejectReason;
        AdditionalDetails = journey.State.RejectionAdditionalDetails;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Cancel)
        {
            journey.DeleteInstance();

            return Redirect(linkGenerator.SupportTasks.OneLoginUserMatching.IdVerification());
        }

        await _validator.ValidateAndThrowAsync(this);

        return journey.AdvanceTo(
            linkGenerator.SupportTasks.OneLoginUserMatching.Resolve.ConfirmReject(journey.InstanceId),
            state =>
            {
                state.RejectReason = Reason;
                state.RejectionAdditionalDetails = Reason is OneLoginIdVerificationRejectReason.AnotherReason ? AdditionalDetails : null;
            });
    }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        BackLink = journey.GetBackLink();
    }
}
