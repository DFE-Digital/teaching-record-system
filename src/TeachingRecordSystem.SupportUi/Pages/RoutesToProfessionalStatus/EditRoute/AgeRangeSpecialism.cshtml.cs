using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.EditRoute;

[Journey(JourneyNames.EditRouteToProfessionalStatus)]
public class AgeRangeSpecialismModel(
    EditRouteJourneyCoordinator journey,
    SupportUiLinkGenerator linkGenerator) : PageModel
{
    private readonly InlineValidator<AgeRangeSpecialismModel> _validator = new()
    {
        v => v.RuleFor(m => m.TrainingAgeSpecialism.AgeRangeType)
            .NotNull().WithMessage("Enter an age range specialism")
            .When(m => m.AgeRangeSpecialismRequired)
    };

    public string PageCaption => journey.PageCaption;

    public string BackLink => journey.GetReturnUrlOrDefault(DetailUrl);

    public bool AgeRangeSpecialismRequired { get; set; }


    [BindProperty]
    public bool Cancel { get; set; }

    [BindProperty]
    public AgeRange TrainingAgeSpecialism { get; set; } = new();


    private string DetailUrl => linkGenerator.RoutesToProfessionalStatus.EditRoute.Detail(journey.InstanceId);

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

        journey.UpdateState(state =>
        {
            state.TrainingAgeSpecialismRangeFrom = TrainingAgeSpecialism.AgeRangeFrom;
            state.TrainingAgeSpecialismRangeTo = TrainingAgeSpecialism.AgeRangeTo;
            state.TrainingAgeSpecialismType = TrainingAgeSpecialism.AgeRangeType;
        });

        return Redirect(journey.GetReturnUrlOrDefault(DetailUrl));
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        AgeRangeSpecialismRequired = await journey.QuestionIsMandatoryAsync(EditRoutePage.AgeRangeSpecialism);


        await base.OnPageHandlerExecutionAsync(context, next);
    }
}
