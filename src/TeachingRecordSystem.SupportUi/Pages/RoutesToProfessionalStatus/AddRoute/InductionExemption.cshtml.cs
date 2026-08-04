using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.AddRoute;

[Journey(JourneyNames.AddRouteToProfessionalStatus)]
public class InductionExemptionModel(AddRouteJourneyCoordinator journey) : PageModel
{
    private readonly InlineValidator<InductionExemptionModel> _validator = new()
    {
        v => v.RuleFor(m => m.IsExemptFromInduction)
            .NotNull().WithMessage("Select yes if this route provides an induction exemption")
    };

    public string PageCaption => journey.PageCaption;

    public string? BackLink { get; set; }

    [BindProperty]
    public bool Cancel { get; set; }

    [BindProperty]
    public bool? IsExemptFromInduction { get; set; }

    public void OnGet()
    {
        IsExemptFromInduction = journey.State.IsExemptFromInduction;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Cancel)
        {
            return Redirect(await journey.CancelAsync());
        }

        await this.ThrowIfInvalidAsync(_validator);

        return await journey.AnswerAndAdvanceAsync(AddRoutePage.InductionExemption, state => state.IsExemptFromInduction = IsExemptFromInduction);
    }

    public override Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        BackLink = journey.GetBackLink();

        return base.OnPageHandlerExecutionAsync(context, next);
    }
}
