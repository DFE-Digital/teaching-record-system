using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.AddRoute;

[Journey(JourneyNames.AddRouteToProfessionalStatus)]
public class CountryModel(AddRouteJourneyCoordinator journey, ReferenceDataCache referenceDataCache) : PageModel
{
    private readonly InlineValidator<CountryModel> _validator = new()
    {
        v => v.RuleFor(m => m.TrainingCountryId)
            .NotNull().WithMessage("Enter a country")
            .When(m => m.CountryRequired)
    };

    public string PageCaption => journey.PageCaption;

    public string? BackLink { get; set; }

    public CountryDisplayInfo[] TrainingCountries { get; set; } = [];

    public bool CountryRequired { get; set; }

    [BindProperty]
    public bool Cancel { get; set; }

    [BindProperty]
    public string? TrainingCountryId { get; set; }

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

        return await journey.AnswerAndAdvanceAsync(AddRoutePage.Country, state => state.TrainingCountryId = TrainingCountryId);
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        CountryRequired = await journey.QuestionIsMandatoryAsync(AddRoutePage.Country);
        TrainingCountries = (await referenceDataCache.GetTrainingCountriesAsync())
            .Select(r => new CountryDisplayInfo()
            {
                Id = r.CountryId,
                DisplayName = $"{r.CountryId} - {r.Name}"
            })
            .ToArray();

        BackLink = journey.GetBackLink();

        await base.OnPageHandlerExecutionAsync(context, next);
    }
}
