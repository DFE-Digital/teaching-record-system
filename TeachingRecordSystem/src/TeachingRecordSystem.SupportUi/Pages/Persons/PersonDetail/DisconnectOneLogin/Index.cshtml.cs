using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Services.Persons;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.DisconnectOneLogin;

[Journey(JourneyNames.DisconnectOneLogin), ActivatesJourney, RequireJourneyInstance]
public class Index(SupportUiLinkGenerator linkGenerator, TrsDbContext dbContext) : PageModel
{
    [Required(ErrorMessage = "Select a reason")]
    [BindProperty]
    public DisconnectOneLoginReason? Reason { get; set; }

    [MaxLength(UiDefaults.ReasonDetailsMaxCharacterCount,
        ErrorMessage = $"Reason details {UiDefaults.ReasonDetailsMaxCharacterCountErrorMessage}")]
    [BindProperty]
    public string? ReasonDetail { get; set; }

    [FromRoute] public required Guid PersonId { get; set; }

    [FromRoute] public required string OneLoginSubject { get; set; }

    public JourneyInstance<DisconnectOneLoginState>? JourneyInstance { get; set; }

    [FromQuery] public bool? FromCheckAnswers { get; set; }

    public void OnGet()
    {
        ReasonDetail = JourneyInstance!.State.Detail;
        Reason = JourneyInstance.State.DisconnectReason;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Reason is not null && Reason == DisconnectOneLoginReason.AnotherReason && ReasonDetail is null)
        {
            ModelState.AddModelError(nameof(ReasonDetail), "Enter a reason");
        }

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

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        var oneLogin = await dbContext.OneLoginUsers.SingleAsync(u => u.Subject == OneLoginSubject);
        var person = await dbContext.Persons.SingleAsync(x => x.PersonId == PersonId);
        await JourneyInstance!.UpdateStateAsync(state =>
        {
            state.EmailAddress = oneLogin.EmailAddress;
            state.OneLoginSubject = OneLoginSubject;
            state.PersonName = $"{person.FirstName} {person.LastName}";
        });
        await next();
    }
}
