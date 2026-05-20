using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.OneLoginUserMatching.Resolve;

[Journey(JourneyNames.ResolveOneLoginUserMatching), ActivatesJourney, RequireJourneyInstance]
public class IndexModel(SupportUiLinkGenerator linkGenerator) : PageModel
{
    public JourneyInstance<ResolveOneLoginUserMatchingState>? JourneyInstance { get; set; }

    [FromRoute]
    public required string? SupportTaskReference { get; set; }

    public void OnGet()
    {
    }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        var supportTask = HttpContext.GetCurrentSupportTaskFeature().SupportTask;
        if (supportTask.SupportTaskType == SupportTaskType.OneLoginUserRecordMatching)
        {
            context.Result = Redirect(JourneyInstance!.State.MatchedPersons.Count > 0 ?
                linkGenerator.SupportTasks.OneLoginUserMatching.Resolve.Matches(SupportTaskReference!, JourneyInstance!.InstanceId) :
                linkGenerator.SupportTasks.OneLoginUserMatching.Resolve.NoMatches(SupportTaskReference!, JourneyInstance!.InstanceId));
        }
        else
        {
            context.Result = Redirect(linkGenerator.SupportTasks.OneLoginUserMatching.Resolve.Verify(SupportTaskReference!, JourneyInstance!.InstanceId));
        }
    }
}
