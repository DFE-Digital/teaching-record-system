using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.UiCommon.DataAnnotations;

namespace TeachingRecordSystem.SupportUi.Pages.Alerts.AddAlert;

[Journey(JourneyNames.AddAlert), RequireJourneyInstance]
public class EndDateModel(TrsLinkGenerator linkGenerator) : PageModel
{
    public JourneyInstance<AddAlertState>? JourneyInstance { get; set; }

    [FromQuery]
    public Guid PersonId { get; set; }

    [FromQuery]
    public bool FromCheckAnswers { get; set; }

    public string? PersonName { get; set; }

    [BindProperty]
    [Display(Name = "Is there an end date for this alert?")]
    [Required(ErrorMessage = "Select yes if there is an end date for this alert")]
    public bool? HasEndDate { get; set; }

    [BindProperty]
    [Display(Name = "End date")]
    [RequiredIfOtherPropertyEquals(nameof(HasEndDate), ErrorMessage = "Enter an end date")]
    public DateOnly? EndDate { get; set; }

    public void OnGet()
    {
        EndDate = JourneyInstance!.State.EndDate;
    }

    public async Task<IActionResult> OnPost()
    {
        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        await JourneyInstance!.UpdateStateAsync(state =>
        {
            state.HasEndDate = HasEndDate;
            state.EndDate = HasEndDate!.Value ? EndDate : null;
        });

        return Redirect(FromCheckAnswers
            ? linkGenerator.AlertAddCheckAnswers(PersonId, JourneyInstance.InstanceId)
            : linkGenerator.AlertAddReason(PersonId, JourneyInstance.InstanceId));
    }

    public async Task<IActionResult> OnPostCancel()
    {
        await JourneyInstance!.DeleteAsync();
        return Redirect(linkGenerator.PersonAlerts(PersonId));
    }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        if (JourneyInstance!.State.StartDate is null)
        {
            context.Result = Redirect(linkGenerator.AlertAddStartDate(PersonId, JourneyInstance.InstanceId));
            return;
        }

        var personInfo = context.HttpContext.GetCurrentPersonFeature();

        PersonName = personInfo.Name;
    }
}
