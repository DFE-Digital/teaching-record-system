using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.AddRoute;

[Journey(JourneyNames.AddRouteToProfessionalStatus), RequireJourneyInstance]
public class CountryModel(TrsLinkGenerator linkGenerator, ReferenceDataCache referenceDataCache)
    : AddRouteCommonPageModel(linkGenerator, referenceDataCache)
{
    public string BackLink => FromCheckAnswers ?
        LinkGenerator.RouteAddCheckYourAnswers(PersonId, JourneyInstance!.InstanceId) :
        LinkGenerator.RouteAddPage(PreviousPage(AddRoutePage.Country) ?? AddRoutePage.Status, PersonId, JourneyInstance!.InstanceId);

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

        await JourneyInstance!.UpdateStateAsync(s => s.TrainingCountryId = TrainingCountryId);

        return Redirect(FromCheckAnswers ?
            LinkGenerator.RouteAddCheckYourAnswers(PersonId, JourneyInstance.InstanceId) :
            LinkGenerator.RouteAddPage(NextPage(AddRoutePage.Country) ?? AddRoutePage.Status, PersonId, JourneyInstance!.InstanceId));
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        TrainingCountries = (await ReferenceDataCache.GetTrainingCountriesAsync())
            .Select(r => new CountryDisplayInfo()
            {
                Id = r.CountryId,
                DisplayName = $"{r.CountryId} - {r.Name}"
            })
            .ToArray();
        await base.OnPageHandlerExecutionAsync(context, next);
    }
}
