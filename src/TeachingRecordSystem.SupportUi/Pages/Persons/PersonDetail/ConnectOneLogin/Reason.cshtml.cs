using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.ConnectOneLogin;

[Journey(JourneyNames.ConnectOneLogin), RequireJourneyInstance]
public class ReasonModel(SupportUiLinkGenerator linkGenerator) : PageModel
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

    public JourneyInstance<ConnectOneLoginState>? JourneyInstance { get; set; }

    [FromRoute]
    public Guid PersonId { get; set; }

    [FromQuery]
    public bool FromCheckAnswers { get; set; }

    public string? OneLoginEmailAddress { get; set; }

    [BindProperty]
    public ConnectOneLoginReason? ConnectReason { get; set; }

    [BindProperty]
    public string? ReasonDetail { get; set; }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        OneLoginEmailAddress = JourneyInstance!.State.OneLoginEmailAddress;
    }

    public void OnGet()
    {
        ConnectReason = JourneyInstance!.State.ConnectReason;
        ReasonDetail = JourneyInstance.State.ReasonDetail;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await _validator.ValidateAndThrowAsync(this);

        await JourneyInstance!.UpdateStateAsync(state =>
        {
            state.ConnectReason = ConnectReason;
            state.ReasonDetail = ReasonDetail;
        });

        return Redirect(linkGenerator.Persons.PersonDetail.ConnectOneLogin.CheckAnswers(PersonId, JourneyInstance.InstanceId));
    }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await JourneyInstance!.DeleteAsync();
        return Redirect(linkGenerator.Persons.PersonDetail.Index(PersonId));
    }
}

