using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.AddRoute;

[Journey(JourneyNames.AddRouteToProfessionalStatus), StartsJourney]
public class IndexModel(AddRouteJourneyCoordinator journey, SupportUiLinkGenerator linkGenerator) : PageModel
{
    public IActionResult OnGet() =>
        journey.AdvanceTo(
            linkGenerator.RoutesToProfessionalStatus.AddRoute.Route(journey.InstanceId),
            new PushStepOptions { SetAsFirstStep = true });
}
