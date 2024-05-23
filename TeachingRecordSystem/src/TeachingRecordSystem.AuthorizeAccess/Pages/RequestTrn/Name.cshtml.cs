using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.FormFlow;

namespace TeachingRecordSystem.AuthorizeAccess.Pages.RequestTrn;

[Journey(RequestTrnJourneyState.JourneyName), RequireJourneyInstance]
public class NameModel(AuthorizeAccessLinkGenerator linkGenerator) : PageModel
{
    public JourneyInstance<RequestTrnJourneyState>? JourneyInstance { get; set; }

    [BindProperty]
    [Display(Name = "What is your name?", Description = "Full name")]
    [Required(ErrorMessage = "Enter your name")]
    [MaxLength(200, ErrorMessage = "Name must be 200 characters or less")]
    public string? Name { get; set; }

    public async Task<IActionResult> OnPost()
    {
        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        await JourneyInstance!.UpdateStateAsync(state =>
        {
            state.Name = Name;
        });

        return Redirect(linkGenerator.RequestTrnPreviousName(JourneyInstance!.InstanceId));
    }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        if (JourneyInstance!.State.Email is null)
        {
            context.Result = Redirect(linkGenerator.RequestTrnEmail(JourneyInstance.InstanceId));
            return;
        }

        Name ??= JourneyInstance!.State.Name;
    }
}
