using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.FormFlow;

namespace TeachingRecordSystem.AuthorizeAccess.Pages;

[Journey(SignInJourneyState.JourneyName), RequireJourneyInstance]
public class NotFoundModel(SignInJourneyHelper helper) : PageModel
{
    public JourneyInstance<SignInJourneyState>? JourneyInstance { get; set; }

    public void OnGet()
    {
    }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        var state = JourneyInstance!.State;

        if (state.OneLoginAuthenticationTicket is null || !state.IdentityVerified)
        {
            // Not authenticated/verified with One Login
            context.Result = BadRequest();
        }
        else if (!state.NationalInsuranceNumberSpecified || !state.TrnSpecified)
        {
            // Not specified NINO or TRN
            context.Result = helper.GetNextPage(JourneyInstance).ToActionResult();
        }
        else if (state.AuthenticationTicket is not null)
        {
            // Already matched to a Teaching Record
            context.Result = Redirect(helper.GetSafeRedirectUri(JourneyInstance));
        }
    }
}
