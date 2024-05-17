using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.FormFlow;

namespace TeachingRecordSystem.AuthorizeAccess.Pages.RequestTrn;

[Journey(RequestTrnJourneyState.JourneyName), ActivatesJourney, RequireJourneyInstance]
public class IndexModel : PageModel
{
    public JourneyInstance<RequestTrnJourneyState>? JourneyInstance { get; set; }
}
