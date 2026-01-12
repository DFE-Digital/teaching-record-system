using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TeachingRecordSystem.AuthorizeAccess.Pages;

[Journey(SignInJourneyCoordinator.JourneyName)]
public class SignOutModel(SignInJourneyCoordinator coordinator) : PageModel
{
    public string ServiceName => coordinator.State.ServiceName;

    public void OnGet()
    {
    }

    public IActionResult OnPost() =>
        SignOut(
            new AuthenticationProperties
            {
                RedirectUri = coordinator.State.ServiceUrl
            },
            coordinator.State.OneLoginAuthenticationScheme);
}
