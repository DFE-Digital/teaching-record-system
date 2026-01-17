using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TeachingRecordSystem.AuthorizeAccess.Pages;

[Journey(SignInJourneyCoordinator.JourneyName)]
public class SupportRequestSubmittedModel() : PageModel
{
    public void OnGet()
    {
    }
}
