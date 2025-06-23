using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.EditRoute;

[Journey(JourneyNames.EditRouteToProfessionalStatus), RequireJourneyInstance]
public class TrainingProviderModel(
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

    public TrainingProvider[] TrainingProviders { get; set; } = [];

    public RouteToProfessionalStatusType? RouteToProfessionalStatusType { get; set; }

    public bool TrainingProviderRequired => QuestionDriverHelper.FieldRequired(RouteToProfessionalStatusType!.TrainingProviderRequired, JourneyInstance!.State.Status.GetTrainingProviderRequirement())
            == FieldRequirement.Mandatory;
    public string PageHeading => "Enter the training provider for this route" + (!TrainingProviderRequired ? " (optional)" : "");

    [BindProperty]
    public Guid? TrainingProviderId { get; set; }

    public void OnGet()
    {
        TrainingProviderId = JourneyInstance!.State.TrainingProviderId;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (TrainingProviderRequired && TrainingProviderId is null)
        {
            ModelState.AddModelError(nameof(TrainingProviderId), "Select a training provider");
        }
        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        await JourneyInstance!.UpdateStateAsync(s => s.TrainingProviderId = TrainingProviderId);

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
        var routeInfo = context.HttpContext.GetCurrentProfessionalStatusFeature();
        RouteToProfessionalStatusType = routeInfo.RouteToProfessionalStatus.RouteToProfessionalStatusType;
        TrainingProviders = await referenceDataCache.GetTrainingProvidersAsync();
        await base.OnPageHandlerExecutionAsync(context, next);
    }
}
