using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Services.OneLogin;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.ConnectOneLogin;

[Journey(JourneyNames.ConnectOneLogin)]
[ActivatesJourney, RequireJourneyInstance]
public class IndexModel(
    TrsDbContext dbContext,
    OneLoginService oneLoginService,
    SupportUiLinkGenerator linkGenerator) : PageModel
{
    private readonly InlineValidator<IndexModel> _validator = new()
    {
        v => v.RuleFor(m => m.EmailAddress)
            .NotEmpty()
            .WithMessage("Enter a GOV.UK One Login email address")
            .EmailAddress()
    };

    public JourneyInstance<ConnectOneLoginState>? JourneyInstance { get; set; }

    [FromRoute]
    public Guid PersonId { get; set; }

    [BindProperty]
    public string? EmailAddress { get; set; }

    public string? Trn { get; set; }

    public void OnGet()
    {
        EmailAddress = JourneyInstance?.State.OneLoginEmailAddress;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await _validator.ValidateAndThrowAsync(this);

        var oneLoginUser = await dbContext.OneLoginUsers
            .Where(u => u.EmailAddress == EmailAddress)
            .FirstOrDefaultAsync();

        if (oneLoginUser is null)
        {
            ModelState.AddModelError(nameof(EmailAddress), "The email address you entered is not linked to a GOV.UK One Login record");
            return this.PageWithErrors();
        }

        if (oneLoginUser.PersonId is not null)
        {
            var errorMessage = oneLoginUser.PersonId == PersonId
                ? "The email address you entered is already connected to this record"
                : "The email address you entered is already connected to another record";

            ModelState.AddModelError(nameof(EmailAddress), errorMessage);
            return this.PageWithErrors();
        }

        var suggestedMatches = await oneLoginService.GetSuggestedPersonMatchesAsync(new GetSuggestedPersonMatchesOptions(
            Names: oneLoginUser.VerifiedNames ?? [],
            DatesOfBirth: oneLoginUser.VerifiedDatesOfBirth ?? [],
            EmailAddress: oneLoginUser.EmailAddress,
            NationalInsuranceNumber: null,
            Trn: null,
            TrnTokenTrnHint: null,
            PersonId: PersonId));

        await JourneyInstance!.UpdateStateAsync(state =>
        {
            state.Subject = oneLoginUser.Subject;
            state.OneLoginEmailAddress = oneLoginUser.EmailAddress;
            state.MatchedPerson = suggestedMatches.FirstOrDefault();
        });

        return Redirect(linkGenerator.Persons.PersonDetail.ConnectOneLogin.Match(PersonId, JourneyInstance.InstanceId));
    }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await JourneyInstance!.DeleteAsync();
        return Redirect(linkGenerator.Persons.PersonDetail.Index(PersonId));
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        Trn = context.HttpContext.GetCurrentPersonFeature().Trn;

        await next();
    }
}

