using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.FormFlow;

namespace TeachingRecordSystem.AuthorizeAccess.Pages.RequestTrn;

[Journey(RequestTrnJourneyState.JourneyName), RequireJourneyInstance]
public class NationalInsuranceNumberModel(AuthorizeAccessLinkGenerator linkGenerator) : PageModel
{
    public JourneyInstance<RequestTrnJourneyState>? JourneyInstance { get; set; }

    [BindProperty]
    [Display(Name = "Do you have a National Insurance number?")]
    [Required(ErrorMessage = "Tell us if you have a National Insurance number")]
    public bool? HasNationalInsuranceNumber { get; set; }

    [BindProperty]
    [Display(Name = "National Insurance number")]
    [Required(ErrorMessage = "Enter your National Insurance number")]
    public string? NationalInsuranceNumber { get; set; }

    public async Task<IActionResult> OnPost()
    {
        if (HasNationalInsuranceNumber == true)
        {
            if (!string.IsNullOrEmpty(NationalInsuranceNumber) && !NationalInsuranceNumberHelper.IsValid(NationalInsuranceNumber))
            {
                ModelState.AddModelError(nameof(NationalInsuranceNumber), "Enter a valid National Insurance number");
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
            state.HasNationalInsuranceNumber = HasNationalInsuranceNumber!.Value;
            state.NationalInsuranceNumber = NationalInsuranceNumber;
        });

        if (HasNationalInsuranceNumber == false)
        {
            return Redirect(linkGenerator.RequestTrnAddress(JourneyInstance!.InstanceId));
        }

        return Redirect(linkGenerator.RequestTrnCheckAnswers(JourneyInstance.InstanceId));
    }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        if (JourneyInstance!.State.EvidenceFileId is null)
        {
            context.Result = Redirect(linkGenerator.RequestTrnIdentity(JourneyInstance.InstanceId));
            return;
        }

        HasNationalInsuranceNumber ??= JourneyInstance?.State.HasNationalInsuranceNumber;
        NationalInsuranceNumber ??= JourneyInstance?.State.NationalInsuranceNumber;
    }
}
