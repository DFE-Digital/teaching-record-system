using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.TeacherPensions.Resolve;

[Journey(JourneyNames.ResolveTpsPotentialDuplicate), ActivatesJourney, RequireJourneyInstance]
public class IndexModel(SupportUiLinkGenerator linkGenerator) : PageModel
{
    public JourneyInstance<ResolveTeacherPensionsPotentialDuplicateState>? JourneyInstance { get; set; }

    [FromRoute]
    public required string SupportTaskReference { get; init; }

    public IActionResult OnGet() => Redirect(linkGenerator.SupportTasks.TeacherPensions.Resolve.Matches(SupportTaskReference, JourneyInstance!.InstanceId));
}
