using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.OneLoginUserMatching.Resolve;

[Journey(JourneyNames.ResolveOneLoginUserMatching), StartsJourney]
public class IndexModel(
    ResolveOneLoginUserMatchingJourneyCoordinator journey,
    SupportUiLinkGenerator linkGenerator) : PageModel
{
    public IActionResult OnGet()
    {
        var supportTask = HttpContext.GetCurrentSupportTaskFeature().SupportTask;

        var firstStepUrl = supportTask.SupportTaskType == SupportTaskType.OneLoginUserRecordMatching ?
            journey.State.MatchedPersons.Count > 0 ?
                linkGenerator.SupportTasks.OneLoginUserMatching.Resolve.Matches(journey.InstanceId) :
                linkGenerator.SupportTasks.OneLoginUserMatching.Resolve.NoMatches(journey.InstanceId) :
            linkGenerator.SupportTasks.OneLoginUserMatching.Resolve.Verify(journey.InstanceId);

        return journey.AdvanceTo(firstStepUrl, new PushStepOptions { SetAsFirstStep = true });
    }
}
