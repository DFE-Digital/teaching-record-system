using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.EditRoute;

[Journey(JourneyNames.EditRouteToProfessionalStatus), RequireJourneyInstance]
public class TrainingProviderModel(
    TrsLinkGenerator linkGenerator,
    ReferenceDataCache referenceDataCache)
    : EditRouteCommonPageModel(linkGenerator, referenceDataCache)
{
    public TrainingProvider[] TrainingProviders { get; set; } = [];

    [BindProperty]
    public Guid? TrainingProviderId { get; set; }

    public bool TrainingProviderRequired => QuestionDriverHelper.FieldRequired(RouteType!.TrainingProviderRequired, Status.GetTrainingProviderRequirement())
            == FieldRequirement.Mandatory;

    public string PageHeading => "Enter the training provider for this route"
        + (TrainingProviderRequired ? "" : " (optional)");

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
            LinkGenerator.RouteEditCheckYourAnswers(QualificationId, JourneyInstance.InstanceId) :
            LinkGenerator.RouteEditDetail(QualificationId, JourneyInstance.InstanceId));
    }

    public override async Task OnPageHandlerExecutingAsync(PageHandlerExecutingContext context)
    {
        await base.OnPageHandlerExecutingAsync(context);

        TrainingProviders = await ReferenceDataCache.GetTrainingProvidersAsync();
    }
}
