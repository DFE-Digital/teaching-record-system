using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.WebCommon.FormFlow;

namespace TeachingRecordSystem.AuthorizeAccess.Pages.RequestTrn;

[WebCommon.FormFlow.Journey(RequestTrnJourneyState.JourneyName), RequireJourneyInstance]
public class NpqCheckModel(RequestTrnLinkGenerator linkGenerator) : PageModel
{
    public JourneyInstance<RequestTrnJourneyState>? JourneyInstance { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Select yes if you’ve already registered for an NPQ")]
    public bool? HaveRegisteredForAnNpq { get; set; }

    public void OnGet()
    {
        HaveRegisteredForAnNpq = JourneyInstance!.State.HaveRegisteredForAnNpq;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        await JourneyInstance!.UpdateStateAsync(state => state.HaveRegisteredForAnNpq = HaveRegisteredForAnNpq);

        return Redirect(linkGenerator.NpqName(JourneyInstance.InstanceId));
    }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        var state = JourneyInstance!.State;
        if (state.HasPendingTrnRequest)
        {
            context.Result = Redirect(linkGenerator.Submitted(JourneyInstance!.InstanceId));
        }
    }
}
