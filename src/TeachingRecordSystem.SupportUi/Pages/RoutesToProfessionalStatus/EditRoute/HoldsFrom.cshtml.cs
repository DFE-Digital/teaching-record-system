using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.EditRoute;

[Journey(JourneyNames.EditRouteToProfessionalStatus)]
public class HoldsFromModel(
    EditRouteJourneyCoordinator journey,
    SupportUiLinkGenerator linkGenerator,
    TimeProvider timeProvider) : PageModel
{
    private readonly InlineValidator<HoldsFromModel> _validator = new()
    {
        v => v.RuleFor(m => m.HoldsFrom)
            .NotNull().WithMessage("Enter the date they first held this professional status")
            .When(m => m.HoldsFromRequired),
        v => v.RuleFor(m => m.HoldsFrom)
            .Must(holdsFrom => !(holdsFrom > timeProvider.Today))
                .WithMessage("The date they first held this professional status must not be in the future")
    };

    public string PageCaption => journey.PageCaption;

    public string? BackLink { get; set; }

    public bool HoldsFromRequired { get; set; }

    [BindProperty]
    public bool Cancel { get; set; }

    [BindProperty]
    public DateOnly? HoldsFrom { get; set; }

    public void OnGet()
    {
        HoldsFrom = journey.State.HoldsFrom;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Cancel)
        {
            return Redirect(await journey.CancelAsync());
        }

        await this.ThrowIfInvalidAsync(_validator);

        var detailUrl = linkGenerator.RoutesToProfessionalStatus.EditRoute.Detail(journey.InstanceId);

        if (!journey.IsCompletingRoute)
        {
            journey.UpdateState(state => state.HoldsFrom = HoldsFrom);
            return Redirect(journey.GetReturnUrlOrDefault(detailUrl));
        }

        if (await journey.IsLastCompletingRoutePageAsync(EditRoutePage.HoldsFrom))
        {
            journey.CompleteRoute(state =>
            {
                state.HoldsFrom = HoldsFrom;
                state.IsExemptFromInduction = state.EditStatusState!.InductionExemption;
            });

            await journey.RefreshAvailablePagesAsync();

            return Redirect(detailUrl);
        }

        journey.UpdateState(state => state.EditStatusState = state.EditStatusState! with { HoldsFrom = HoldsFrom });

        return Redirect(linkGenerator.RoutesToProfessionalStatus.EditRoute.InductionExemption(journey.InstanceId));
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        HoldsFromRequired = await journey.QuestionIsMandatoryAsync(EditRoutePage.HoldsFrom);

        BackLink = journey.GetReturnUrlOrDefault(
            journey.IsCompletingRoute
                ? linkGenerator.RoutesToProfessionalStatus.EditRoute.Status(journey.InstanceId)
                : linkGenerator.RoutesToProfessionalStatus.EditRoute.Detail(journey.InstanceId));

        await base.OnPageHandlerExecutionAsync(context, next);
    }
}
