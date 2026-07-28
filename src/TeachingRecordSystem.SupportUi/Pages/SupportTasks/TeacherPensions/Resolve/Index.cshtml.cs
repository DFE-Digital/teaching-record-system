using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.TeacherPensions.Resolve;

[Journey(JourneyNames.ResolveTpsPotentialDuplicate), StartsJourney]
public class IndexModel(
    ResolveTeacherPensionsPotentialDuplicateJourneyCoordinator journey,
    SupportUiLinkGenerator linkGenerator) : PageModel
{
    [FromQuery]
    public string? ReturnUrl { get; set; }

    public IActionResult OnGet() =>
        journey.AdvanceTo(
            linkGenerator.SupportTasks.TeacherPensions.Resolve.Matches(journey.InstanceId, ReturnUrl),
            new PushStepOptions { SetAsFirstStep = true });
}
