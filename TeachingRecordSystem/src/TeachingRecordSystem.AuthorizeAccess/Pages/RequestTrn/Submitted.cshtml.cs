using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.UiCommon.FormFlow;

namespace TeachingRecordSystem.AuthorizeAccess.Pages.RequestTrn;

[Journey(RequestTrnJourneyState.JourneyName), RequireJourneyInstance]
public class SubmittedModel(AuthorizeAccessLinkGenerator linkGenerator) : PageModel
{
    public JourneyInstance<RequestTrnJourneyState>? JourneyInstance { get; set; }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        if (!JourneyInstance!.State.HasPendingTrnRequest)
        {
            context.Result = Redirect(linkGenerator.RequestTrnCheckAnswers(JourneyInstance!.InstanceId));
        }
    }
}
