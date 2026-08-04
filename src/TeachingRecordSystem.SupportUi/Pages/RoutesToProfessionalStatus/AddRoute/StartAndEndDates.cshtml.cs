using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.AddRoute;

[Journey(JourneyNames.AddRouteToProfessionalStatus)]
public class StartAndEndDateModel(AddRouteJourneyCoordinator journey) : PageModel
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

    public string? BackLink { get; set; }

    [BindProperty]
    public bool Cancel { get; set; }

    [BindProperty]
    public DateOnly? TrainingStartDate { get; set; }

    [BindProperty]
    public DateOnly? TrainingEndDate { get; set; }

    public bool StartAndEndDatesRequired { get; set; }

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

        return await journey.AnswerAndAdvanceAsync(AddRoutePage.StartAndEndDate, state =>
        {
            state.TrainingStartDate = TrainingStartDate;
            state.TrainingEndDate = TrainingEndDate;
        });
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        StartAndEndDatesRequired = await journey.QuestionIsMandatoryAsync(AddRoutePage.StartAndEndDate);

        BackLink = journey.GetBackLink();

        await base.OnPageHandlerExecutionAsync(context, next);
    }
}
