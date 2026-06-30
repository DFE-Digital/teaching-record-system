using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Events.ChangeReasons;
using TeachingRecordSystem.Core.Services.OneLogin;
using TeachingRecordSystem.SupportUi.Infrastructure.Filters;

namespace TeachingRecordSystem.SupportUi.Pages.OneLogins.OneLoginDetail.DisconnectPerson;

[Journey(JourneyNames.DisconnectPerson), RequireJourneyInstance]
[TypeFilter(typeof(CheckOneLoginUserExistsFilterFactory))]
public class CheckAnswers(
    SupportUiLinkGenerator linkGenerator,
    TimeProvider timeProvider,
    OneLoginService oneLoginService,
    TrsDbContext dbContext)
    : PageModel
{
    [FromRoute] public required string OneLoginUserSubject { get; set; }

    [FromRoute] public required Guid PersonId { get; set; }

    public JourneyInstance<DisconnectPersonState>? JourneyInstance { get; set; }

    public string? PersonName { get; set; }

    public DisconnectPersonReason? Reason { get; set; }

    public string? Detail { get; set; }

    public DisconnectPersonStayVerified? StayVerified { get; set; }

    public async Task OnGetAsync()
    {
        var person = await dbContext.Persons.SingleAsync(x => x.PersonId == PersonId);
        PersonName = $"{person.FirstName} {person.LastName}";
        Reason = JourneyInstance!.State.DisconnectReason;
        Detail = JourneyInstance!.State.DisconnectReason == DisconnectPersonReason.AnotherReason ? JourneyInstance.State.Detail : null;
        StayVerified = JourneyInstance!.State.StayVerified;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var changeReason = new ChangeReasonWithDetailsAndEvidence()
        {
            Reason = Reason?.GetDisplayName(),
            Details = JourneyInstance!.State.DisconnectReason == DisconnectPersonReason.AnotherReason
                ? JourneyInstance.State.Detail
                : null,
            EvidenceFile = null,
            AdditionalInformation = null
        };
        var processContext = new ProcessContext(ProcessType.OneLoginUserPersonDisconnecting, timeProvider.UtcNow, User.GetUserId(), changeReason: changeReason);
        var person = await dbContext.Persons.SingleAsync(x => x.PersonId == PersonId);
        if (JourneyInstance!.State.StayVerified == DisconnectPersonStayVerified.Yes)
        {
            await oneLoginService.SetUserUnmatchedAsync(OneLoginUserSubject, processContext);
        }
        else
        {
            await oneLoginService.SetUserUnverifiedAndUnmatchedAsync(OneLoginUserSubject, processContext);

        }

        var personName = $"{person.FirstName} {person.LastName}";
        TempData.SetFlashNotificationBanner($"{personName}\u2019s record disconnected from GOV.UK One Login");
        return Redirect(linkGenerator.OneLogins.OneLoginDetail.Index(OneLoginUserSubject));
    }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await JourneyInstance!.DeleteAsync();
        return Redirect(linkGenerator.OneLogins.OneLoginDetail.Index(OneLoginUserSubject));
    }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        if (!JourneyInstance!.State.DisconnectReason.HasValue || !JourneyInstance.State.StayVerified.HasValue)
        {
            context.Result = Redirect(linkGenerator.OneLogins.OneLoginDetail.DisconnectPerson.Index(OneLoginUserSubject, PersonId, JourneyInstance.InstanceId));
            return;
        }
    }
}
