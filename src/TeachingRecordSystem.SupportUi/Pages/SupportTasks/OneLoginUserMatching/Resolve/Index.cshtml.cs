using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.OneLoginUserMatching.Resolve;

[Journey(JourneyNames.ResolveOneLoginUserMatching), StartsJourney]
public class IndexModel(ResolveOneLoginUserMatchingJourneyCoordinator journey) : PageModel
{
    [FromQuery]
    public string? ReturnUrl { get; set; }

    public IActionResult OnGet() =>
        journey.AdvanceTo(journey.GetFirstStepUrl(ReturnUrl), new PushStepOptions { SetAsFirstStep = true });
}
