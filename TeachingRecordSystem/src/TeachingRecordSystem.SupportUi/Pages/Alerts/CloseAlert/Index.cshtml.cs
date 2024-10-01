using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TeachingRecordSystem.SupportUi.Pages.Alerts.CloseAlert;

[Journey(JourneyNames.CloseAlert), ActivatesJourney, RequireJourneyInstance]
public class IndexModel(TrsLinkGenerator linkGenerator, IClock clock) : PageModel
{
    public JourneyInstance<CloseAlertState>? JourneyInstance { get; set; }

    [FromRoute]
    public Guid AlertId { get; set; }

    [FromQuery]
    public bool FromCheckAnswers { get; set; }

    public Guid PersonId { get; set; }

    public string? PersonName { get; set; }

    [BindProperty]
    [Display(Name = "Add an end date")]
    public DateOnly? EndDate { get; set; }

    public DateOnly? StartDate { get; set; }

    public void OnGet()
    {
        EndDate = JourneyInstance!.State.EndDate;
    }

    public async Task<IActionResult> OnPost()
    {
        if (EndDate is null)
        {
            ModelState.AddModelError(nameof(EndDate), "Enter an end date");
        }
        else if (EndDate > clock.Today)
        {
            ModelState.AddModelError(nameof(EndDate), "End date cannot be in the future");
        }
        else if (EndDate <= StartDate)
        {
            ModelState.AddModelError(nameof(EndDate), "End date must be after the start date");
        }

        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        await JourneyInstance!.UpdateStateAsync(state => state.EndDate = EndDate);

        return Redirect(FromCheckAnswers
            ? linkGenerator.AlertCloseCheckAnswers(AlertId, JourneyInstance.InstanceId)
            : linkGenerator.AlertCloseReason(AlertId, JourneyInstance!.InstanceId));
    }

    public async Task<IActionResult> OnPostCancel()
    {
        await JourneyInstance!.DeleteAsync();
        return Redirect(linkGenerator.PersonAlerts(PersonId));
    }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        var alertInfo = context.HttpContext.GetCurrentAlertFeature();
        if (alertInfo.Alert.EndDate is not null)
        {
            context.Result = BadRequest();
            return;
        }

        var personInfo = context.HttpContext.GetCurrentPersonFeature();

        PersonId = personInfo.PersonId;
        PersonName = personInfo.Name;
        StartDate = alertInfo.Alert.StartDate;
    }

}
