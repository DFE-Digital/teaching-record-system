using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.NpqTrnRequests.Reject;

[Journey(JourneyNames.RejectNpqTrnRequest), ActivatesJourney, RequireJourneyInstance]
public class IndexModel(TrsLinkGenerator linkGenerator) : PageModel
{
    public JourneyInstance<RejectNpqTrnRequestState>? JourneyInstance { get; set; }

    [FromRoute]
    public required string SupportTaskReference { get; init; }

    public IActionResult OnGet() => Redirect(linkGenerator.NpqTrnRequestRejectionReason(SupportTaskReference, JourneyInstance!.InstanceId));
}
