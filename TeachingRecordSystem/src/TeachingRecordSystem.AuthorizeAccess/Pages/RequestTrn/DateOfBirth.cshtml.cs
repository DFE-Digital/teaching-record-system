using System.ComponentModel.DataAnnotations;
using GovUk.Frontend.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.WebCommon.FormFlow;

namespace TeachingRecordSystem.AuthorizeAccess.Pages.RequestTrn;

[WebCommon.FormFlow.Journey(RequestTrnJourneyState.JourneyName), RequireJourneyInstance]
public class DateOfBirthModel(RequestTrnLinkGenerator linkGenerator, TimeProvider timeProvider) : PageModel
{
    public JourneyInstance<RequestTrnJourneyState>? JourneyInstance { get; set; }

    [FromQuery]
    public bool? FromCheckAnswers { get; set; }

    [BindProperty]
    [DateInput(ErrorMessagePrefix = "Date of birth")]
    [Required(ErrorMessage = "Enter your date of birth")]
    public DateOnly? DateOfBirth { get; set; }

    public void OnGet()
    {
        DateOfBirth = JourneyInstance!.State.DateOfBirth;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (DateOfBirth >= timeProvider.Today)
        {
            ModelState.AddModelError(nameof(DateOfBirth), "Date of birth must be in the past");
        }

        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        await JourneyInstance!.UpdateStateAsync(state => state.DateOfBirth = DateOfBirth);

        return FromCheckAnswers == true ?
            Redirect(linkGenerator.CheckAnswers(JourneyInstance!.InstanceId)) :
            Redirect(linkGenerator.Identity(JourneyInstance!.InstanceId));
    }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        var state = JourneyInstance!.State;
        if (state.HasPendingTrnRequest)
        {
            context.Result = Redirect(linkGenerator.Submitted(JourneyInstance!.InstanceId));
        }
        else if (state.HasPreviousName is null)
        {
            context.Result = Redirect(linkGenerator.PreviousName(JourneyInstance.InstanceId));
        }
    }
}
