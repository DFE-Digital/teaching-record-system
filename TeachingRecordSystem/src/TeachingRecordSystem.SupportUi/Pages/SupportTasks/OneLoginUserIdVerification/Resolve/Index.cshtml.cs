using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.OneLoginUserIdVerification.Resolve;

[Journey(JourneyNames.ResolveOneLoginUserIdVerification), ActivatesJourney, RequireJourneyInstance]
public class Index : PageModel
{
    public void OnGet()
    {

    }
}
