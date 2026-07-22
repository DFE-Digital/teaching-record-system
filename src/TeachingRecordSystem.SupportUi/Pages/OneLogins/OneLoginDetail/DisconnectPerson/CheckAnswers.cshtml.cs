using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Events.ChangeReasons;
using TeachingRecordSystem.Core.Services.OneLogin;
using TeachingRecordSystem.SupportUi.Infrastructure.Filters;

namespace TeachingRecordSystem.SupportUi.Pages.OneLogins.OneLoginDetail.DisconnectPerson;

[Journey(JourneyNames.DisconnectPerson)]
[TypeFilter(typeof(CheckOneLoginUserExistsFilterFactory))]
public class CheckAnswers(
    DisconnectPersonJourneyCoordinator journey,
    SupportUiLinkGenerator linkGenerator,
    TimeProvider timeProvider,
    OneLoginService oneLoginService,
    TrsDbContext dbContext)
    : PageModel
{
    [FromRoute] public required string OneLoginUserSubject { get; set; }

    [FromRoute] public required Guid PersonId { get; set; }


    public JourneyInstanceId InstanceId => journey.InstanceId;

    public string? BackLink { get; set; }

    [BindProperty]
    public bool Cancel { get; set; }

    public string? PersonName { get; set; }

    public DisconnectPersonReason? Reason { get; set; }

    public string? Detail { get; set; }

    public DisconnectPersonStayVerified? StayVerified { get; set; }

    public async Task OnGetAsync()
    {
        var person = await dbContext.Persons.SingleAsync(x => x.PersonId == PersonId);
        PersonName = $"{person.FirstName} {person.LastName}";
        Reason = journey.State.DisconnectReason;
        Detail = journey.State.DisconnectReason == DisconnectPersonReason.AnotherReason ? journey.State.Detail : null;
        StayVerified = journey.State.StayVerified;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Cancel)
        {
            journey.DeleteInstance();
            return Redirect(linkGenerator.OneLogins.OneLoginDetail.Index(OneLoginUserSubject));
        }

        var changeReason = new ChangeReasonWithDetailsAndEvidence()
        {
            Reason = Reason?.GetDisplayName(),
            Details = journey.State.DisconnectReason == DisconnectPersonReason.AnotherReason
                ? journey.State.Detail
                : null,
            EvidenceFile = null,
            AdditionalInformation = null
        };
        var processContext = new ProcessContext(ProcessType.OneLoginUserPersonDisconnecting, timeProvider.UtcNow, User.GetUserId(), changeReason: changeReason);
        var person = await dbContext.Persons.SingleAsync(x => x.PersonId == PersonId);
        if (journey.State.StayVerified == DisconnectPersonStayVerified.Yes)
        {
            await oneLoginService.SetUserUnmatchedAsync(OneLoginUserSubject, processContext);
        }
        else
        {
            await oneLoginService.SetUserUnverifiedAndUnmatchedAsync(OneLoginUserSubject, processContext);

        }

        var personName = $"{person.FirstName} {person.LastName}";
        TempData.SetFlashNotificationBanner($"{personName}\u2019s record disconnected from GOV.UK One Login");

        journey.DeleteInstance();
        return Redirect(linkGenerator.OneLogins.OneLoginDetail.Index(OneLoginUserSubject));
    }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        BackLink = journey.GetBackLink();
    }
}
