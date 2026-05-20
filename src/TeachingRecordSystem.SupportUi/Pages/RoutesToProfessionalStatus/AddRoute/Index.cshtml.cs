using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.AddRoute;

[Journey(JourneyNames.AddRouteToProfessionalStatus), ActivatesJourney, RequireJourneyInstance]
public class IndexModel(SupportUiLinkGenerator linkGenerator) : PageModel
{
    public JourneyInstance<AddRouteState>? JourneyInstance { get; set; }

    [FromQuery]
    public Guid PersonId { get; set; }

    public IActionResult OnGet() => Redirect(linkGenerator.RoutesToProfessionalStatus.AddRoute.Route(PersonId, JourneyInstance!.InstanceId));
}

