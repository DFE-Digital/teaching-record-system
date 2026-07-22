using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Services.Persons;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.DisconnectOneLogin;

[Journey(JourneyNames.DisconnectOneLogin), StartsJourney]
public class Index(DisconnectOneLoginJourneyCoordinator journey, SupportUiLinkGenerator linkGenerator, TrsDbContext dbContext) : PageModel
{
    private readonly InlineValidator<Index> _validator = new()
    {
        v => v.RuleFor(m => m.ReasonDetail)
            .MaximumLength(UiDefaults.ReasonDetailsMaxCharacterCount)
            .WithMessage($"Reason details {UiDefaults.ReasonDetailsMaxCharacterCountErrorMessage}"),

        v => v.RuleFor(m => m.Reason)
            .NotNull()
            .WithMessage($"Select a reason"),

        v => v.RuleFor(m => m.ReasonDetail)
            .NotEmpty()
            .WithMessage("Enter a reason")
            .When(m => m.Reason == DisconnectOneLoginReason.AnotherReason)
    };

    [BindProperty]
    public DisconnectOneLoginReason? Reason { get; set; }

    [BindProperty]
    public string? ReasonDetail { get; set; }

    [FromRoute] public required Guid PersonId { get; set; }

    [FromRoute] public required string OneLoginSubject { get; set; }



    public string? EmailAddress { get; set; }

    public string? BackLink { get; set; }

    [BindProperty]
    public bool Cancel { get; set; }

    public async Task OnGetAsync()
    {
        var oneLogin = await dbContext.OneLoginUsers.SingleAsync(u => u.Subject == OneLoginSubject);
        ReasonDetail = journey.State.Detail;
        Reason = journey.State.DisconnectReason;
        EmailAddress = oneLogin.EmailAddress;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Cancel)
        {
            journey.DeleteInstance();
            return Redirect(linkGenerator.Persons.PersonDetail.Index(PersonId));
        }

        await _validator.ValidateAndThrowAsync(this);

        return journey.AdvanceTo(
            linkGenerator.Persons.PersonDetail.DisconnectOneLogin.Verified(journey.InstanceId),
            state =>
            {
                state.DisconnectReason = Reason;
                state.Detail = Reason == DisconnectOneLoginReason.AnotherReason ? ReasonDetail : null;
            });
    }

    public override void OnPageHandlerExecuting(Microsoft.AspNetCore.Mvc.Filters.PageHandlerExecutingContext context)
    {
        BackLink = journey.GetBackLink() ?? linkGenerator.Persons.PersonDetail.Index(PersonId);
    }

}
