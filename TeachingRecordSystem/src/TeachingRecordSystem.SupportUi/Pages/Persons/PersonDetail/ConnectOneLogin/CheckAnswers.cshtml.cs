using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Services.OneLogin;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.ConnectOneLogin;

[Journey(JourneyNames.ConnectOneLogin), RequireJourneyInstance]
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

    public JourneyInstance<ConnectOneLoginState>? JourneyInstance { get; set; }

    [FromRoute]
    public Guid PersonId { get; set; }

    public string? OneLoginEmailAddress { get; set; }

    public ConnectOneLoginReason? ConnectReason { get; set; }

    public string? ReasonDetail { get; set; }

    public bool IsOneLoginUserVerified { get; set; }

    [BindProperty]
    public bool IdentityConfirmed { get; set; }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        if (!JourneyInstance!.State.ConnectReason.HasValue)
        {
            context.Result = Redirect(linkGenerator.Persons.PersonDetail.ConnectOneLogin.Reason(PersonId, JourneyInstance.InstanceId));
            return;
        }

        var oneLoginUser = await dbContext.OneLoginUsers
            .Where(u => u.Subject == JourneyInstance.State.Subject)
            .Select(u => new { u.VerifiedOn, u.VerifiedNames, u.VerifiedDatesOfBirth })
            .SingleAsync();

        IsOneLoginUserVerified = oneLoginUser.VerifiedOn is not null;
        OneLoginEmailAddress = JourneyInstance.State.OneLoginEmailAddress;
        ConnectReason = JourneyInstance.State.ConnectReason;
        ReasonDetail = JourneyInstance.State.ReasonDetail;

        await next();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await _validator.ValidateAndThrowAsync(this);

        var oneLoginUser = await dbContext.OneLoginUsers
            .Where(u => u.Subject == JourneyInstance!.State.Subject)
            .SingleAsync();

        var person = await dbContext.Persons
            .Where(p => p.PersonId == PersonId)
            .SingleAsync();

        var processContext = new ProcessContext(ProcessType.PersonOneLoginUserConnecting, timeProvider.UtcNow, User.GetUserId());

        var matchedPerson = JourneyInstance!.State.MatchedPerson!;

        if (oneLoginUser.VerifiedOn is null)
        {
            var verifiedNames = new[] { person.FirstName, person.MiddleName, person.LastName }
                .Where(n => !string.IsNullOrEmpty(n))
                .ToArray();

            await oneLoginService.SetUserVerifiedAndMatchedAsync(
                new SetUserVerifiedAndMatchedOptions
                {
                    OneLoginUserSubject = JourneyInstance.State.Subject!,
                    VerificationRoute = OneLoginUserVerificationRoute.Support,
                    VerifiedNames = [verifiedNames],
                    VerifiedDatesOfBirth = person.DateOfBirth.HasValue ? [person.DateOfBirth.Value] : [],
                    CoreIdentityClaimVc = null,
                    MatchedPersonId = matchedPerson.PersonId,
                    MatchRoute = OneLoginUserMatchRoute.SupportUi,
                    MatchedAttributes = matchedPerson.MatchedAttributes
                },
                processContext);
        }
        else
        {
            await oneLoginService.SetUserMatchedAsync(
                new SetUserMatchedOptions
                {
                    OneLoginUserSubject = JourneyInstance.State.Subject!,
                    MatchedPersonId = matchedPerson.PersonId,
                    MatchRoute = OneLoginUserMatchRoute.SupportUi,
                    MatchedAttributes = matchedPerson.MatchedAttributes
                },
                processContext);
        }

        var personName = StringHelper.JoinNonEmpty(' ', person.FirstName, person.MiddleName, person.LastName);
        TempData.SetFlashSuccess($"Record connected to {personName}’s GOV.UK One Login");

        await JourneyInstance!.CompleteAsync();
        return Redirect(linkGenerator.Persons.PersonDetail.Index(PersonId));
    }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await JourneyInstance!.DeleteAsync();
        return Redirect(linkGenerator.Persons.PersonDetail.Index(PersonId));
    }
}
