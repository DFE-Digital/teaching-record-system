using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.AddRoute;

[Journey(JourneyNames.AddRouteToProfessionalStatus), ActivatesJourney, RequireJourneyInstance]
public class CheckYourAnswersModel : PageModel
{
    public void OnGet()
    {
    }
}
