using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.SupportUi.Infrastructure.Filters;

namespace TeachingRecordSystem.SupportUi.Pages.OneLogins.OneLoginDetail.ConnectPerson;

[Journey(JourneyNames.ConnectPerson), RequireJourneyInstance]
[TypeFilter(typeof(CheckOneLoginUserExistsFilterFactory))]
public class ReasonModel(SupportUiLinkGenerator linkGenerator) : PageModel
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

    public JourneyInstance<ConnectPersonState>? JourneyInstance { get; set; }

    [FromRoute]
    public string OneLoginUserSubject { get; set; } = null!;

    [FromQuery]
    public bool FromCheckAnswers { get; set; }

    public string? PersonTrn { get; set; }

    [BindProperty]
    public ConnectPersonReason? ConnectReason { get; set; }

    [BindProperty]
    public string? ReasonDetail { get; set; }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        if (!JourneyInstance!.State.PersonId.HasValue)
        {
            context.Result = Redirect(linkGenerator.OneLogins.OneLoginDetail.ConnectPerson.Index(OneLoginUserSubject, JourneyInstance.InstanceId));
            return;
        }

        PersonTrn = JourneyInstance.State.PersonTrn;
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

        return Redirect(linkGenerator.OneLogins.OneLoginDetail.ConnectPerson.CheckAnswers(OneLoginUserSubject, JourneyInstance.InstanceId));
    }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await JourneyInstance!.DeleteAsync();
        return Redirect(linkGenerator.OneLogins.OneLoginDetail.Index(OneLoginUserSubject));
    }
}
