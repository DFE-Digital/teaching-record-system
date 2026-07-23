using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Services.OneLogin;
using TeachingRecordSystem.SupportUi.Infrastructure.Filters;

namespace TeachingRecordSystem.SupportUi.Pages.OneLogins.OneLoginDetail.DisconnectPerson;

[Journey(JourneyNames.DisconnectPerson), StartsJourney]
[TypeFilter(typeof(CheckOneLoginUserExistsFilterFactory))]
public class Index(DisconnectPersonJourneyCoordinator journey, SupportUiLinkGenerator linkGenerator, TrsDbContext dbContext) : PageModel
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



    public string? BackLink { get; set; }

    [BindProperty]
    public bool Cancel { get; set; }

    public async Task OnGetAsync()
    {
        var person = await dbContext.Persons.SingleAsync(p => p.PersonId == PersonId);
        ReasonDetail = journey.State.Detail;
        Reason = journey.State.DisconnectReason;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Cancel)
        {
            journey.DeleteInstance();
            return Redirect(linkGenerator.OneLogins.OneLoginDetail.Index(OneLoginUserSubject));
        }

        await _validator.ValidateAndThrowAsync(this);

        return journey.AdvanceTo(
            linkGenerator.OneLogins.OneLoginDetail.DisconnectPerson.Verified(journey.InstanceId),
            state =>
            {
                state.DisconnectReason = Reason;
                state.Detail = Reason == DisconnectPersonReason.AnotherReason ? ReasonDetail : null;
            });
    }

    public override void OnPageHandlerExecuting(Microsoft.AspNetCore.Mvc.Filters.PageHandlerExecutingContext context)
    {
        BackLink = journey.GetBackLink() ?? linkGenerator.OneLogins.OneLoginDetail.Index(OneLoginUserSubject);
    }

}
