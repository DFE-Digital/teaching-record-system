using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.UiCommon.FormFlow;

namespace TeachingRecordSystem.AuthorizeAccess.Pages;

[Journey(SignInJourneyState.JourneyName), RequireJourneyInstance]
public class SignOutModel : PageModel
{
    public JourneyInstance<SignInJourneyState>? JourneyInstance { get; set; }

    public string ServiceName => JourneyInstance!.State.ServiceName;

    public void OnGet()
    {
    }

    public IActionResult OnPost() =>
        SignOut(
            new AuthenticationProperties()
            {
                RedirectUri = JourneyInstance!.State.ServiceUrl
            },
            JourneyInstance.State.OneLoginAuthenticationScheme);
}
