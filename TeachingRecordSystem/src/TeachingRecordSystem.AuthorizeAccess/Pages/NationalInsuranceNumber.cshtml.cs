using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.UiCommon.FormFlow;

namespace TeachingRecordSystem.AuthorizeAccess.Pages;

[Journey(SignInJourneyState.JourneyName), RequireJourneyInstance]
public class NationalInsuranceNumberModel(SignInJourneyHelper helper) : PageModel
{
    public JourneyInstance<SignInJourneyState>? JourneyInstance { get; set; }

    [FromQuery]
    public bool? FromCheckAnswers { get; set; }

    [BindProperty]
    [Display(Name = "Do you have a National Insurance number?")]
    [Required(ErrorMessage = "Select yes if you have a National Insurance number")]
    public bool? HaveNationalInsuranceNumber { get; set; }

    [BindProperty]
    [Display(Name = "National Insurance number")]
    [Required(ErrorMessage = "Enter a National Insurance number")]
    public string? NationalInsuranceNumber { get; set; }

    public void OnGet()
    {
        HaveNationalInsuranceNumber = JourneyInstance!.State.HaveNationalInsuranceNumber;
        NationalInsuranceNumber = JourneyInstance!.State.NationalInsuranceNumber;
    }

    public async Task<IActionResult> OnPost()
    {
        if (HaveNationalInsuranceNumber == true)
        {
            if (!string.IsNullOrEmpty(NationalInsuranceNumber) && !NationalInsuranceNumberHelper.IsValid(NationalInsuranceNumber))
            {
                ModelState.AddModelError(nameof(NationalInsuranceNumber), "Enter a National Insurance number in the correct format");
            }
        }
        else
        {
            ModelState.Remove(nameof(NationalInsuranceNumber));
        }

        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        await JourneyInstance!.UpdateStateAsync(state => state.SetNationalInsuranceNumber(HaveNationalInsuranceNumber!.Value, NationalInsuranceNumber));

        return await helper.TryMatchToTeachingRecord(JourneyInstance!) ? Redirect(helper.LinkGenerator.Found(JourneyInstance!.InstanceId)) :
            FromCheckAnswers == true ? Redirect(helper.LinkGenerator.CheckAnswers(JourneyInstance!.InstanceId)) :
            Redirect(helper.LinkGenerator.Trn(JourneyInstance!.InstanceId));
    }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        var state = JourneyInstance!.State;

        if (state.AuthenticationTicket is not null)
        {
            // Already matched to a Teaching Record
            context.Result = Redirect(helper.GetSafeRedirectUri(JourneyInstance));
        }
        else if (state.OneLoginAuthenticationTicket is null || !state.IdentityVerified)
        {
            // Not authenticated/verified with One Login
            context.Result = BadRequest();
        }
    }
}
