using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TeachingRecordSystem.AuthorizeAccess.Pages;

[Journey(SignInJourneyCoordinator.JourneyName)]
public class FoundModel(SignInJourneyCoordinator coordinator) : PageModel
{
    public void OnGet()
    {
    }

    public IActionResult OnPost() => coordinator.GetNextPage().ToActionResult();
}
