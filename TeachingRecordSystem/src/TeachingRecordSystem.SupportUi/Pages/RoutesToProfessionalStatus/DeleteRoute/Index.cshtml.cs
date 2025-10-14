using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.SupportUi.Infrastructure.Filters;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.DeleteRoute;

[Journey(JourneyNames.DeleteRouteToProfessionalStatus), ActivatesJourney, RequireJourneyInstance, CheckRouteToProfessionalStatusExistsFilterFactory()]
public class IndexModel(SupportUiLinkGenerator linkGenerator) : PageModel
{
    public JourneyInstance<DeleteRouteState>? JourneyInstance { get; set; }

    [FromRoute]
    public Guid QualificationId { get; set; }

    public IActionResult OnGet() => Redirect(linkGenerator.RoutesToProfessionalStatus.DeleteRoute.Reason(QualificationId, JourneyInstance!.InstanceId));
}
