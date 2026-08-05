using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.EditRoute;

[Journey(JourneyNames.EditRouteToProfessionalStatus)]
public class TrainingProviderModel(
    EditRouteJourneyCoordinator journey,
    SupportUiLinkGenerator linkGenerator,
    ReferenceDataCache referenceDataCache) : PageModel
{
    private readonly InlineValidator<TrainingProviderModel> _validator = new()
    {
        v => v.RuleFor(m => m.TrainingProviderId)
            .NotNull().WithMessage("Select a training provider")
            .When(m => m.TrainingProviderRequired)
    };

    public string PageCaption => journey.PageCaption;

    public string BackLink => journey.GetReturnUrlOrDefault(DetailUrl);

    public TrainingProvider[] TrainingProviders { get; set; } = [];

    public bool TrainingProviderRequired { get; set; }


    [BindProperty]
    public bool Cancel { get; set; }

    [BindProperty]
    public Guid? TrainingProviderId { get; set; }


    private string DetailUrl => linkGenerator.RoutesToProfessionalStatus.EditRoute.Detail(journey.InstanceId);

    public void OnGet()
    {
        TrainingProviderId = journey.State.TrainingProviderId;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Cancel)
        {
            return Redirect(await journey.CancelAsync());
        }

        await this.ThrowIfInvalidAsync(_validator);

        journey.UpdateState(state => state.TrainingProviderId = TrainingProviderId);

        return Redirect(journey.GetReturnUrlOrDefault(DetailUrl));
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        TrainingProviderRequired = await journey.QuestionIsMandatoryAsync(EditRoutePage.TrainingProvider);
        TrainingProviders = await referenceDataCache.GetTrainingProvidersAsync();


        await base.OnPageHandlerExecutionAsync(context, next);
    }
}
