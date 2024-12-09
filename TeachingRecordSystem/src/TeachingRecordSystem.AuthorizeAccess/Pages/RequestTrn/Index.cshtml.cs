using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.UiCommon.FormFlow;

namespace TeachingRecordSystem.AuthorizeAccess.Pages.RequestTrn;

[Journey(RequestTrnJourneyState.JourneyName), ActivatesJourney, RequireJourneyInstance]
public class IndexModel(IConfiguration configuration) : PageModel
{
    public JourneyInstance<RequestTrnJourneyState>? JourneyInstance { get; set; }

    [FromQuery]
    public string? AccessToken { get; set; }

    public ActionResult OnGet()
    {
        var whitelistedAccessToken = configuration.GetRequiredValue("RequestTrnAccessToken");
        if (!whitelistedAccessToken.Equals(AccessToken, StringComparison.Ordinal))
        {
            return NotFound();
        }
        return Page();
    }
}
