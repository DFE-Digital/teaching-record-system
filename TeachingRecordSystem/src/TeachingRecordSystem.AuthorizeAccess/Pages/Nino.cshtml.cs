using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.FormFlow;

namespace TeachingRecordSystem.AuthorizeAccess.Pages;

[Authorize]
[Journey(SignInJourneyState.JourneyName), RequireJourneyInstance]
public class NinoModel : PageModel
{
    public void OnGet()
    {
    }
}
