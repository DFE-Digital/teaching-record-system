using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TeachingRecordSystem.AuthorizeAccess.Pages;

[Journey(SignInJourneyCoordinator.JourneyName)]
public class NotFoundModel(SignInJourneyCoordinator coordinator) : PageModel
{
    public void OnGet()
    {
    }

    public IActionResult OnPost() => coordinator.AdvanceTo(links => links.CheckAnswers());
}
