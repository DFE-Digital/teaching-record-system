using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Events.ChangeReasons;
using TeachingRecordSystem.Core.Services.OneLogin;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.ConnectOneLogin;

[Journey(JourneyNames.ConnectOneLogin)]
public class CheckAnswersModel(
    ConnectOneLoginJourneyCoordinator journey,
    TrsDbContext dbContext,
    OneLoginService oneLoginService,
    TimeProvider timeProvider,
    SupportUiLinkGenerator linkGenerator) : PageModel
{
    private readonly InlineValidator<CheckAnswersModel> _validator = new()
    {
        v => v.RuleFor(m => m.IdentityConfirmed)
            .Equal(true)
            .WithMessage("Confirm you’ve completed the required identity checks")
            .When(m => !m.IsOneLoginUserVerified)
    };

    [FromRoute]
    public Guid PersonId { get; set; }

    [BindProperty]
    public bool Cancel { get; set; }

    public JourneyInstanceId InstanceId => journey.InstanceId;

    public string? BackLink { get; set; }

    public string? OneLoginEmailAddress { get; set; }

    public ConnectOneLoginReason? ConnectReason { get; set; }

    public string? ReasonDetail { get; set; }

    public bool IsOneLoginUserVerified { get; set; }

    [BindProperty]
    public bool IdentityConfirmed { get; set; }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        BackLink = journey.GetBackLink();

        var oneLoginUser = await dbContext.OneLoginUsers
            .Where(u => u.Subject == journey.State.Subject)
            .Select(u => new { u.VerifiedOn, u.VerifiedNames, u.VerifiedDatesOfBirth })
            .SingleAsync();

        IsOneLoginUserVerified = oneLoginUser.VerifiedOn is not null;
        OneLoginEmailAddress = journey.State.OneLoginEmailAddress;
        ConnectReason = journey.State.ConnectReason;
        ReasonDetail = journey.State.ReasonDetail;

        await next();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Cancel)
        {
            return CancelJourney();
        }

        await _validator.ValidateAndThrowAsync(this);

        var oneLoginUser = await dbContext.OneLoginUsers
            .Where(u => u.Subject == journey.State.Subject)
            .SingleAsync();

        var person = await dbContext.Persons
            .Where(p => p.PersonId == PersonId)
            .SingleAsync();

        var changeReason = new ChangeReasonWithDetailsAndEvidence()
        {
            Reason = journey.State.ConnectReason?.GetDisplayName(),
            Details = journey.State.ConnectReason == ConnectOneLoginReason.AnotherReason
                ? journey.State.ReasonDetail
                : null,
            EvidenceFile = null,
            AdditionalInformation = null
        };

        var processContext = new ProcessContext(ProcessType.PersonOneLoginUserConnecting, timeProvider.UtcNow, User.GetUserId(), changeReason: changeReason);

        var matchedAttributes = journey.State.MatchedAttributes!;

        if (oneLoginUser.VerifiedOn is null)
        {
            var verifiedNames = new[] { person.FirstName, person.MiddleName, person.LastName }
                .Where(n => !string.IsNullOrEmpty(n))
                .ToArray();

            await oneLoginService.SetUserVerifiedAndMatchedAsync(
                new SetUserVerifiedAndMatchedOptions
                {
                    OneLoginUserSubject = journey.State.Subject!,
                    VerificationRoute = OneLoginUserVerificationRoute.Support,
                    VerifiedNames = [verifiedNames],
                    VerifiedDatesOfBirth = person.DateOfBirth.HasValue ? [person.DateOfBirth.Value] : [],
                    CoreIdentityClaimVc = null,
                    MatchedPersonId = PersonId,
                    MatchRoute = OneLoginUserMatchRoute.SupportUi,
                    MatchedAttributes = matchedAttributes
                },
                processContext);
        }
        else
        {
            await oneLoginService.SetUserMatchedAsync(
                new SetUserMatchedOptions
                {
                    OneLoginUserSubject = journey.State.Subject!,
                    MatchedPersonId = PersonId,
                    MatchRoute = OneLoginUserMatchRoute.SupportUi,
                    MatchedAttributes = matchedAttributes
                },
                processContext);
        }

        var personName = string.JoinNonEmpty(' ', person.FirstName, person.MiddleName, person.LastName);
        TempData.SetFlashNotificationBanner($"Record connected to {personName}’s GOV.UK One Login");

        journey.DeleteInstance();

        return Redirect(linkGenerator.Persons.PersonDetail.Index(PersonId));
    }

    private IActionResult CancelJourney()
    {
        journey.DeleteInstance();
        return Redirect(linkGenerator.Persons.PersonDetail.Index(PersonId));
    }
}
