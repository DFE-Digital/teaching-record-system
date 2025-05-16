using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.AddRoute;

[Journey(JourneyNames.AddRouteToProfessionalStatus), RequireJourneyInstance]
public class CountryModel(TrsLinkGenerator linkGenerator, ReferenceDataCache referenceDataCache)
    : AddRouteCommonPageModel(linkGenerator, referenceDataCache)
{
    public string BackLink => FromCheckAnswers ?
        LinkGenerator.RouteAddCheckYourAnswers(PersonId, JourneyInstance!.InstanceId) :
        LinkGenerator.RouteAddPage(PreviousPage(AddRoutePage.Country) ?? AddRoutePage.Status, PersonId, JourneyInstance!.InstanceId);

    public Country[] TrainingCountries { get; set; } = [];

    [BindProperty]
    [Required(ErrorMessage = "Enter a country")]
    [Display(Name = "Enter the country associated with their route")]
    public string? TrainingCountryId { get; set; }

    public void OnGet()
    {
        TrainingCountryId = JourneyInstance!.State.TrainingCountryId;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        await JourneyInstance!.UpdateStateAsync(s => s.TrainingCountryId = TrainingCountryId);

        return Redirect(FromCheckAnswers ?
            _linkGenerator.RouteAddCheckYourAnswers(PersonId, JourneyInstance.InstanceId) :
            _linkGenerator.RouteAddPage(NextPage(AddRoutePage.Country) ?? AddRoutePage.Status, PersonId, JourneyInstance!.InstanceId));
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        TrainingCountries = await ReferenceDataCache.GetTrainingCountriesAsync();
        await base.OnPageHandlerExecutionAsync(context, next);
    }
}
