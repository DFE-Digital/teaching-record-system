using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.AuthorizeAccess.DataAnnotations;
using TeachingRecordSystem.FormFlow;

namespace TeachingRecordSystem.AuthorizeAccess.Pages.RequestTrn;

[Journey(RequestTrnJourneyState.JourneyName), RequireJourneyInstance]
public class NameModel(AuthorizeAccessLinkGenerator linkGenerator) : PageModel
{
    public JourneyInstance<RequestTrnJourneyState>? JourneyInstance { get; set; }

    [BindProperty]
    [Display(Name = "What is your name?", Description = "Full name")]
    [Required(ErrorMessage = "Enter your name")]
    public string? Name { get; set; }

    [BindProperty]
    [Display(Name = "Do you have a previous name?")]
    [Required(ErrorMessage = "Tell us if you have a previous name")]
    public bool? HasPreviousName { get; set; }

    [BindProperty]
    [Display(Name = "Previous full name")]
    [RequiredIfOtherPropertyEquals(nameof(HasPreviousName), ErrorMessage = "Tell us your previous name")]
    public string? PreviousName { get; set; }

    public async Task<IActionResult> OnPost()
    {
        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        await JourneyInstance!.UpdateStateAsync(state =>
        {
            state.Name = Name;
            state.HasPreviousName = HasPreviousName;
            state.PreviousName = HasPreviousName!.Value ? PreviousName : null;
        });

        return Redirect(linkGenerator.RequestTrnDateOfBirth(JourneyInstance!.InstanceId));
    }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        if (JourneyInstance!.State.Email is null)
        {
            context.Result = Redirect(linkGenerator.RequestTrnEmail(JourneyInstance.InstanceId));
            return;
        }

        Name ??= JourneyInstance!.State.Name;
        HasPreviousName ??= JourneyInstance!.State.HasPreviousName;
        PreviousName ??= JourneyInstance!.State.PreviousName;
    }
}
