using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.FormFlow;

namespace TeachingRecordSystem.AuthorizeAccess.Pages;

[Journey(SignInJourneyState.JourneyName), RequireJourneyInstance]
public class CheckAnswersModel(SignInJourneyHelper helper) : PageModel
{
    public JourneyInstance<SignInJourneyState>? JourneyInstance { get; set; }

    public string? Email { get; set; }

    public string? Name { get; set; }

    public DateOnly DateOfBirth { get; set; }

    public string? NationalInsuranceNumber => JourneyInstance!.State.NationalInsuranceNumber;

    public string? Trn => JourneyInstance!.State.Trn;

    public void OnGet()
    {
    }

    public IActionResult OnPost() => throw new NotImplementedException();

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

        if (context.Result is null)
        {
            Email = state.OneLoginAuthenticationTicket!.Principal.FindFirstValue("email")!;
            Name = string.Join(" ", state.VerifiedNames!.First());
            DateOfBirth = state.VerifiedDatesOfBirth!.First();
        }
    }
}
