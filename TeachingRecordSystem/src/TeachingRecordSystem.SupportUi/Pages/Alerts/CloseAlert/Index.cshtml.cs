using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Alerts.CloseAlert;

[Journey(JourneyNames.CloseAlert), ActivatesJourney, RequireJourneyInstance]
public class IndexModel(
    SupportUiLinkGenerator linkGenerator,
    EvidenceUploadManager evidenceUploadManager,
    IClock clock) : PageModel
{
    public JourneyInstance<CloseAlertState>? JourneyInstance { get; set; }

    [FromRoute]
    public Guid AlertId { get; set; }

    [FromQuery]
    public bool FromCheckAnswers { get; set; }

    public Guid PersonId { get; set; }

    public string? PersonName { get; set; }

    [BindProperty]
    public DateOnly? EndDate { get; set; }

    public DateOnly? StartDate { get; set; }

    public void OnGet()
    {
        EndDate = JourneyInstance!.State.EndDate;
    }

    public async Task<IActionResult> OnPostAsync()
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
            ? linkGenerator.Alerts.CloseAlert.CheckAnswers(AlertId, JourneyInstance.InstanceId)
            : linkGenerator.Alerts.CloseAlert.Reason(AlertId, JourneyInstance!.InstanceId));
    }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await evidenceUploadManager.DeleteUploadedFileAsync(JourneyInstance!.State.Evidence.UploadedEvidenceFile);
        await JourneyInstance!.DeleteAsync();
        return Redirect(linkGenerator.Persons.PersonDetail.Alerts(PersonId));
    }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        var alertInfo = context.HttpContext.GetCurrentAlertFeature();
        var personInfo = context.HttpContext.GetCurrentPersonFeature();

        PersonId = personInfo.PersonId;
        PersonName = personInfo.Name;
        StartDate = alertInfo.Alert.StartDate;
    }

}
