using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Queries;
using TeachingRecordSystem.UiCommon.FormFlow;
using EmailAddress = TeachingRecordSystem.AuthorizeAccess.DataAnnotations.EmailAddressAttribute;

namespace TeachingRecordSystem.AuthorizeAccess.Pages.RequestTrn;

[Journey(RequestTrnJourneyState.JourneyName), RequireJourneyInstance]
public class EmailModel(AuthorizeAccessLinkGenerator linkGenerator, ICrmQueryDispatcher crmQueryDispatcher) : PageModel
{
    public JourneyInstance<RequestTrnJourneyState>? JourneyInstance { get; set; }

    [FromQuery]
    public bool? FromCheckAnswers { get; set; }

    [BindProperty]
    [Display(Name = "What is your email address?", Description = "We’ll only use this to send you your TRN if you’re eligible for one.")]
    [Required(ErrorMessage = "Enter your email address")]
    [@EmailAddress(ErrorMessage = "Enter an email address in the correct format, like name@example.com")]
    public string? Email { get; set; }

    public async Task<IActionResult> OnPost()
    {
        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        await JourneyInstance!.UpdateStateAsync(state => state.Email = Email);

        var openTasks = await crmQueryDispatcher.ExecuteQuery(
            new GetOpenTasksForEmailAddressQuery(EmailAddress: JourneyInstance!.State.Email!));

        if (openTasks.Any())
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

        Email ??= JourneyInstance!.State.Email;
    }
}
