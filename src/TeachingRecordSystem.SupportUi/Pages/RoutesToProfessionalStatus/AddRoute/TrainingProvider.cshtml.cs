using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.AddRoute;

[Journey(JourneyNames.AddRouteToProfessionalStatus)]
public class TrainingProviderModel(AddRouteJourneyCoordinator journey, ReferenceDataCache referenceDataCache) : PageModel
{
    private readonly InlineValidator<TrainingProviderModel> _validator = new()
    {
        v => v.RuleFor(m => m.TrainingProviderId)
            .NotNull().WithMessage("Select a training provider")
            .When(m => m.TrainingProviderRequired)
    };

    public string PageCaption => journey.PageCaption;

    public string? BackLink { get; set; }

    public TrainingProvider[] TrainingProviders { get; set; } = [];

    public bool TrainingProviderRequired { get; set; }

    [BindProperty]
    public bool Cancel { get; set; }

    [BindProperty]
    public Guid? TrainingProviderId { get; set; }

    public void OnGet()
    {
        TrainingProviderId = journey.State.TrainingProviderId;
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (Cancel)
        {
            return Redirect(await journey.CancelAsync(cancellationToken));
        }

        await this.ThrowIfInvalidAsync(_validator, cancellationToken);

        return await journey.AnswerAndAdvanceAsync(AddRoutePage.TrainingProvider, state => state.TrainingProviderId = TrainingProviderId);
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        var cancellationToken = context.HttpContext.RequestAborted;

        TrainingProviderRequired = await journey.QuestionIsMandatoryAsync(AddRoutePage.TrainingProvider);
        TrainingProviders = await referenceDataCache.GetTrainingProvidersAsync();

        BackLink = journey.GetBackLink();

        await base.OnPageHandlerExecutionAsync(context, next);
    }
}
