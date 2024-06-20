using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.UiCommon.FormFlow;

namespace TeachingRecordSystem.AuthorizeAccess.Pages.RequestTrn;

[Journey(RequestTrnJourneyState.JourneyName), RequireJourneyInstance]
public class DateOfBirthModel(AuthorizeAccessLinkGenerator linkGenerator, IClock clock) : PageModel
{
    public JourneyInstance<RequestTrnJourneyState>? JourneyInstance { get; set; }

    [FromQuery]
    public bool? FromCheckAnswers { get; set; }

    [BindProperty]
    [Display(Name = "What is your date of birth?")]
    [Required(ErrorMessage = "Enter your date of birth")]
    public DateOnly? DateOfBirth { get; set; }

    public async Task<IActionResult> OnPost()
    {
        if (DateOfBirth >= clock.Today)
        {
            ModelState.AddModelError(nameof(DateOfBirth), "Date of birth must be in the past");
        }

        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        await JourneyInstance!.UpdateStateAsync(state => state.DateOfBirth = DateOfBirth);

        return FromCheckAnswers == true ?
            Redirect(linkGenerator.RequestTrnCheckAnswers(JourneyInstance!.InstanceId)) :
            Redirect(linkGenerator.RequestTrnIdentity(JourneyInstance!.InstanceId));
    }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        var state = JourneyInstance!.State;
        if (state.HasPendingTrnRequest)
        {
            context.Result = Redirect(linkGenerator.RequestTrnSubmitted(JourneyInstance!.InstanceId));
        }
        else if (state.HasPreviousName is null)
        {
            context.Result = Redirect(linkGenerator.RequestTrnPreviousName(JourneyInstance.InstanceId));
            return;
        }

        DateOfBirth ??= JourneyInstance!.State.DateOfBirth;
    }
}
