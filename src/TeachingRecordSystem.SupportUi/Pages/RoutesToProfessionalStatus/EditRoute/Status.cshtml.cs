using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.EditRoute;

[Journey(JourneyNames.EditRouteToProfessionalStatus)]
public class StatusModel(
    EditRouteJourneyCoordinator journey,
    SupportUiLinkGenerator linkGenerator) : PageModel
{
    private readonly InlineValidator<StatusModel> _validator = new()
    {
        v => v.RuleFor(m => m.Status)
            .IsInEnum().WithMessage("Select a route status")
    };

    public string PageCaption => journey.PageCaption;

    public string BackLink => journey.GetReturnUrlOrDefault(DetailUrl);

    public RouteToProfessionalStatusType Route { get; set; } = null!;

    public ProfessionalStatusStatusInfo[] Statuses { get; set; } = [];

    [BindProperty]
    public bool Cancel { get; set; }

    [BindProperty]
    public RouteToProfessionalStatusStatus Status { get; set; }

    public bool NotCompletedRoute => Status is not RouteToProfessionalStatusStatus.Holds;

    private string DetailUrl => linkGenerator.RoutesToProfessionalStatus.EditRoute.Detail(journey.InstanceId);

    // The user is completing the route if they're moving it to 'holds' from something else.
    private bool CompletingRoute =>
        Status is RouteToProfessionalStatusStatus.Holds && journey.State.CurrentStatus is not RouteToProfessionalStatusStatus.Holds;

    public void OnGet()
    {
        Status = journey.Status;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Cancel)
        {
            return Redirect(await journey.CancelAsync());
        }

        await this.ThrowIfInvalidAsync(_validator);

        if (CompletingRoute)
        {
            // A route that comes with an induction exemption of its own has it set now rather than asking.
            var hasImplicitExemption = Route.InductionExemptionReason?.RouteImplicitExemption ?? false;

            journey.UpdateState(state => state.EditStatusState = state.EditStatusState is EditRouteStatusState editStatusState
                ? editStatusState with { Status = Status }
                : new EditRouteStatusState
                {
                    Status = Status,
                    RouteImplicitExemption = hasImplicitExemption,
                    InductionExemption = hasImplicitExemption ? true : null
                });

            await journey.RefreshAvailablePagesAsync();

            return Redirect(linkGenerator.RoutesToProfessionalStatus.EditRoute.HoldsFrom(journey.InstanceId));
        }

        journey.UpdateState(state =>
        {
            // Any date and exemption the route held before no longer apply.
            state.HoldsFrom = null;
            state.IsExemptFromInduction = null;
            state.Status = Status;
        });

        await journey.RefreshAvailablePagesAsync();

        return Redirect(journey.GetReturnUrlOrDefault(DetailUrl));
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        Statuses = ProfessionalStatusStatusRegistry.All.ToArray();
        Route = await journey.GetRouteTypeAsync();

        await base.OnPageHandlerExecutionAsync(context, next);
    }
}
