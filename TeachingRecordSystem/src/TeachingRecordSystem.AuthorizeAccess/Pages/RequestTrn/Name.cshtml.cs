using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.WebCommon.FormFlow;

namespace TeachingRecordSystem.AuthorizeAccess.Pages.RequestTrn;

[Journey(RequestTrnJourneyState.JourneyName), RequireJourneyInstance]
public class NameModel(AuthorizeAccessLinkGenerator linkGenerator) : PageModel
{
    public JourneyInstance<RequestTrnJourneyState>? JourneyInstance { get; set; }

    [FromQuery]
    public bool? FromCheckAnswers { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Enter your first name")]
    [MaxLength(Person.FirstNameMaxLength, ErrorMessage = $"First name must be 100 characters or less")]
    public string? FirstName { get; set; }

    [BindProperty]
    [MaxLength(Person.FirstNameMaxLength, ErrorMessage = $"Middle name must be 100 characters or less")]
    public string? MiddleName { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Enter your last name")]
    [MaxLength(Person.FirstNameMaxLength, ErrorMessage = $"Last name must be 100 characters or less")]

    public string? LastName { get; set; }
    public void OnGet()
    {
        FirstName = JourneyInstance!.State.FirstName;
        MiddleName = JourneyInstance!.State.MiddleName;
        LastName = JourneyInstance!.State.LastName;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        await JourneyInstance!.UpdateStateAsync(state =>
        {
            state.FirstName = FirstName;
            state.MiddleName = MiddleName;
            state.LastName = LastName;
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
