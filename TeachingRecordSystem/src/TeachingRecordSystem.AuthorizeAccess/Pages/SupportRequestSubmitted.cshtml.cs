using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.WebCommon.FormFlow;

namespace TeachingRecordSystem.AuthorizeAccess.Pages;

[Journey(SignInJourneyState.JourneyName), RequireJourneyInstance]
public class SupportRequestSubmittedModel(SignInJourneyHelper helper) : PageModel
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
        else if (state.AuthenticationTicket is not null)
        {
            // Already matched to a Teaching Record
            context.Result = Redirect(helper.GetSafeRedirectUri(JourneyInstance));
        }
        else if (!state.HaveNationalInsuranceNumber.HasValue)
        {
            // Not answered the NINO question
            context.Result = Redirect(helper.LinkGenerator.NationalInsuranceNumber(JourneyInstance.InstanceId));
        }
        else if (!state.HaveTrn.HasValue)
        {
            // Not answered the TRN question
            context.Result = Redirect(helper.LinkGenerator.Trn(JourneyInstance.InstanceId));
        }
        else if (!state.HasPendingSupportRequest)
        {
            // Not submitted a submit request
            context.Result = Redirect(helper.LinkGenerator.CheckAnswers(JourneyInstance.InstanceId));
        }
    }
}
