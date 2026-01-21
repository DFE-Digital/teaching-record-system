using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TeachingRecordSystem.AuthorizeAccess.Pages;

[Journey(SignInJourneyCoordinator.JourneyName)]
public class SupportRequestSubmittedModel(SignInJourneyCoordinator coordinator) : PageModel
{
    public string SupportTaskReference => coordinator.State.CreatedSupportTaskReference!;
}
