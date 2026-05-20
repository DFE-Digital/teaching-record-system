using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TeachingRecordSystem.AuthorizeAccess.Pages;

[Journey(SignInJourneyCoordinator.JourneyName)]
public class NotVerifiedModel(SignInJourneyCoordinator coordinator) : PageModel
{
    public void OnGet()
    {
    }

    public IActionResult OnPost(bool tryAgain)
    {
        if (tryAgain)
        {
            return coordinator.VerifyIdentityWithOneLogin().ToActionResult();
        }

        return coordinator.AdvanceTo(links => links.Name());
    }
}
