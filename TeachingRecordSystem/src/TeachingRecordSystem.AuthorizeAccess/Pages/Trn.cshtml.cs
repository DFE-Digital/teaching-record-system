using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.FormFlow;

namespace TeachingRecordSystem.AuthorizeAccess.Pages;

[Journey(SignInJourneyState.JourneyName), RequireJourneyInstance]
public class TrnModel(SignInJourneyHelper helper) : PageModel
{
    public JourneyInstance<SignInJourneyState>? JourneyInstance { get; set; }

    [FromQuery]
    public bool? FromCheckAnswers { get; set; }

    [BindProperty]
    [Display(Name = "Do you have a teacher reference number (TRN)?")]
    [Required(ErrorMessage = "Select yes if you have a TRN")]
    public bool? HaveTrn { get; set; }

    [BindProperty]
    [Display(Name = "TRN")]
    [Required(ErrorMessage = "Enter your TRN")]
    [RegularExpression(@"\A\D*(\d{1}\D*){7}\D*\Z", ErrorMessage = "Your TRN should contain 7 digits")]
    public string? Trn { get; set; }

    public void OnGet()
    {
        HaveTrn = JourneyInstance!.State.HaveTrn;
        Trn = JourneyInstance!.State.Trn;
    }

    public async Task<IActionResult> OnPost()
    {
        if (HaveTrn != true)
        {
            ModelState.Remove(nameof(Trn));
        }

        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        await JourneyInstance!.UpdateStateAsync(state => state.SetTrn(HaveTrn!.Value, Trn));

        return await helper.TryMatchToTeachingRecord(JourneyInstance!) ? Redirect(helper.LinkGenerator.Found(JourneyInstance.InstanceId)) :
            Redirect(helper.LinkGenerator.CheckAnswers(JourneyInstance.InstanceId));
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
        else if (!state.HaveNationalInsuranceNumber.HasValue)
        {
            // Not answered the NINO question
            context.Result = Redirect(helper.LinkGenerator.NationalInsuranceNumber(JourneyInstance.InstanceId));
        }
    }
}
