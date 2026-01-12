using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.WebCommon.FormFlow;

namespace TeachingRecordSystem.AuthorizeAccess.Pages.RequestTrn;

[WebCommon.FormFlow.Journey(RequestTrnJourneyState.JourneyName), RequireJourneyInstance]
public class TakingNpqModel(RequestTrnLinkGenerator linkGenerator) : PageModel
{
    public JourneyInstance<RequestTrnJourneyState>? JourneyInstance { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Select yes if you need a TRN because you’re taking an NPQ")]
    public bool? IsTakingAnNpq { get; set; }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        await JourneyInstance!.UpdateStateAsync(state => state.IsTakingNpq = IsTakingAnNpq);

        return IsTakingAnNpq == true ?
            Redirect(linkGenerator.NpqCheck(JourneyInstance!.InstanceId)) :
            Redirect(linkGenerator.NotEligible(JourneyInstance.InstanceId));
    }
}
