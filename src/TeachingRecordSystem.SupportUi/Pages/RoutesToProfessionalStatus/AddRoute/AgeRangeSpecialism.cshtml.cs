using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.AddRoute;

[Journey(JourneyNames.AddRouteToProfessionalStatus)]
public class AgeRangeSpecialismModel(AddRouteJourneyCoordinator journey) : PageModel
{
    private readonly InlineValidator<AgeRangeSpecialismModel> _validator = new()
    {
        v => v.RuleFor(m => m.TrainingAgeSpecialism.AgeRangeType)
            .NotNull().WithMessage("Enter an age range specialism")
            .When(m => m.AgeRangeSpecialismRequired)
    };

    public string PageCaption => journey.PageCaption;

    public string? BackLink { get; set; }

    [BindProperty]
    public bool Cancel { get; set; }

    [BindProperty]
    public AgeRange TrainingAgeSpecialism { get; set; } = new();

    public bool AgeRangeSpecialismRequired { get; set; }

    public void OnGet()
    {
        TrainingAgeSpecialism = new AgeRange
        {
            AgeRangeFrom = journey.State.TrainingAgeSpecialismRangeFrom,
            AgeRangeTo = journey.State.TrainingAgeSpecialismRangeTo,
            AgeRangeType = journey.State.TrainingAgeSpecialismType
        };
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Cancel)
        {
            return Redirect(await journey.CancelAsync());
        }

        await this.ThrowIfInvalidAsync(_validator);

        return await journey.AnswerAndAdvanceAsync(AddRoutePage.AgeRangeSpecialism, state =>
        {
            state.TrainingAgeSpecialismRangeFrom = TrainingAgeSpecialism.AgeRangeFrom;
            state.TrainingAgeSpecialismRangeTo = TrainingAgeSpecialism.AgeRangeTo;
            state.TrainingAgeSpecialismType = TrainingAgeSpecialism.AgeRangeType;
        });
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        AgeRangeSpecialismRequired = await journey.QuestionIsMandatoryAsync(AddRoutePage.AgeRangeSpecialism);

        BackLink = journey.GetBackLink();

        await base.OnPageHandlerExecutionAsync(context, next);
    }
}
