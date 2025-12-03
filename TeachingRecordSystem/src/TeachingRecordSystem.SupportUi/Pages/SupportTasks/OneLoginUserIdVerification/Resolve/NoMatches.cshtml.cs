using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.OneLoginUserIdVerification.Resolve;

[Journey(JourneyNames.ResolveOneLoginUserIdVerification), RequireJourneyInstance]
public class NoMatches : PageModel
{
    public JourneyInstance<ResolveOneLoginUserIdVerificationState>? JourneyInstance { get; set; }

    public void OnGet()
    {

    }
}
