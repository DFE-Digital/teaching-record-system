using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Events.ChangeReasons;
using TeachingRecordSystem.Core.Services.OneLogin;
using TeachingRecordSystem.SupportUi.Infrastructure.Filters;

namespace TeachingRecordSystem.SupportUi.Pages.OneLogins.OneLoginDetail.ConnectPerson;

[Journey(JourneyNames.ConnectPerson), RequireJourneyInstance]
[TypeFilter(typeof(CheckOneLoginUserExistsFilterFactory))]
public class CheckAnswersModel(
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

    public JourneyInstance<ConnectPersonState>? JourneyInstance { get; set; }

    [FromRoute]
    public string OneLoginUserSubject { get; set; } = null!;

    public string? OneLoginEmailAddress { get; set; }

    public string? PersonTrn { get; set; }

    public ConnectPersonReason? ConnectReason { get; set; }

    public string? ReasonDetail { get; set; }

    public bool IsOneLoginUserVerified { get; set; }

    [BindProperty]
    public bool IdentityConfirmed { get; set; }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        if (!JourneyInstance!.State.ConnectReason.HasValue)
        {
            context.Result = Redirect(linkGenerator.OneLogins.OneLoginDetail.ConnectPerson.Reason(OneLoginUserSubject, JourneyInstance.InstanceId));
            return;
        }

        var oneLoginUserFeature = context.HttpContext.GetCurrentOneLoginUserFeature();

        IsOneLoginUserVerified = oneLoginUserFeature.VerifiedOn is not null;
        OneLoginEmailAddress = oneLoginUserFeature.EmailAddress;
        PersonTrn = JourneyInstance.State.PersonTrn;
        ConnectReason = JourneyInstance.State.ConnectReason;
        ReasonDetail = JourneyInstance.State.ReasonDetail;

        await next();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await _validator.ValidateAndThrowAsync(this);

        var oneLoginUserFeature = HttpContext.GetCurrentOneLoginUserFeature();

        var person = await dbContext.Persons
            .Where(p => p.PersonId == JourneyInstance!.State.PersonId)
            .SingleAsync();

        var changeReason = new ChangeReasonWithDetailsAndEvidence()
        {
            Reason = JourneyInstance!.State.ConnectReason?.GetDisplayName(),
            Details = JourneyInstance.State.ConnectReason == ConnectPersonReason.AnotherReason
                ? JourneyInstance.State.ReasonDetail
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

        var personName = StringHelper.JoinNonEmpty(' ', person.FirstName, person.MiddleName, person.LastName);
        TempData.SetFlashNotificationBanner($"Record connected to {personName}’s GOV.UK One Login");

        await JourneyInstance!.CompleteAsync();
        return Redirect(linkGenerator.Persons.PersonDetail.Index(person.PersonId));
    }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await JourneyInstance!.DeleteAsync();
        return Redirect(linkGenerator.OneLogins.OneLoginDetail.Index(OneLoginUserSubject));
    }
}
