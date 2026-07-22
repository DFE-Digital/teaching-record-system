using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.TrnRequests.Resolve;

[Journey(JourneyNames.ResolveTrnRequest), StartsJourney]
public class IndexModel(
    ResolveTrnRequestJourneyCoordinator journey,
    SupportUiLinkGenerator linkGenerator) : PageModel
{
    public IActionResult OnGet() =>
        journey.AdvanceTo(
            linkGenerator.SupportTasks.TrnRequests.Resolve.Matches(journey.InstanceId),
            new PushStepOptions { SetAsFirstStep = true });
}
