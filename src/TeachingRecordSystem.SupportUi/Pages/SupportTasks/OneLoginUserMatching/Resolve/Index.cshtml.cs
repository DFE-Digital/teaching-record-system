using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.OneLoginUserMatching.Resolve;

[Journey(JourneyNames.ResolveOneLoginUserMatching), StartsJourney]
public class IndexModel(ResolveOneLoginUserMatchingJourneyCoordinator journey) : PageModel
{
    public IActionResult OnGet() =>
        journey.AdvanceTo(journey.GetFirstStepUrl(), new PushStepOptions { SetAsFirstStep = true });
}
