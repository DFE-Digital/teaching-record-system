using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.SupportUi.Infrastructure.Filters;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.EditRoute;

[Journey(JourneyNames.EditRouteToProfessionalStatus), ActivatesJourney, RequireJourneyInstance, CheckRouteToProfessionalStatusExistsFilterFactory()]
public class IndexModel(SupportUiLinkGenerator linkGenerator) : PageModel
{
    public JourneyInstance<EditRouteState>? JourneyInstance { get; set; }

    [FromRoute]
    public Guid QualificationId { get; set; }

    [FromQuery]
    public bool FromInductions { get; set; }

    public IActionResult OnGet() => Redirect(linkGenerator.RoutesToProfessionalStatus.EditRoute.Detail(QualificationId, JourneyInstance!.InstanceId, FromInductions));
}
