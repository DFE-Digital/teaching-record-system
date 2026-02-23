using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.Services.Persons;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.DisconnectOneLogin;

[Journey(JourneyNames.DisconnectOneLogin), RequireJourneyInstance]
public class Verified(SupportUiLinkGenerator linkGenerator) : PageModel
{
    private readonly InlineValidator<Verified> _validator = new()
    {
        v => v.RuleFor(m => m.StayVerified)
            .NotNull()
            .WithMessage("Select yes if you want to keep the person verified")
    };

    [FromRoute] public required Guid PersonId { get; set; }

    [FromRoute] public required string OneLoginSubject { get; set; }

    [BindProperty] public DisconnectOneLoginStayVerified? StayVerified { get; set; }

    public JourneyInstance<DisconnectOneLoginState>? JourneyInstance { get; set; }

    [FromQuery] public bool? FromCheckAnswers { get; set; }

    public string BackLink => FromCheckAnswers == true
        ? linkGenerator.Persons.PersonDetail.DisconnectOneLogin.CheckAnswers(PersonId, OneLoginSubject!,
            JourneyInstance!.InstanceId)
        : linkGenerator.Persons.PersonDetail.DisconnectOneLogin.DisconnectOneLoginSubject(PersonId,
            OneLoginSubject!, JourneyInstance!.InstanceId);

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
            linkGenerator.Persons.PersonDetail.DisconnectOneLogin.CheckAnswers(
                PersonId,
                OneLoginSubject,
                JourneyInstance.InstanceId));
    }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await JourneyInstance!.DeleteAsync();
        return Redirect(linkGenerator.Persons.PersonDetail.Index(PersonId));
    }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        if (!JourneyInstance!.State.DisconnectReason.HasValue)
        {
            context.Result = Redirect(
                linkGenerator.Persons.PersonDetail.DisconnectOneLogin.DisconnectOneLoginSubject(
                    PersonId,
                    OneLoginSubject,
                    JourneyInstance.InstanceId));
        }
    }
}
