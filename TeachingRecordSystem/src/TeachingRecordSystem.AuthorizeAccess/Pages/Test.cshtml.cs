using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.AuthorizeAccess.Infrastructure.Security;
using TeachingRecordSystem.FormFlow;

namespace TeachingRecordSystem.AuthorizeAccess.Pages;

[Journey(SignInJourneyState.JourneyName)]
public class TestModel : PageModel
{
    [FromQuery(Name = "scheme")]
    public string? AuthenticationScheme { get; set; }

    public IActionResult OnGet()
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            return Challenge(
                new AuthenticationProperties()
                {
                    Items =
                    {
                        { "OneLoginAuthenticationScheme", AuthenticationScheme }
                    },
                    RedirectUri = Request.GetEncodedUrl()
                },
                AuthenticationSchemes.MatchToTeachingRecord);
        }

        return Page();
    }
}
