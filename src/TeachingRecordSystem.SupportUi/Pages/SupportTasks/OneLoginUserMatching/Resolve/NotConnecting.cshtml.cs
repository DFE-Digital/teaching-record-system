using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.Models.SupportTasks;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.OneLoginUserMatching.Resolve;

[Journey(JourneyNames.ResolveOneLoginUserMatching)]
public class NotConnecting(
    ResolveOneLoginUserMatchingJourneyCoordinator journey,
    SupportUiLinkGenerator linkGenerator) : PageModel
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

    [BindProperty]
    public OneLoginUserNotConnectingReason? Reason { get; set; }

    [BindProperty]
    public string? AdditionalDetails { get; set; }

    [BindProperty]
    public bool Cancel { get; set; }

    public string? BackLink { get; set; }

    public void OnGet()
    {
        Reason = journey.State.NotConnectingReason;
        AdditionalDetails = journey.State.NotConnectingAdditionalDetails;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Cancel)
        {
            journey.DeleteInstance();

            return Redirect(journey.GetListPageUrl());
        }

        await _validator.ValidateAndThrowAsync(this);

        return journey.AdvanceTo(
            linkGenerator.SupportTasks.OneLoginUserMatching.Resolve.ConfirmNotConnecting(journey.InstanceId),
            state =>
            {
                state.NotConnectingReason = Reason;
                state.NotConnectingAdditionalDetails = Reason is OneLoginUserNotConnectingReason.AnotherReason ? AdditionalDetails : null;
            });
    }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        BackLink = journey.GetBackLink();
    }
}
