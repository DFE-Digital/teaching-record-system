using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.EditRoute;

[Journey(JourneyNames.EditRouteToProfessionalStatus), RequireJourneyInstance]
public class CountryModel(
    TrsLinkGenerator linkGenerator,
    ReferenceDataCache referenceDataCache) : PageModel
{
    public JourneyInstance<EditRouteState>? JourneyInstance { get; set; }

    [FromQuery]
    public bool FromCheckAnswers { get; set; }

    [FromRoute]
    public Guid QualificationId { get; set; }

    public string? PersonName { get; set; }

    public Guid PersonId { get; set; }

    public CountryDisplayInfo[] TrainingCountries { get; set; } = [];

    public RouteToProfessionalStatusType Route { get; set; } = null!;

    public RouteToProfessionalStatusStatus Status { get; set; }

    public string PageHeading => "Enter the country associated with their route" + (!CountryRequired ? " (optional)" : "");
    public bool CountryRequired => QuestionDriverHelper.FieldRequired(Route.TrainingCountryRequired, Status.GetCountryRequirement())
        == FieldRequirement.Mandatory;

    [BindProperty]
    [Display(Name = "Enter the country associated with their route")]
    public string? TrainingCountryId { get; set; }

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
            linkGenerator.RouteEditCheckYourAnswers(QualificationId, JourneyInstance.InstanceId) :
            linkGenerator.RouteEditDetail(QualificationId, JourneyInstance.InstanceId));
    }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await JourneyInstance!.DeleteAsync();
        return Redirect(linkGenerator.PersonQualifications(PersonId));
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        var personInfo = context.HttpContext.GetCurrentPersonFeature();
        PersonName = personInfo.Name;
        PersonId = personInfo.PersonId;

        Route = await referenceDataCache.GetRouteToProfessionalStatusTypeByIdAsync(JourneyInstance!.State.RouteToProfessionalStatusId);
        Status = JourneyInstance!.State.Status;

        TrainingCountries = (await referenceDataCache.GetTrainingCountriesAsync())
            .Select(r => new CountryDisplayInfo()
            {
                Id = r.CountryId,
                DisplayName = $"{r.CountryId} - {r.Name}"
            })
            .ToArray();

        await base.OnPageHandlerExecutionAsync(context, next);
    }
}
