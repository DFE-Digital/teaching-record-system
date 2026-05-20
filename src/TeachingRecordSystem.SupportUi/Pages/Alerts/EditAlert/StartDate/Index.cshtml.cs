using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Alerts.EditAlert.StartDate;

[Journey(JourneyNames.EditAlertStartDate), ActivatesJourney, RequireJourneyInstance]
public class IndexModel(
    SupportUiLinkGenerator linkGenerator,
    EvidenceUploadManager evidenceUploadManager,
    TimeProvider timeProvider) : PageModel
{
    public JourneyInstance<EditAlertStartDateState>? JourneyInstance { get; set; }

    [FromRoute]
    public Guid AlertId { get; set; }

    [FromQuery]
    public bool FromCheckAnswers { get; set; }

    public Guid PersonId { get; set; }

    public string? PersonName { get; set; }

    [BindProperty]
    public DateOnly? StartDate { get; set; }

    public DateOnly? PreviousStartDate { get; set; }

    public void OnGet()
    {
        StartDate = JourneyInstance!.State.StartDate;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (StartDate is null)
        {
            ModelState.AddModelError(nameof(StartDate), "Enter a start date");
        }
        else if (StartDate > timeProvider.Today)
        {
            ModelState.AddModelError(nameof(StartDate), "Start date cannot be in the future");
        }
        else if (StartDate == PreviousStartDate)
        {
            ModelState.AddModelError(nameof(StartDate), "Enter a different start date");
        }

        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        await JourneyInstance!.UpdateStateAsync(state => state.StartDate = StartDate);

        return Redirect(FromCheckAnswers
            ? linkGenerator.Alerts.EditAlert.StartDate.CheckAnswers(AlertId, JourneyInstance.InstanceId)
            : linkGenerator.Alerts.EditAlert.StartDate.Reason(AlertId, JourneyInstance!.InstanceId));
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

        JourneyInstance!.State.EnsureInitialized(alertInfo);

        PersonId = personInfo.PersonId;
        PersonName = personInfo.Name;
        PreviousStartDate = alertInfo.Alert.StartDate;
    }
}
