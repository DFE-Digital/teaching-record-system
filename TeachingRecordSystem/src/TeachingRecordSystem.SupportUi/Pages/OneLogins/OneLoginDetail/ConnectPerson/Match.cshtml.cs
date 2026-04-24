using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.SupportUi.Infrastructure.Filters;

namespace TeachingRecordSystem.SupportUi.Pages.OneLogins.OneLoginDetail.ConnectPerson;

[Journey(JourneyNames.ConnectPerson)]
[RequireJourneyInstance]
[TypeFilter(typeof(CheckOneLoginUserExistsFilterFactory))]
public class MatchModel(SupportUiLinkGenerator linkGenerator) : PageModel
{
    public JourneyInstance<ConnectPersonState>? JourneyInstance { get; set; }

    [FromRoute]
    public string OneLoginUserSubject { get; set; } = null!;

    public string? PersonTrn { get; set; }
    public string? OneLoginUserEmailAddress { get; set; }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await JourneyInstance!.DeleteAsync();
        return Redirect(linkGenerator.Index());
    }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        if (!JourneyInstance!.State.PersonId.HasValue)
        {
            context.Result = NotFound();
            return;
        }

        var oneLoginUserFeature = context.HttpContext.GetCurrentOneLoginUserFeature();
        OneLoginUserEmailAddress = oneLoginUserFeature.EmailAddress;
        PersonTrn = JourneyInstance.State.PersonTrn;
    }
}
