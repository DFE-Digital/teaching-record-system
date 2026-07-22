using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Events.ChangeReasons;
using TeachingRecordSystem.Core.Services.OneLogin;
using TeachingRecordSystem.SupportUi.Infrastructure.Filters;

namespace TeachingRecordSystem.SupportUi.Pages.OneLogins.OneLoginDetail.ConnectPerson;

[Journey(JourneyNames.ConnectPerson)]
[TypeFilter(typeof(CheckOneLoginUserExistsFilterFactory))]
public class CheckAnswersModel(
    ConnectPersonJourneyCoordinator journey,
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
    public string OneLoginUserSubject { get; set; } = null!;

    [BindProperty]
    public bool Cancel { get; set; }

    public JourneyInstanceId InstanceId => journey.InstanceId;

    public string? BackLink { get; set; }

    public string? OneLoginEmailAddress { get; set; }

    public string? PersonTrn { get; set; }

    public ConnectPersonReason? ConnectReason { get; set; }

    public string? ReasonDetail { get; set; }

    public bool IsOneLoginUserVerified { get; set; }

    [BindProperty]
    public bool IdentityConfirmed { get; set; }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        BackLink = journey.GetBackLink();

        var oneLoginUserFeature = context.HttpContext.GetCurrentOneLoginUserFeature();

        IsOneLoginUserVerified = oneLoginUserFeature.VerifiedOn is not null;
        OneLoginEmailAddress = oneLoginUserFeature.EmailAddress;
        PersonTrn = journey.State.PersonTrn;
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

        var oneLoginUserFeature = HttpContext.GetCurrentOneLoginUserFeature();

        var person = await dbContext.Persons
            .Where(p => p.PersonId == journey.State.PersonId)
            .SingleAsync();

        var changeReason = new ChangeReasonWithDetailsAndEvidence()
        {
            Reason = journey.State.ConnectReason?.GetDisplayName(),
            Details = journey.State.ConnectReason == ConnectPersonReason.AnotherReason
                ? journey.State.ReasonDetail
                : null,
            EvidenceFile = null,
            AdditionalInformation = null
        };

        var processContext = new ProcessContext(ProcessType.OneLoginUserPersonConnecting, timeProvider.UtcNow, User.GetUserId(), changeReason: changeReason);

        if (oneLoginUserFeature.VerifiedOn is null)
        {
            var verifiedNames = new[] { person.FirstName, person.MiddleName, person.LastName }
                .Where(n => !string.IsNullOrEmpty(n))
                .ToArray();

            await oneLoginService.SetUserVerifiedAndMatchedAsync(
                new SetUserVerifiedAndMatchedOptions
                {
                    OneLoginUserSubject = OneLoginUserSubject,
                    VerificationRoute = OneLoginUserVerificationRoute.Support,
                    VerifiedNames = [verifiedNames],
                    VerifiedDatesOfBirth = person.DateOfBirth.HasValue ? [person.DateOfBirth.Value] : [],
                    CoreIdentityClaimVc = null,
                    MatchedPersonId = person.PersonId,
                    MatchRoute = OneLoginUserMatchRoute.SupportUi,
                    MatchedAttributes = []
                },
                processContext);
        }
        else
        {
            await oneLoginService.SetUserMatchedAsync(
                new SetUserMatchedOptions
                {
                    OneLoginUserSubject = OneLoginUserSubject,
                    MatchedPersonId = person.PersonId,
                    MatchRoute = OneLoginUserMatchRoute.SupportUi,
                    MatchedAttributes = []
                },
                processContext);
        }

        var personName = string.JoinNonEmpty(' ', person.FirstName, person.MiddleName, person.LastName);
        TempData.SetFlashNotificationBanner($"Record connected to {personName}’s GOV.UK One Login");

        journey.DeleteInstance();

        return Redirect(linkGenerator.Persons.PersonDetail.Index(person.PersonId));
    }

    private IActionResult CancelJourney()
    {
        journey.DeleteInstance();
        return Redirect(linkGenerator.OneLogins.OneLoginDetail.Index(OneLoginUserSubject));
    }
}
