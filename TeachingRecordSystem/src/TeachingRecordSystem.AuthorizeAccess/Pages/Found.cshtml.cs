using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.FormFlow;

namespace TeachingRecordSystem.AuthorizeAccess.Pages;

[Journey(SignInJourneyState.JourneyName), RequireJourneyInstance]
public class FoundModel(SignInJourneyHelper helper) : PageModel
{
    public JourneyInstance<SignInJourneyState>? JourneyInstance { get; set; }

    public void OnGet()
    {
    }

    public IActionResult OnPost() => Redirect(helper.GetNextPage(JourneyInstance!));

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        var state = JourneyInstance!.State;

        if (state.AuthenticationTicket is null)
        {
            // Not matched
            context.Result = Redirect(helper.GetNextPage(JourneyInstance));
        }
    }
}
