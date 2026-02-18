using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.Services.Persons;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.DisconnectOneLogin;

[Journey(JourneyNames.DisconnectOneLogin), RequireJourneyInstance]
public class Verified(
    SupportUiLinkGenerator linkGenerator,
    IClock clock)
    : PageModel
{
    [FromRoute] public required Guid PersonId { get; set; }

    [FromRoute] public required string OneLoginSubject { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Select yes if you want to keep the person verified")]
    public DisconnectOneLoginStayVerified? StayVerified { get; set; }

    public JourneyInstance<DisconnectOneLoginState>? JourneyInstance { get; set; }

    [FromQuery] public bool? FromCheckAnswers { get; set; }

    public void OnGet()
    {
        StayVerified = JourneyInstance?.State.StayVerified;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        await JourneyInstance!.UpdateStateAsync(state =>
        {
            state.StayVerified = StayVerified;
            state.OneLoginSubject = OneLoginSubject;
        });


        return Redirect(linkGenerator.Persons.PersonDetail.DisconnectOneLogin.CheckAnswers(PersonId, OneLoginSubject, JourneyInstance.InstanceId));
    }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await JourneyInstance!.DeleteAsync();
        return Redirect(linkGenerator.Persons.PersonDetail.Index(PersonId));
    }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        if (!JourneyInstance!.State.DisconnectReason.HasValue)
        {
            context.Result = Redirect(linkGenerator.Persons.PersonDetail.DisconnectOneLogin.DisconnectOneLoginSubject(PersonId, OneLoginSubject, JourneyInstance.InstanceId));
            return;
        }
    }
}
