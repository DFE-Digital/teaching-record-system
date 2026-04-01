using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TeachingRecordSystem.AuthorizeAccess.Pages;

[Journey(SignInJourneyCoordinator.JourneyName)]
public class SupportRequestSubmittedModel(SignInJourneyCoordinator coordinator) : PageModel
{
    public string SupportTaskReference => coordinator.State.CreatedSupportTaskReference!;

    public bool CanReturnToService => coordinator.State.AuthenticationTicket is not null;

    public string ServiceName => coordinator.State.ServiceName;

    public IActionResult OnPost()
    {
        return coordinator.GetNextPage().ToActionResult();
    }
}
