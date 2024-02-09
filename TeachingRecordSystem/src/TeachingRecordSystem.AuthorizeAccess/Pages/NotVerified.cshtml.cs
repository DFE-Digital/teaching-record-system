using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.FormFlow;

namespace TeachingRecordSystem.AuthorizeAccess.Pages;

[Journey(SignInJourneyState.JourneyName), RequireJourneyInstance]
public class NotVerifiedModel : PageModel
{
    public JourneyInstance<SignInJourneyState>? JourneyInstance { get; set; }

    public void OnGet()
    {
    }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        var state = JourneyInstance!.State;

        if (state.OneLoginAuthenticationTicket is null)
        {
            // Not authenticated with One Login
            context.Result = BadRequest();
        }
        else if (state.IdentityVerified)
        {
            // Verified specified NINO or TRN
            context.Result = BadRequest();
        }
    }
}
