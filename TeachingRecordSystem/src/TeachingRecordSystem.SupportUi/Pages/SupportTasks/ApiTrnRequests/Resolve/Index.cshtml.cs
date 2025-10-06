using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.ApiTrnRequests.Resolve;

[Journey(JourneyNames.ResolveApiTrnRequest), ActivatesJourney, RequireJourneyInstance]
public class IndexModel(TrsLinkGenerator linkGenerator) : PageModel
{
    public JourneyInstance<ResolveApiTrnRequestState>? JourneyInstance { get; set; }

    [FromRoute]
    public string? SupportTaskReference { get; set; }

    public IActionResult OnGet() => Redirect(linkGenerator.ApiTrnRequestMatches(SupportTaskReference, JourneyInstance!.InstanceId));
}
