using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TeachingRecordSystem.AuthorizeAccess.Pages;

[Journey(SignInJourneyCoordinator.JourneyName)]
public class CookiesModel(SignInJourneyCoordinator coordinator) : PageModel
{
    public void OnGet()
    {
    }
}
