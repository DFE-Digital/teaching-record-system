using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.SupportUi.Infrastructure.Filters;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.DeleteRoute;

[Journey(JourneyNames.DeleteRouteToProfessionalStatus), ActivatesJourney, RequireJourneyInstance, CheckProfessionalStatusExistsFilterFactory()]
public class IndexModel(TrsLinkGenerator linkGenerator) : PageModel
{
    public JourneyInstance<DeleteRouteState>? JourneyInstance { get; set; }

    [FromQuery]
    public Guid QualificationId { get; set; }

    public IActionResult OnGet() => Redirect(linkGenerator.DeleteRouteChangeReason(QualificationId, JourneyInstance!.InstanceId));
}
