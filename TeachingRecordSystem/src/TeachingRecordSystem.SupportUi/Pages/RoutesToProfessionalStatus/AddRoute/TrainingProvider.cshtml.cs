using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.AddRoute;

[Journey(JourneyNames.AddRouteToProfessionalStatus), RequireJourneyInstance]
public class TrainingProviderModel(
    TrsLinkGenerator linkGenerator,
    ReferenceDataCache referenceDataCache,
    EvidenceUploadManager evidenceController)
    : AddRoutePostStatusPageModel(AddRoutePage.TrainingProvider, linkGenerator, referenceDataCache, evidenceController)
{
    public TrainingProvider[] TrainingProviders { get; set; } = [];

    public bool TrainingProviderRequired => QuestionDriverHelper.FieldRequired(RouteType.TrainingProviderRequired, Status.GetTrainingProviderRequirement())
        == FieldRequirement.Mandatory;

    public string PageHeading => "Enter the training provider for this route"
        + (TrainingProviderRequired ? "" : " (optional)");

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

        await JourneyInstance!.UpdateStateAsync(state =>
        {
            state.TrainingProviderId = TrainingProviderId;
        });

        return await ContinueAsync();
    }

    public override async Task OnPageHandlerExecutingAsync(PageHandlerExecutingContext context)
    {
        await base.OnPageHandlerExecutingAsync(context);

        TrainingProviders = await ReferenceDataCache.GetTrainingProvidersAsync();
    }
}
