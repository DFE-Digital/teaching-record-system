using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.TrnRequests.Resolve;

[Journey(JourneyNames.ResolveTrnRequest), ActivatesJourney, RequireJourneyInstance]
public class IndexModel(SupportUiLinkGenerator linkGenerator) : PageModel
{
    public JourneyInstance<ResolveTrnRequestState>? JourneyInstance { get; set; }

    [FromRoute]
    public required string SupportTaskReference { get; init; }

    public IActionResult OnGet() => Redirect(linkGenerator.SupportTasks.TrnRequests.Resolve.Matches(SupportTaskReference, JourneyInstance!.InstanceId));
}
