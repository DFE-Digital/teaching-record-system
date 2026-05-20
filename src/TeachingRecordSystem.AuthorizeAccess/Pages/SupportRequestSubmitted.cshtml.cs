using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TeachingRecordSystem.AuthorizeAccess.Pages;

[Journey(SignInJourneyCoordinator.JourneyName)]
public class SupportRequestSubmittedModel(SignInJourneyCoordinator coordinator) : PageModel
{
    public string SupportTaskReference => coordinator.State.CreatedSupportTaskReference!;

    public bool CanContinueToService => coordinator.State.AuthenticationTicket is not null;

    public string ServiceName => coordinator.State.ServiceName;

    public void OnGet()
    {
    }

    public IActionResult OnPost()
    {
        if (!CanContinueToService)
        {
            return BadRequest();
        }

        return coordinator.GetNextPage().ToActionResult();
    }
}
