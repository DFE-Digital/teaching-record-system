using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Services.Persons;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.DisconnectOneLogin;

[Journey(JourneyNames.DisconnectOneLogin), ActivatesJourney, RequireJourneyInstance]
public class Index(SupportUiLinkGenerator linkGenerator, TrsDbContext dbContext) : PageModel
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

    public JourneyInstance<DisconnectOneLoginState>? JourneyInstance { get; set; }

    [FromQuery] public bool? FromCheckAnswers { get; set; }

    public string? EmailAddress { get; set; }

    public string BackLink => FromCheckAnswers == true
        ? linkGenerator.Persons.PersonDetail.DisconnectOneLogin.CheckAnswers(PersonId, OneLoginSubject!,
            JourneyInstance!.InstanceId)
        : linkGenerator.Persons.PersonDetail.Index(PersonId);

    public async Task OnGetAsync()
    {
        var oneLogin = await dbContext.OneLoginUsers.SingleAsync(u => u.Subject == OneLoginSubject);
        var person = await dbContext.Persons.SingleAsync(x => x.PersonId == PersonId);
        await JourneyInstance!.UpdateStateAsync(state =>
        {
            state.EmailAddress = oneLogin.EmailAddress;
            state.OneLoginSubject = OneLoginSubject;
            state.PersonName = $"{person.FirstName} {person.LastName}";
        });

        ReasonDetail = JourneyInstance!.State.Detail;
        Reason = JourneyInstance.State.DisconnectReason;
        EmailAddress = oneLogin.EmailAddress;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await _validator.ValidateAndThrowAsync(this);

        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        await JourneyInstance!.UpdateStateAsync(state =>
        {
            state.DisconnectReason = Reason;
            state.Detail = Reason == DisconnectOneLoginReason.AnotherReason ? ReasonDetail : null;
        });

        if (FromCheckAnswers == true)
        {
            return Redirect(linkGenerator.Persons.PersonDetail.DisconnectOneLogin.CheckAnswers(PersonId, OneLoginSubject,
                JourneyInstance.InstanceId));
        }

        return Redirect(linkGenerator.Persons.PersonDetail.DisconnectOneLogin.Verified(PersonId, OneLoginSubject,
            JourneyInstance.InstanceId));
    }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await JourneyInstance!.DeleteAsync();
        return Redirect(linkGenerator.Persons.PersonDetail.Index(PersonId));
    }
}
