using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.SupportUi.Infrastructure.Filters;

namespace TeachingRecordSystem.SupportUi.Pages.OneLogins.OneLoginDetail.ConnectPerson;

[Journey(JourneyNames.ConnectPerson)]
[TypeFilter(typeof(CheckOneLoginUserExistsFilterFactory))]
public class ReasonModel(
    ConnectPersonJourneyCoordinator journey,
    SupportUiLinkGenerator linkGenerator) : PageModel
{
    private readonly InlineValidator<ReasonModel> _validator = new()
    {
        v => v.RuleFor(m => m.ConnectReason)
            .NotNull().WithMessage("Select a reason"),
        v => v.RuleFor(m => m.ReasonDetail)
            .NotEmpty().WithMessage("Enter details")
            .MaximumLength(UiDefaults.ReasonDetailsMaxCharacterCount).WithMessage($"Details must be {UiDefaults.ReasonDetailsMaxCharacterCount} characters or less")
            .When(m => m.ConnectReason == ConnectPersonReason.AnotherReason)
    };

    [FromRoute]
    public string OneLoginUserSubject { get; set; } = null!;

    [BindProperty]
    public bool Cancel { get; set; }

    public string? BackLink { get; set; }

    public string? PersonTrn { get; set; }

    [BindProperty]
    public ConnectPersonReason? ConnectReason { get; set; }

    [BindProperty]
    public string? ReasonDetail { get; set; }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        BackLink = journey.GetBackLink();

        PersonTrn = journey.State.PersonTrn;
    }

    public void OnGet()
    {
        ConnectReason = journey.State.ConnectReason;
        ReasonDetail = journey.State.ReasonDetail;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Cancel)
        {
            journey.DeleteInstance();
            return Redirect(linkGenerator.OneLogins.OneLoginDetail.Index(OneLoginUserSubject));
        }

        await _validator.ValidateAndThrowAsync(this);

        return journey.AdvanceTo(
            linkGenerator.OneLogins.OneLoginDetail.ConnectPerson.CheckAnswers(journey.InstanceId),
            state =>
            {
                state.ConnectReason = ConnectReason;
                state.ReasonDetail = ReasonDetail;
            });
    }
}
