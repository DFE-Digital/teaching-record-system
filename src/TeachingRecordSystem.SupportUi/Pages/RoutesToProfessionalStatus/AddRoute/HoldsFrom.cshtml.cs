using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.AddRoute;

[Journey(JourneyNames.AddRouteToProfessionalStatus)]
public class HoldsFromModel(AddRouteJourneyCoordinator journey, TimeProvider timeProvider) : PageModel
{
    private readonly InlineValidator<HoldsFromModel> _validator = new()
    {
        v => v.RuleFor(m => m.HoldsFrom)
            .NotNull().WithMessage("Enter the date they first held this professional status")
            .When(m => m.HoldsFromRequired),
        v => v.RuleFor(m => m.HoldsFrom)
            .Must(holdsFrom => !(holdsFrom > timeProvider.Today))
                .WithMessage("The date they first held this professional status must not be in the future")
    };

    public string PageCaption => journey.PageCaption;

    public string? BackLink { get; set; }

    [BindProperty]
    public bool Cancel { get; set; }

    [BindProperty]
    public DateOnly? HoldsFrom { get; set; }

    public bool HoldsFromRequired { get; set; }

    public void OnGet()
    {
        HoldsFrom = journey.State.HoldsFrom;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Cancel)
        {
            return Redirect(await journey.CancelAsync());
        }

        await this.ThrowIfInvalidAsync(_validator);

        return await journey.AnswerAndAdvanceAsync(AddRoutePage.HoldsFrom, state => state.HoldsFrom = HoldsFrom);
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        HoldsFromRequired = await journey.QuestionIsMandatoryAsync(AddRoutePage.HoldsFrom);

        BackLink = journey.GetBackLink();

        await base.OnPageHandlerExecutionAsync(context, next);
    }
}
