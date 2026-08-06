using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.EditRoute;

[Journey(JourneyNames.EditRouteToProfessionalStatus), StartsJourney]
public class IndexModel(EditRouteJourneyCoordinator journey, SupportUiLinkGenerator linkGenerator) : PageModel
{
    [FromRoute]
    public Guid QualificationId { get; set; }

    public IActionResult OnGet() =>
        journey.AdvanceTo(
            linkGenerator.RoutesToProfessionalStatus.EditRoute.Detail(journey.InstanceId),
            new PushStepOptions { SetAsFirstStep = true });
}
