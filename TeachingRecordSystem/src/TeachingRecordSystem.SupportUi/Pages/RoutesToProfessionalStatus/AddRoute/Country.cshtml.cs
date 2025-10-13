using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.AddRoute;

[Journey(JourneyNames.AddRouteToProfessionalStatus), RequireJourneyInstance]
public class CountryModel(
    TrsLinkGenerator linkGenerator,
    ReferenceDataCache referenceDataCache,
    EvidenceUploadManager evidenceController)
    : AddRoutePostStatusPageModel(AddRoutePage.Country, linkGenerator, referenceDataCache, evidenceController)
{
    public CountryDisplayInfo[] TrainingCountries { get; set; } = [];

    [BindProperty]
    public string? TrainingCountryId { get; set; }

    public bool CountryRequired => QuestionDriverHelper.FieldRequired(RouteType.TrainingCountryRequired, Status.GetCountryRequirement())
        == FieldRequirement.Mandatory;

    public string PageHeading => "Enter the country associated with their route"
       + (CountryRequired ? "" : " (optional)");

    public void OnGet()
    {
        TrainingCountryId = JourneyInstance!.State.TrainingCountryId;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (CountryRequired && TrainingCountryId is null)
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
