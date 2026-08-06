using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.DeleteRoute;

[Journey(JourneyNames.DeleteRouteToProfessionalStatus), StartsJourney]
public class IndexModel(DeleteRouteJourneyCoordinator journey, SupportUiLinkGenerator linkGenerator) : PageModel
{
    public IActionResult OnGet() =>
        journey.AdvanceTo(
            linkGenerator.RoutesToProfessionalStatus.DeleteRoute.Reason(journey.InstanceId),
            new PushStepOptions { SetAsFirstStep = true });
}
