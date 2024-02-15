using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.FormFlow;

namespace TeachingRecordSystem.AuthorizeAccess.Pages;

[Journey(SignInJourneyState.JourneyName), RequireJourneyInstance]
public class TrnModel(SignInJourneyHelper helper, AuthorizeAccessLinkGenerator linkGenerator) : PageModel
{
    public JourneyInstance<SignInJourneyState>? JourneyInstance { get; set; }

    [BindProperty]
    [Display(Name = "Teacher reference number (TRN)")]
    [Required(ErrorMessage = "Enter your TRN")]
    [RegularExpression(@"\A\D*(\d{1}\D*){7}\D*\Z", ErrorMessage = "Your TRN should contain 7 digits")]
    public string? Trn { get; set; }

    public void OnGet()
    {
        Trn = JourneyInstance!.State.Trn;
    }

    public async Task<IActionResult> OnPost()
    {
        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        await JourneyInstance!.UpdateStateAsync(state =>
        {
            state.TrnSpecified = true;
            state.Trn = Trn;
        });

        if (await helper.TryMatchToTeachingRecord(JourneyInstance!))
        {
            return new RedirectResult(helper.GetNextPage(JourneyInstance));
        }
        else
        {
            return Redirect(linkGenerator.NotFound(JourneyInstance.InstanceId));
        }
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
    }
}
