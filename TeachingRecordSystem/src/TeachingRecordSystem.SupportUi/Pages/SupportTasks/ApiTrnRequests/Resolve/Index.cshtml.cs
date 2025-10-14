using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.ApiTrnRequests.Resolve;

[Journey(JourneyNames.ResolveApiTrnRequest), ActivatesJourney, RequireJourneyInstance]
public class IndexModel(SupportUiLinkGenerator linkGenerator) : PageModel
{
    public JourneyInstance<ResolveApiTrnRequestState>? JourneyInstance { get; set; }

    [FromRoute]
    public required string SupportTaskReference { get; init; }

    public IActionResult OnGet() => Redirect(linkGenerator.SupportTasks.ApiTrnRequests.Resolve.Matches(SupportTaskReference, JourneyInstance!.InstanceId));
}
