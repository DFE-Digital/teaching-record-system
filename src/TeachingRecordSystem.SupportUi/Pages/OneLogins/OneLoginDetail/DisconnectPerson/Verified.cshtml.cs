using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.Services.OneLogin;
using TeachingRecordSystem.SupportUi.Infrastructure.Filters;

namespace TeachingRecordSystem.SupportUi.Pages.OneLogins.OneLoginDetail.DisconnectPerson;

[Journey(JourneyNames.DisconnectPerson)]
[TypeFilter(typeof(CheckOneLoginUserExistsFilterFactory))]
public class Verified(
    DisconnectPersonJourneyCoordinator journey,
    SupportUiLinkGenerator linkGenerator) : PageModel
{
    private readonly InlineValidator<Verified> _validator = new()
    {
        v => v.RuleFor(m => m.StayVerified)
            .NotNull()
            .WithMessage("Select yes if you want to keep the GOV.UK One Login verified")
    };

    [FromRoute] public required string OneLoginUserSubject { get; set; }

    [FromRoute] public required Guid PersonId { get; set; }

    [BindProperty] public DisconnectPersonStayVerified? StayVerified { get; set; }

    public string? BackLink { get; set; }

    [BindProperty]
    public bool Cancel { get; set; }

    public void OnGet()
    {
        StayVerified = journey.State.StayVerified;
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
            linkGenerator.OneLogins.OneLoginDetail.DisconnectPerson.CheckAnswers(journey.InstanceId),
            state => state.StayVerified = StayVerified);
    }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        BackLink = journey.GetBackLink();
    }
}
