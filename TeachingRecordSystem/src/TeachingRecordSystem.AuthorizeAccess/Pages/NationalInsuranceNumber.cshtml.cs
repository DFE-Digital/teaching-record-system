using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.FormFlow;

namespace TeachingRecordSystem.AuthorizeAccess.Pages;

[Journey(SignInJourneyState.JourneyName), RequireJourneyInstance]
public class NationalInsuranceNumberModel(SignInJourneyHelper helper, AuthorizeAccessLinkGenerator linkGenerator) : PageModel
{
    public JourneyInstance<SignInJourneyState>? JourneyInstance { get; set; }

    [BindProperty]
    [Display(Name = "What is your National Insurance number?", Description = "It’s on your National Insurance card, benefit letter, payslip or P60. For example, ‘QQ 12 34 56 C’.")]
    [Required(ErrorMessage = "Enter a National Insurance number")]
    public string? NationalInsuranceNumber { get; set; }

    public void OnGet()
    {
        NationalInsuranceNumber = JourneyInstance!.State.NationalInsuranceNumber;
    }

    public async Task<IActionResult> OnPost()
    {
        if (ModelState[nameof(NationalInsuranceNumber)]!.Errors.Count == 0)
        {
            if (!NationalInsuranceNumberHelper.IsValid(NationalInsuranceNumber))
            {
                ModelState.AddModelError(nameof(NationalInsuranceNumber), "Enter a National Insurance number in the correct format");
            }
        }

        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        await JourneyInstance!.UpdateStateAsync(state =>
        {
            state.NationalInsuranceNumberSpecified = true;
            state.NationalInsuranceNumber = NationalInsuranceNumber;
        });

        if (await helper.TryMatchToTeachingRecord(JourneyInstance!))
        {
            return helper.GetNextPage(JourneyInstance).ToActionResult();
        }

        return RedirectToNextPage();
    }

    public async Task<IActionResult> OnPostContinueWithout()
    {
        await JourneyInstance!.UpdateStateAsync(state =>
        {
            state.NationalInsuranceNumberSpecified = true;
            state.NationalInsuranceNumber = null;
        });

        return RedirectToNextPage();
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

    private IActionResult RedirectToNextPage() => Redirect(linkGenerator.Trn(JourneyInstance!.InstanceId));
}
