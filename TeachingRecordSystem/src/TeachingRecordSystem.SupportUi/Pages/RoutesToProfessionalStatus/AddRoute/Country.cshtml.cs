using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.AddRoute;

[Journey(JourneyNames.AddRouteToProfessionalStatus), RequireJourneyInstance]
public class CountryModel(TrsLinkGenerator linkGenerator, ReferenceDataCache referenceDataCache)
    : AddRoutePostStatusPageModel(AddRoutePage.Country, linkGenerator, referenceDataCache)
{
    public CountryDisplayInfo[] TrainingCountries { get; set; } = [];

    public string PageHeading => "Enter the country associated with their route" + (!CountryRequired ? " (optional)" : "");
    public bool CountryRequired => QuestionDriverHelper.FieldRequired(Route.TrainingCountryRequired, Status.GetCountryRequirement())
        == FieldRequirement.Mandatory;

    [BindProperty]
    public string? TrainingCountryId { get; set; }

    public void OnGet()
    {
        TrainingCountryId = JourneyInstance!.State.TrainingCountryId;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (TrainingCountryId is null && CountryRequired)
        {
            ModelState.AddModelError("TrainingCountryId", "Enter a country");
        }

        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        await JourneyInstance!.UpdateStateAsync(state =>
        {
            state.TrainingCountryId = TrainingCountryId;
        });

        return await ContinueAsync();
    }

    public override async Task OnPageHandlerExecutingAsync(PageHandlerExecutingContext context)
    {
        await base.OnPageHandlerExecutingAsync(context);

        TrainingCountries = (await ReferenceDataCache.GetTrainingCountriesAsync())
            .Select(r => new CountryDisplayInfo()
            {
                Id = r.CountryId,
                DisplayName = $"{r.CountryId} - {r.Name}"
            })
            .ToArray();
    }
}
