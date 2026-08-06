using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.EditRoute;

[Journey(JourneyNames.EditRouteToProfessionalStatus)]
public class StartAndEndDateModel(
    EditRouteJourneyCoordinator journey,
    SupportUiLinkGenerator linkGenerator) : PageModel
{
    private readonly InlineValidator<StartAndEndDateModel> _validator = new()
    {
        v => v.RuleFor(m => m.TrainingStartDate)
            .NotNull().WithMessage("Enter a start date")
            .When(m => m.StartAndEndDatesRequired),
        v => v.RuleFor(m => m.TrainingEndDate)
            .NotNull().WithMessage("Enter an end date")
            .When(m => m.StartAndEndDatesRequired),
        v => v.RuleFor(m => m.TrainingEndDate)
            .Must((m, endDate) => !(m.TrainingStartDate >= endDate))
                .WithMessage("End date must be after start date")
    };

    public string PageCaption => journey.PageCaption;

    public string BackLink => journey.GetReturnUrlOrDefault(DetailUrl);

    public bool StartAndEndDatesRequired { get; set; }


    [BindProperty]
    public bool Cancel { get; set; }

    [BindProperty]
    public DateOnly? TrainingStartDate { get; set; }

    [BindProperty]
    public DateOnly? TrainingEndDate { get; set; }


    private string DetailUrl => linkGenerator.RoutesToProfessionalStatus.EditRoute.Detail(journey.InstanceId);

    public void OnGet()
    {
        TrainingStartDate = journey.State.TrainingStartDate;
        TrainingEndDate = journey.State.TrainingEndDate;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Cancel)
        {
            return Redirect(await journey.CancelAsync());
        }

        await this.ThrowIfInvalidAsync(_validator);

        journey.UpdateState(state =>
        {
            state.TrainingStartDate = TrainingStartDate;
            state.TrainingEndDate = TrainingEndDate;
        });

        // The dates are asked for part way through completing a route, which carries on to the date it
        // was first held.
        if (journey.IsCompletingRoute)
        {
            return Redirect(linkGenerator.RoutesToProfessionalStatus.EditRoute.HoldsFrom(journey.InstanceId));
        }

        return Redirect(journey.GetReturnUrlOrDefault(DetailUrl));
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        StartAndEndDatesRequired = await journey.QuestionIsMandatoryAsync(EditRoutePage.StartAndEndDate);


        await base.OnPageHandlerExecutionAsync(context, next);
    }
}
