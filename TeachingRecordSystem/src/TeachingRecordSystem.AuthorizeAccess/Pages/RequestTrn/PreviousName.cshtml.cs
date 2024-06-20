using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.AuthorizeAccess.DataAnnotations;
using TeachingRecordSystem.UiCommon.FormFlow;

namespace TeachingRecordSystem.AuthorizeAccess.Pages.RequestTrn;

[Journey(RequestTrnJourneyState.JourneyName), RequireJourneyInstance]
public class PreviousNameModel(AuthorizeAccessLinkGenerator linkGenerator) : PageModel
{
    public JourneyInstance<RequestTrnJourneyState>? JourneyInstance { get; set; }

    [FromQuery]
    public bool? FromCheckAnswers { get; set; }

    [BindProperty]
    [Display(Name = "Have you ever changed your name?")]
    [Required(ErrorMessage = "Select yes if you’ve ever changed your name")]
    public bool? HasPreviousName { get; set; }

    [BindProperty]
    [Display(Name = "Previous full name")]
    [RequiredIfOtherPropertyEquals(nameof(HasPreviousName), ErrorMessage = "Enter your previous full name")]
    [MaxLength(200, ErrorMessage = "Previous name must be 200 characters or less")]
    public string? PreviousName { get; set; }

    public async Task<IActionResult> OnPost()
    {
        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        await JourneyInstance!.UpdateStateAsync(state =>
        {
            state.HasPreviousName = HasPreviousName;
            state.PreviousName = HasPreviousName!.Value ? PreviousName : null;
        });

        return FromCheckAnswers == true ?
            Redirect(linkGenerator.RequestTrnCheckAnswers(JourneyInstance!.InstanceId)) :
            Redirect(linkGenerator.RequestTrnDateOfBirth(JourneyInstance!.InstanceId));
    }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        var state = JourneyInstance!.State;
        if (state.HasPendingTrnRequest)
        {
            context.Result = Redirect(linkGenerator.RequestTrnSubmitted(JourneyInstance!.InstanceId));
        }
        else if (state.Name is null)
        {
            context.Result = Redirect(linkGenerator.RequestTrnName(JourneyInstance.InstanceId));
            return;
        }

        if (context.Result is null)
        {
            HasPreviousName ??= JourneyInstance!.State.HasPreviousName;
            PreviousName ??= JourneyInstance!.State.PreviousName;
        }
    }
}
