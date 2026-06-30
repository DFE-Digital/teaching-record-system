using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Services.OneLogin;
using TeachingRecordSystem.SupportUi.Infrastructure.Filters;

namespace TeachingRecordSystem.SupportUi.Pages.OneLogins.OneLoginDetail.DisconnectPerson;

[Journey(JourneyNames.DisconnectPerson), ActivatesJourney, RequireJourneyInstance]
[TypeFilter(typeof(CheckOneLoginUserExistsFilterFactory))]
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
            .When(m => m.Reason == DisconnectPersonReason.AnotherReason)
    };

    [BindProperty]
    public DisconnectPersonReason? Reason { get; set; }

    [BindProperty]
    public string? ReasonDetail { get; set; }

    [FromRoute] public required string OneLoginUserSubject { get; set; }

    [FromRoute] public required Guid PersonId { get; set; }

    public JourneyInstance<DisconnectPersonState>? JourneyInstance { get; set; }

    [FromQuery] public bool? FromCheckAnswers { get; set; }

    public string BackLink => FromCheckAnswers == true
        ? linkGenerator.OneLogins.OneLoginDetail.DisconnectPerson.CheckAnswers(OneLoginUserSubject, PersonId,
            JourneyInstance!.InstanceId)
        : linkGenerator.OneLogins.OneLoginDetail.Index(OneLoginUserSubject);

    public async Task OnGetAsync()
    {
        var person = await dbContext.Persons.SingleAsync(p => p.PersonId == PersonId);
        ReasonDetail = JourneyInstance!.State.Detail;
        Reason = JourneyInstance.State.DisconnectReason;
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
            state.Detail = Reason == DisconnectPersonReason.AnotherReason ? ReasonDetail : null;
        });

        if (FromCheckAnswers == true)
        {
            return Redirect(linkGenerator.OneLogins.OneLoginDetail.DisconnectPerson.CheckAnswers(OneLoginUserSubject, PersonId,
                JourneyInstance.InstanceId));
        }

        return Redirect(linkGenerator.OneLogins.OneLoginDetail.DisconnectPerson.Verified(OneLoginUserSubject, PersonId,
            JourneyInstance.InstanceId));
    }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await JourneyInstance!.DeleteAsync();
        return Redirect(linkGenerator.OneLogins.OneLoginDetail.Index(OneLoginUserSubject));
    }
}
