using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.ConnectOneLogin;

[Journey(JourneyNames.ConnectOneLogin)]
public class ReasonModel(
    ConnectOneLoginJourneyCoordinator journey,
    SupportUiLinkGenerator linkGenerator) : PageModel
{
    private readonly InlineValidator<ReasonModel> _validator = new()
    {
        v => v.RuleFor(m => m.ConnectReason)
            .NotNull().WithMessage("Select a reason"),
        v => v.RuleFor(m => m.ReasonDetail)
            .NotEmpty().WithMessage("Enter details")
            .MaximumLength(4000).WithMessage("Details must be 4000 characters or less")
            .When(m => m.ConnectReason == ConnectOneLoginReason.AnotherReason)
    };

    [FromRoute]
    public Guid PersonId { get; set; }

    [BindProperty]
    public bool Cancel { get; set; }

    public string? BackLink { get; set; }

    public string? OneLoginEmailAddress { get; set; }

    [BindProperty]
    public ConnectOneLoginReason? ConnectReason { get; set; }

    [BindProperty]
    public string? ReasonDetail { get; set; }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        BackLink = journey.GetBackLink();

        OneLoginEmailAddress = journey.State.OneLoginEmailAddress;
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
            return Redirect(linkGenerator.Persons.PersonDetail.Index(PersonId));
        }

        await _validator.ValidateAndThrowAsync(this);

        return journey.AdvanceTo(
            linkGenerator.Persons.PersonDetail.ConnectOneLogin.CheckAnswers(journey.InstanceId),
            state =>
            {
                state.ConnectReason = ConnectReason;
                state.ReasonDetail = ReasonDetail;
            });
    }
}

