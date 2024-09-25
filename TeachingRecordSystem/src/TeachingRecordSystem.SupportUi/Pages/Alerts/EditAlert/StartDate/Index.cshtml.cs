using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TeachingRecordSystem.SupportUi.Pages.Alerts.EditAlert.StartDate;

[Journey(JourneyNames.EditAlertStartDate), ActivatesJourney, RequireJourneyInstance]
public class IndexModel(TrsLinkGenerator linkGenerator) : PageModel
{
    public JourneyInstance<EditAlertStartDateState>? JourneyInstance { get; set; }

    [FromRoute]
    public Guid AlertId { get; set; }

    [FromQuery]
    public bool FromCheckAnswers { get; set; }

    public Guid PersonId { get; set; }

    public string? PersonName { get; set; }

    [BindProperty]
    [Display(Name = "Start date")]
    public DateOnly? StartDate { get; set; }

    public DateOnly? CurrentStartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public void OnGet()
    {
        StartDate = JourneyInstance!.State.StartDate;
    }

    public async Task<IActionResult> OnPost()
    {
        if (StartDate is null)
        {
            ModelState.AddModelError(nameof(StartDate), "Enter a start date");
        }
        else if (StartDate >= EndDate)
        {
            ModelState.AddModelError(nameof(StartDate), "Start date must be before end date");
        }
        else if (StartDate == CurrentStartDate)
        {
            ModelState.AddModelError(nameof(StartDate), "Enter a different start date");
        }

        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        await JourneyInstance!.UpdateStateAsync(state => state.StartDate = StartDate);

        return Redirect(FromCheckAnswers
            ? linkGenerator.AlertEditStartDateCheckAnswers(AlertId, JourneyInstance.InstanceId)
            : linkGenerator.AlertEditStartDateReason(AlertId, JourneyInstance!.InstanceId));
    }

    public async Task<IActionResult> OnPostCancel()
    {
        await JourneyInstance!.DeleteAsync();
        return Redirect(linkGenerator.PersonAlerts(PersonId));
    }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        var alertInfo = context.HttpContext.GetCurrentAlertFeature();
        var personInfo = context.HttpContext.GetCurrentPersonFeature();

        JourneyInstance!.State.EnsureInitialized(alertInfo);

        PersonId = personInfo.PersonId;
        PersonName = personInfo.Name;
        CurrentStartDate = alertInfo.Alert.StartDate;
        EndDate = alertInfo.Alert.EndDate;
    }
}
