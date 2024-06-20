using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.UiCommon.FormFlow;

namespace TeachingRecordSystem.AuthorizeAccess.Pages.RequestTrn;

[Journey(RequestTrnJourneyState.JourneyName), RequireJourneyInstance]
public class NationalInsuranceNumberModel(AuthorizeAccessLinkGenerator linkGenerator) : PageModel
{
    public JourneyInstance<RequestTrnJourneyState>? JourneyInstance { get; set; }

    [FromQuery]
    public bool? FromCheckAnswers { get; set; }

    [BindProperty]
    [Display(Name = "Do you have a National Insurance number?")]
    [Required(ErrorMessage = "Select yes if you have a National Insurance number")]
    public bool? HasNationalInsuranceNumber { get; set; }

    [BindProperty]
    [Display(Name = "National Insurance number", Description = "It’s on your National Insurance card, benefit letter, payslip or P60. For example, ‘QQ 12 34 56 C’.")]
    [Required(ErrorMessage = "Enter your National Insurance number")]
    public string? NationalInsuranceNumber { get; set; }

    public async Task<IActionResult> OnPost()
    {
        if (HasNationalInsuranceNumber == true)
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

        await JourneyInstance!.UpdateStateAsync(state =>
        {
            var hasNationalInsuranceNumber = HasNationalInsuranceNumber!.Value;
            state.HasNationalInsuranceNumber = hasNationalInsuranceNumber;
            state.NationalInsuranceNumber = hasNationalInsuranceNumber ? NationalInsuranceNumber : null;
        });

        if (HasNationalInsuranceNumber == false)
        {
            return Redirect(linkGenerator.RequestTrnAddress(JourneyInstance!.InstanceId));
        }

        return Redirect(linkGenerator.RequestTrnCheckAnswers(JourneyInstance.InstanceId));
    }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        var state = JourneyInstance!.State;
        if (state.HasPendingTrnRequest)
        {
            context.Result = Redirect(linkGenerator.RequestTrnSubmitted(JourneyInstance!.InstanceId));
        }
        else if (state.EvidenceFileId is null)
        {
            context.Result = Redirect(linkGenerator.RequestTrnIdentity(JourneyInstance.InstanceId));
        }

        if (context.Result is null)
        {
            HasNationalInsuranceNumber ??= JourneyInstance?.State.HasNationalInsuranceNumber;
            NationalInsuranceNumber ??= JourneyInstance?.State.NationalInsuranceNumber;
        }
    }
}
