using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.AddRoute;

[Journey(JourneyNames.AddRouteToProfessionalStatus), RequireJourneyInstance]
public class TrainingProviderModel(TrsLinkGenerator linkGenerator, ReferenceDataCache referenceDataCache) : AddRouteCommonPageModel(linkGenerator, referenceDataCache)
{
    protected override RoutePage CurrentPage => RoutePage.TrainingProvider;

    public string BackLink => PreviousPageUrl;

    public TrainingProvider[] TrainingProviders { get; set; } = [];

    public string PageHeading => "Enter the training provider for this route" + (!TrainingProviderRequired ? " (optional)" : "");
    public bool TrainingProviderRequired => QuestionDriverHelper.FieldRequired(Route.TrainingProviderRequired, Status.GetTrainingProviderRequirement())
        == FieldRequirement.Mandatory;

    [BindProperty]
    public Guid? TrainingProviderId { get; set; }

    public void OnGet()
    {
        TrainingProviderId = JourneyInstance!.State.NewRouteToProfessionalStatusId != null ? JourneyInstance!.State.NewTrainingProviderId : JourneyInstance!.State.TrainingProviderId;
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

        await JourneyInstance!.UpdateStateAsync(s =>
        {
            if (JourneyInstance!.State.NewRouteToProfessionalStatusId == null)
            {
                s.Begin();
            }

            s.NewTrainingProviderId = TrainingProviderId;
        });

        return await ContinueAsync();
    }

    protected override async Task OnPageHandlerExecutingAsync(PageHandlerExecutingContext context)
    {
        TrainingProviders = await ReferenceDataCache.GetTrainingProvidersAsync();
    }

}
