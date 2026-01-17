using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.WebCommon.FormFlow;
using EmailAddress = TeachingRecordSystem.AuthorizeAccess.DataAnnotations.EmailAddressAttribute;

namespace TeachingRecordSystem.AuthorizeAccess.Pages.RequestTrn;

[WebCommon.FormFlow.Journey(RequestTrnJourneyState.JourneyName), RequireJourneyInstance]
public class PersonalEmailModel(RequestTrnLinkGenerator linkGenerator, TrsDbContext dbContext) : PageModel
{
    public JourneyInstance<RequestTrnJourneyState>? JourneyInstance { get; set; }

    [FromQuery]
    public bool? FromCheckAnswers { get; set; }

    [BindProperty]
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

        var openTasks = await SearchForOpenTasksForEmailAddressAsync(emailAddress: JourneyInstance!.State.PersonalEmail!);

        if (openTasks)
        {
            return Redirect(linkGenerator.EmailInUse(JourneyInstance!.InstanceId));
        }

        return FromCheckAnswers == true ?
            Redirect(linkGenerator.CheckAnswers(JourneyInstance!.InstanceId)) :
            Redirect(linkGenerator.Name(JourneyInstance.InstanceId));
    }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        var state = JourneyInstance!.State;
        if (state.HasPendingTrnRequest)
        {
            context.Result = Redirect(linkGenerator.Submitted(JourneyInstance!.InstanceId));
            return;
        }
    }

    private Task<bool> SearchForOpenTasksForEmailAddressAsync(string emailAddress) =>
        dbContext.SupportTasks.AnyAsync(s =>
            s.Status == SupportTaskStatus.Open &&
            s.SupportTaskType == SupportTaskType.NpqTrnRequest &&
            s.TrnRequestMetadata!.EmailAddress != null &&
            EF.Functions.Collate(s.TrnRequestMetadata.EmailAddress, Collations.CaseInsensitive) == emailAddress);
}
