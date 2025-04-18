using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.WebCommon.FormFlow;

namespace TeachingRecordSystem.AuthorizeAccess.Pages.RequestTrn;

[Journey(RequestTrnJourneyState.JourneyName), RequireJourneyInstance]
public class NameModel(AuthorizeAccessLinkGenerator linkGenerator) : PageModel
{
    public JourneyInstance<RequestTrnJourneyState>? JourneyInstance { get; set; }

    [FromQuery]
    public bool? FromCheckAnswers { get; set; }

    [BindProperty]
    [Display(Name = "What is your name?", Description = "Full name")]
    [Required(ErrorMessage = "Enter your name")]
    [MaxLength(200, ErrorMessage = "Name must be 200 characters or less")]
    public string? Name { get; set; }

    public void OnGet()
    {
        Name = JourneyInstance!.State.Name;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        await JourneyInstance!.UpdateStateAsync(state =>
        {
            state.Name = Name;
        });

        return FromCheckAnswers == true ?
            Redirect(linkGenerator.RequestTrnCheckAnswers(JourneyInstance!.InstanceId)) :
            Redirect(linkGenerator.RequestTrnPreviousName(JourneyInstance!.InstanceId));
    }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        var state = JourneyInstance!.State;
        if (state.HasPendingTrnRequest)
        {
            context.Result = Redirect(linkGenerator.RequestTrnSubmitted(JourneyInstance!.InstanceId));
        }
        else if (state.PersonalEmail is null)
        {
            context.Result = Redirect(linkGenerator.RequestTrnPersonalEmail(JourneyInstance.InstanceId));
        }
        else if (state.WorkEmail is null && state.WorkingInSchoolOrEducationalSetting == true)
        {
            context.Result = Redirect(linkGenerator.RequestTrnWorkEmail(JourneyInstance.InstanceId));
        }
    }
}
