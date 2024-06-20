using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.UiCommon.FormFlow;

namespace TeachingRecordSystem.AuthorizeAccess.Pages;

[Journey(SignInJourneyState.JourneyName), RequireJourneyInstance]
public class FoundModel(SignInJourneyHelper helper) : PageModel
{
    public JourneyInstance<SignInJourneyState>? JourneyInstance { get; set; }

    public void OnGet()
    {
    }

    public IActionResult OnPost() => helper.GetNextPage(JourneyInstance!).ToActionResult();

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        var state = JourneyInstance!.State;

        if (state.AuthenticationTicket is null)
        {
            // Not matched
            context.Result = helper.GetNextPage(JourneyInstance).ToActionResult();
        }
    }
}
