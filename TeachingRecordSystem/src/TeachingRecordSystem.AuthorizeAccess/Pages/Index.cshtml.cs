using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.AuthorizeAccess.Infrastructure.Security;
using TeachingRecordSystem.FormFlow;

namespace TeachingRecordSystem.AuthorizeAccess.Pages;

[Journey(SignInJourneyState.JourneyName)]
[Authorize(AuthenticationSchemes = AuthenticationSchemes.MatchToTeachingRecord)]
public class IndexModel : PageModel
{
    public void OnGet()
    {
    }
}
