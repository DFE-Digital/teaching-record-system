using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.EditRoute;

[Journey(JourneyNames.EditRouteToProfessionalStatus)]
public class CountryModel(
    EditRouteJourneyCoordinator journey,
    SupportUiLinkGenerator linkGenerator,
    ReferenceDataCache referenceDataCache) : PageModel
{
    private readonly InlineValidator<CountryModel> _validator = new()
    {
        v => v.RuleFor(m => m.TrainingCountryId)
            .NotNull().WithMessage("Enter a country")
            .When(m => m.CountryRequired)
    };

    public string PageCaption => journey.PageCaption;

    public string BackLink => journey.GetReturnUrlOrDefault(DetailUrl);

    public CountryDisplayInfo[] TrainingCountries { get; set; } = [];

    public bool CountryRequired { get; set; }


    [BindProperty]
    public bool Cancel { get; set; }

    [BindProperty]
    public string? TrainingCountryId { get; set; }


    private string DetailUrl => linkGenerator.RoutesToProfessionalStatus.EditRoute.Detail(journey.InstanceId);

    public void OnGet()
    {
        TrainingCountryId = journey.State.TrainingCountryId;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Cancel)
        {
            return Redirect(await journey.CancelAsync());
        }

        await this.ThrowIfInvalidAsync(_validator);

        journey.UpdateState(state => state.TrainingCountryId = TrainingCountryId);

        return Redirect(journey.GetReturnUrlOrDefault(DetailUrl));
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        CountryRequired = await journey.QuestionIsMandatoryAsync(EditRoutePage.Country);
        TrainingCountries = (await referenceDataCache.GetTrainingCountriesAsync())
            .Select(c => new CountryDisplayInfo()
            {
                Id = c.CountryId,
                DisplayName = $"{c.CountryId} - {c.Name}"
            })
            .ToArray();


        await base.OnPageHandlerExecutionAsync(context, next);
    }
}
