using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.UiCommon.FormFlow;

namespace TeachingRecordSystem.AuthorizeAccess.Pages.RequestTrn;

[Journey(RequestTrnJourneyState.JourneyName), RequireJourneyInstance]
public class NpqCheckModel(AuthorizeAccessLinkGenerator linkGenerator) : PageModel
{
    public JourneyInstance<RequestTrnJourneyState>? JourneyInstance { get; set; }

    [BindProperty]
    [Display(Name = "Do you plan on taking a national professional qualification (NPQ)?")]
    [Required(ErrorMessage = "Tell us whether you plan on taking a national professional qualification (NPQ)")]
    public bool? IsPlanningToTakeAnNpq { get; set; }

    public async Task<IActionResult> OnPost()
    {
        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        await JourneyInstance!.UpdateStateAsync(state => state.IsPlanningToTakeAnNpq = IsPlanningToTakeAnNpq);

        return IsPlanningToTakeAnNpq == true ?
            Redirect(linkGenerator.RequestTrnEmail(JourneyInstance!.InstanceId)) :
            Redirect(linkGenerator.RequestTrnNotEligible(JourneyInstance.InstanceId));
    }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        var state = JourneyInstance!.State;
        if (state.HasPendingTrnRequest)
        {
            context.Result = Redirect(linkGenerator.RequestTrnSubmitted(JourneyInstance!.InstanceId));
            return;
        }

        IsPlanningToTakeAnNpq ??= JourneyInstance!.State.IsPlanningToTakeAnNpq;
    }
}
