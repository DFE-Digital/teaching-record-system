using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.EditRoute;

[Journey(JourneyNames.EditRouteToProfessionalStatus), RequireJourneyInstance]
public class CountryModel(
    SupportUiLinkGenerator linkGenerator,
    ReferenceDataCache referenceDataCache,
    EvidenceUploadManager evidenceController)
    : EditRouteCommonPageModel(linkGenerator, referenceDataCache, evidenceController)
{
    public CountryDisplayInfo[] TrainingCountries { get; set; } = [];

    [BindProperty]
    [Display(Name = "Enter the country associated with their route")]
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

        await JourneyInstance!.UpdateStateAsync(s => s.TrainingCountryId = TrainingCountryId);

        return Redirect(FromCheckAnswers ?
            LinkGenerator.RoutesToProfessionalStatus.EditRoute.CheckAnswers(QualificationId, JourneyInstance.InstanceId) :
            LinkGenerator.RoutesToProfessionalStatus.EditRoute.Detail(QualificationId, JourneyInstance.InstanceId));
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
