using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.WebCommon.FormFlow;
using EmailAddress = TeachingRecordSystem.AuthorizeAccess.DataAnnotations.EmailAddressAttribute;

namespace TeachingRecordSystem.AuthorizeAccess.Pages.RequestTrn;

[Journey(RequestTrnJourneyState.JourneyName), RequireJourneyInstance]
public class PersonalEmailModel(AuthorizeAccessLinkGenerator linkGenerator, TrsDbContext dbContext) : PageModel
{
    public JourneyInstance<RequestTrnJourneyState>? JourneyInstance { get; set; }

    [FromQuery]
    public bool? FromCheckAnswers { get; set; }

    [BindProperty]
    [Display(Name = "What is your personal email address?", Description = "We need this to send you your TRN if you’re eligible for one. Do not use your work email address.")]
    [Required(ErrorMessage = "Enter your personal email address")]
    [@EmailAddress(ErrorMessage = "Enter an email address in the correct format, like name@example.com")]
    public string? PersonalEmail { get; set; }

    public void OnGet()
    {
        PersonalEmail = JourneyInstance!.State.PersonalEmail;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        await JourneyInstance!.UpdateStateAsync(state => state.PersonalEmail = PersonalEmail);

        var openTasks = SearchForOpenTasksForEmailAddress(emailAddress: JourneyInstance!.State.PersonalEmail!);

        if (openTasks)
        {
            return Redirect(linkGenerator.RequestTrnEmailInUse(JourneyInstance!.InstanceId));
        }

        return FromCheckAnswers == true ?
            Redirect(linkGenerator.RequestTrnCheckAnswers(JourneyInstance!.InstanceId)) :
            Redirect(linkGenerator.RequestTrnName(JourneyInstance.InstanceId));
    }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        var state = JourneyInstance!.State;
        if (state.HasPendingTrnRequest)
        {
            context.Result = Redirect(linkGenerator.RequestTrnSubmitted(JourneyInstance!.InstanceId));
            return;
        }
    }

    private bool SearchForOpenTasksForEmailAddress(string emailAddress)
    {
        var openTasks = dbContext.SupportTasks
            .Where(s => s.Status == SupportTaskStatus.Open && s.SupportTaskType == SupportTaskType.NpqTrnRequest)
            .Include(s => s.TrnRequestMetadata)
            .Where(s => s.TrnRequestMetadata!.EmailAddress == emailAddress)
            .Any();
        return openTasks;
    }
}
