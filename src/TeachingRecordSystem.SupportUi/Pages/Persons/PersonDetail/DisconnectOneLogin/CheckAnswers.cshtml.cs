using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Events.ChangeReasons;
using TeachingRecordSystem.Core.Services.OneLogin;
using TeachingRecordSystem.Core.Services.Persons;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.DisconnectOneLogin;

[Journey(JourneyNames.DisconnectOneLogin)]
public class CheckAnswers(
    DisconnectOneLoginJourneyCoordinator journey,
    SupportUiLinkGenerator linkGenerator,
    TimeProvider timeProvider,
    OneLoginService oneLoginService,
    TrsDbContext dbContext)
    : PageModel
{
    [FromRoute] public required Guid PersonId { get; set; }

    [FromRoute] public required string OneLoginSubject { get; set; }


    public JourneyInstanceId InstanceId => journey.InstanceId;

    public string? BackLink { get; set; }

    [BindProperty]
    public bool Cancel { get; set; }

    public string? EmailAddress { get; set; }

    public DisconnectOneLoginReason? Reason { get; set; }

    public string? Detail { get; set; }

    public DisconnectOneLoginStayVerified? StayVerified { get; set; }

    public async Task OnGetAsync()
    {
        var oneLogin = await dbContext.OneLoginUsers.SingleAsync(x => x.Subject == OneLoginSubject);
        EmailAddress = oneLogin.EmailAddress;
        Reason = journey.State.DisconnectReason;
        Detail = journey.State.DisconnectReason == DisconnectOneLoginReason.AnotherReason ? journey.State.Detail : null;
        StayVerified = journey.State.StayVerified;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Cancel)
        {
            journey.DeleteInstance();
            return Redirect(linkGenerator.Persons.PersonDetail.Index(PersonId));
        }

        var changeReason = new ChangeReasonWithDetailsAndEvidence()
        {
            Reason = Reason?.GetDisplayName(),
            Details = journey.State.DisconnectReason == DisconnectOneLoginReason.AnotherReason
                ? journey.State.Detail
                : null,
            EvidenceFile = null,
            AdditionalInformation = null
        };
        var processContext = new ProcessContext(ProcessType.PersonOneLoginUserDisconnecting, timeProvider.UtcNow, User.GetUserId(), changeReason: changeReason);
        var person = await dbContext.Persons.SingleAsync(x => x.PersonId == PersonId);
        if (journey.State.StayVerified == DisconnectOneLoginStayVerified.Yes)
        {
            await oneLoginService.SetUserUnmatchedAsync(OneLoginSubject, processContext);
        }
        else
        {
            await oneLoginService.SetUserUnverifiedAndUnmatchedAsync(OneLoginSubject, processContext);

        }

        var personName = $"{person.FirstName} {person.LastName}";
        TempData.SetFlashNotificationBanner($"GOV.UK One Login disconnected from {personName}’s record");

        journey.DeleteInstance();
        return Redirect(linkGenerator.Persons.PersonDetail.Index(PersonId));
    }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        BackLink = journey.GetBackLink();
    }
}
