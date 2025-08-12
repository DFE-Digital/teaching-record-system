using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.WebCommon.FormFlow;

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
    [Display(Name = "Previous first name")]
    [Required(ErrorMessage = "Enter your previous first name")]
    [MaxLength(Person.FirstNameMaxLength, ErrorMessage = $"Previous first name must be 100 characters or less")]
    public string? FirstName { get; set; }

    [BindProperty]
    [Display(Name = "Previous middle name (optional)")]
    [MaxLength(Person.FirstNameMaxLength, ErrorMessage = $"Previous middle name must be 100 characters or less")]
    public string? MiddleName { get; set; }

    [BindProperty]
    [Display(Name = "Previous last name")]
    [Required(ErrorMessage = "Enter your previous last name")]
    [MaxLength(Person.FirstNameMaxLength, ErrorMessage = $"Previous last name must be 100 characters or less")]
    public string? LastName { get; set; }

    public void OnGet()
    {
        HasPreviousName = JourneyInstance!.State.HasPreviousName;
        FirstName = JourneyInstance!.State.PreviousFirstName;
        MiddleName = JourneyInstance!.State.PreviousMiddleName;
        LastName = JourneyInstance!.State.PreviousLastName;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        await JourneyInstance!.UpdateStateAsync(state =>
        {
            state.HasPreviousName = HasPreviousName;
            state.PreviousFirstName = FirstName;
            state.PreviousMiddleName = MiddleName;
            state.PreviousLastName = LastName;
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
        else if (state.FirstName is null || state.LastName is null)
        {
            context.Result = Redirect(linkGenerator.RequestTrnName(JourneyInstance.InstanceId));
            return;
        }
    }
}
