using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Services.OneLogin;
using TeachingRecordSystem.Core.Services.Persons;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.DisconnectOneLogin;

[Journey(JourneyNames.DisconnectOneLogin), RequireJourneyInstance]
public class CheckAnswers(
    SupportUiLinkGenerator linkGenerator,
    IClock clock,
    OneLoginService oneLoginService,
    TrsDbContext dbContext)
    : PageModel
{
    [FromRoute] public required Guid PersonId { get; set; }

    [FromRoute] public required string OneLoginSubject { get; set; }

    public JourneyInstance<DisconnectOneLoginState>? JourneyInstance { get; set; }

    public string? EmailAddress { get; set; }

    public DisconnectOneLoginReason? Reason { get; set; }

    public string? Detail { get; set; }

    public DisconnectOneLoginStayVerified? StayVerified { get; set; }

    public async Task OnGetAsync()
    {
        var oneLogin = await dbContext.OneLoginUsers.SingleAsync(x => x.Subject == OneLoginSubject);
        EmailAddress = oneLogin.EmailAddress;
        Reason = JourneyInstance!.State.DisconnectReason;
        Detail = JourneyInstance!.State.DisconnectReason == DisconnectOneLoginReason.AnotherReason ? JourneyInstance.State.Detail : null;
        StayVerified = JourneyInstance!.State.StayVerified;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var processContext = new ProcessContext(ProcessType.PersonOneLoginUserDisconnecting, clock.UtcNow, User.GetUserId());
        var person = await dbContext.Persons.SingleAsync(x => x.PersonId == PersonId);
        if (JourneyInstance!.State.StayVerified == DisconnectOneLoginStayVerified.Yes)
        {
            await oneLoginService.SetUserUnmatchedAsync(OneLoginSubject, processContext);
        }
        else
        {
            await oneLoginService.SetUserUnverifiedAndUnmatchedAsync(OneLoginSubject, processContext);

        }

        var personName = $"{person.FirstName} {person.LastName}";
        TempData.SetFlashSuccess($"GOV.UK One Login disconnected from {personName}`s record");
        return Redirect(linkGenerator.Persons.PersonDetail.Index(PersonId));
    }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await JourneyInstance!.DeleteAsync();
        return Redirect(linkGenerator.Persons.PersonDetail.Index(PersonId));
    }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        if (!JourneyInstance!.State.DisconnectReason.HasValue || !JourneyInstance.State.StayVerified.HasValue)
        {
            context.Result = Redirect(linkGenerator.Persons.PersonDetail.DisconnectOneLogin.DisconnectOneLoginSubject(PersonId, OneLoginSubject, JourneyInstance.InstanceId));
            return;
        }
    }
}
