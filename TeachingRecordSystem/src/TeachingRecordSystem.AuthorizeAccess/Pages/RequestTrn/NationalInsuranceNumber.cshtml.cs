using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.WebCommon.FormFlow;

namespace TeachingRecordSystem.AuthorizeAccess.Pages.RequestTrn;

[WebCommon.FormFlow.Journey(RequestTrnJourneyState.JourneyName), RequireJourneyInstance]
public class NationalInsuranceNumberModel(RequestTrnLinkGenerator linkGenerator) : PageModel
{
    public JourneyInstance<RequestTrnJourneyState>? JourneyInstance { get; set; }

    [FromQuery]
    public bool? FromCheckAnswers { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Select yes if you have a National Insurance number")]
    public bool? HasNationalInsuranceNumber { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Enter your National Insurance number")]
    public string? NationalInsuranceNumber { get; set; }

    public void OnGet()
    {
        HasNationalInsuranceNumber = JourneyInstance?.State.HasNationalInsuranceNumber;
        NationalInsuranceNumber = JourneyInstance?.State.NationalInsuranceNumber;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (HasNationalInsuranceNumber == true)
        {
            if (!string.IsNullOrEmpty(NationalInsuranceNumber) && !Core.NationalInsuranceNumber.TryParse(NationalInsuranceNumber, out _))
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
            return Redirect(linkGenerator.Address(JourneyInstance!.InstanceId));
        }

        return Redirect(linkGenerator.CheckAnswers(JourneyInstance.InstanceId));
    }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        var state = JourneyInstance!.State;
        if (state.HasPendingTrnRequest)
        {
            context.Result = Redirect(linkGenerator.Submitted(JourneyInstance!.InstanceId));
        }
        else if (state.EvidenceFileId is null)
        {
            context.Result = Redirect(linkGenerator.Identity(JourneyInstance.InstanceId));
        }
    }
}
