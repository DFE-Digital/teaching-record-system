using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.Services.OneLogin;
using TeachingRecordSystem.SupportUi.Infrastructure.Filters;

namespace TeachingRecordSystem.SupportUi.Pages.OneLogins.OneLoginDetail.DisconnectPerson;

[Journey(JourneyNames.DisconnectPerson), RequireJourneyInstance]
[TypeFilter(typeof(CheckOneLoginUserExistsFilterFactory))]
public class Verified(SupportUiLinkGenerator linkGenerator) : PageModel
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

    public JourneyInstance<DisconnectPersonState>? JourneyInstance { get; set; }

    [FromQuery] public bool? FromCheckAnswers { get; set; }

    public string BackLink => FromCheckAnswers == true
        ? linkGenerator.OneLogins.OneLoginDetail.DisconnectPerson.CheckAnswers(OneLoginUserSubject, PersonId,
            JourneyInstance!.InstanceId)
        : linkGenerator.OneLogins.OneLoginDetail.DisconnectPerson.Index(OneLoginUserSubject,
            PersonId, JourneyInstance!.InstanceId);

    public void OnGet()
    {
        StayVerified = JourneyInstance?.State.StayVerified;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await _validator.ValidateAndThrowAsync(this);

        await JourneyInstance!.UpdateStateAsync(state =>
        {
            state.StayVerified = StayVerified;
        });

        return Redirect(
            linkGenerator.OneLogins.OneLoginDetail.DisconnectPerson.CheckAnswers(
                OneLoginUserSubject,
                PersonId,
                JourneyInstance.InstanceId));
    }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await JourneyInstance!.DeleteAsync();
        return Redirect(linkGenerator.OneLogins.OneLoginDetail.Index(OneLoginUserSubject));
    }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        if (!JourneyInstance!.State.DisconnectReason.HasValue)
        {
            context.Result = Redirect(
                linkGenerator.OneLogins.OneLoginDetail.DisconnectPerson.Index(
                    OneLoginUserSubject,
                    PersonId,
                    JourneyInstance.InstanceId));
        }
    }
}
