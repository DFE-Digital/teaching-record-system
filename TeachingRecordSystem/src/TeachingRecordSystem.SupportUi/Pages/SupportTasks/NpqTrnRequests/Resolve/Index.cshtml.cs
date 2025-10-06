using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.NpqTrnRequests.Resolve;

[Journey(JourneyNames.ResolveNpqTrnRequest), ActivatesJourney, RequireJourneyInstance]
public class IndexModel(TrsLinkGenerator linkGenerator) : PageModel
{
    public JourneyInstance<ResolveNpqTrnRequestState>? JourneyInstance { get; set; }

    [FromRoute]
    public string SupportTaskReference { get; set; }

    public IActionResult OnGet() => Redirect(linkGenerator.NpqTrnRequestMatches(SupportTaskReference, JourneyInstance!.InstanceId));
}
