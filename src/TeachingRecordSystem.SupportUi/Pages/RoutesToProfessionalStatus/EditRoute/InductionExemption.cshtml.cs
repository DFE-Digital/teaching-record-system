using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.EditRoute;

[Journey(JourneyNames.EditRouteToProfessionalStatus)]
public class InductionExemptionModel(
    EditRouteJourneyCoordinator journey,
    SupportUiLinkGenerator linkGenerator) : PageModel
{
    private readonly InlineValidator<InductionExemptionModel> _validator = new()
    {
        v => v.RuleFor(m => m.IsExemptFromInduction)
            .NotNull().WithMessage("Select yes if this route provides an induction exemption")
    };

    public string PageCaption => journey.PageCaption;

    public string? BackLink { get; set; }

    [BindProperty]
    public bool Cancel { get; set; }

    [BindProperty]
    public bool? IsExemptFromInduction { get; set; }

    public void OnGet()
    {
        IsExemptFromInduction = journey.State.IsExemptFromInduction;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Cancel)
        {
            return Redirect(await journey.CancelAsync());
        }

        await this.ThrowIfInvalidAsync(_validator);

        var detailUrl = linkGenerator.RoutesToProfessionalStatus.EditRoute.Detail(journey.InstanceId);

        // This is always the last question asked when a route is being completed.
        if (journey.IsCompletingRoute)
        {
            journey.CompleteRoute(state =>
            {
                state.HoldsFrom = state.EditStatusState!.HoldsFrom;
                state.IsExemptFromInduction = IsExemptFromInduction;
            });

            await journey.RefreshAvailablePagesAsync();

            return Redirect(detailUrl);
        }

        journey.UpdateState(state => state.IsExemptFromInduction = IsExemptFromInduction);

        return Redirect(journey.GetReturnUrlOrDefault(detailUrl));
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        // A route that doesn't ask about an induction exemption isn't a step the journey allows, so
        // there's nothing to guard against here.
        BackLink = journey.GetReturnUrlOrDefault(
            journey.IsCompletingRoute
                ? linkGenerator.RoutesToProfessionalStatus.EditRoute.HoldsFrom(journey.InstanceId)
                : linkGenerator.RoutesToProfessionalStatus.EditRoute.Detail(journey.InstanceId));

        await base.OnPageHandlerExecutionAsync(context, next);
    }
}
