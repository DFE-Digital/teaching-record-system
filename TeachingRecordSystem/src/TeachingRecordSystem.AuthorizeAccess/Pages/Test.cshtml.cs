using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.AuthorizeAccess.Infrastructure.Security;
using TeachingRecordSystem.UiCommon.FormFlow;

namespace TeachingRecordSystem.AuthorizeAccess.Pages;

[Journey(SignInJourneyState.JourneyName)]
public class TestModel : PageModel
{
    [FromQuery(Name = "scheme")]
    public string? AuthenticationScheme { get; set; }

    [FromQuery(Name = "trn_token")]
    public string? TrnToken { get; set; }

    public IActionResult OnGet()
    {
        if (string.IsNullOrEmpty(AuthenticationScheme))
        {
            return BadRequest();
        }

        if (User.Identity?.IsAuthenticated != true)
        {
            return Challenge(
                new AuthenticationProperties()
                {
                    Items =
                    {
                        { MatchToTeachingRecordAuthenticationHandler.AuthenticationPropertiesItemKeys.OneLoginAuthenticationScheme, AuthenticationScheme },
                        { MatchToTeachingRecordAuthenticationHandler.AuthenticationPropertiesItemKeys.ServiceName, "Test service" },
                        { MatchToTeachingRecordAuthenticationHandler.AuthenticationPropertiesItemKeys.ServiceUrl, Request.GetEncodedUrl() },
                        { MatchToTeachingRecordAuthenticationHandler.AuthenticationPropertiesItemKeys.TrnToken, TrnToken },
                    },
                    RedirectUri = Request.GetEncodedUrl()
                },
                AuthenticationSchemes.MatchToTeachingRecord);
        }

        return Page();
    }
}
