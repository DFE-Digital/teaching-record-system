using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TeachingRecordSystem.AuthorizeAccess.Pages;

[Journey(SignInJourneyCoordinator.JourneyName)]
public class FoundModel(SignInJourneyCoordinator coordinator) : PageModel
{
    public string? LinkText => coordinator.State.AppContent?.OneLoginFoundPageLinkText;

    public string ContinueToApplicationUrl => coordinator.Links.ContinueToApplication();

    public void OnGet()
    {
    }
}
