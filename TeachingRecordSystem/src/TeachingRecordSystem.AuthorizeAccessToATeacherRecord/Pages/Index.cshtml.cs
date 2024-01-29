using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.AuthorizeAccessToATeacherRecord.Infrastructure.Security;
using TeachingRecordSystem.FormFlow;

namespace TeachingRecordSystem.AuthorizeAccessToATeacherRecord.Pages;

[Journey(SignInJourneyState.JourneyName)]
[Authorize(AuthenticationSchemes = AuthenticationSchemes.MatchToTeachingRecord)]
public class IndexModel : PageModel
{
    public void OnGet()
    {
    }
}
