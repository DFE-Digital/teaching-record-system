using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Alerts.EditAlert.EndDate;

[Journey(JourneyNames.EditAlertEndDate), ActivatesJourney, RequireJourneyInstance]
public class IndexModel(SupportUiLinkGenerator linkGenerator, EvidenceUploadManager evidenceController, IClock clock) : PageModel
{
    public JourneyInstance<EditAlertEndDateState>? JourneyInstance { get; set; }

    [FromRoute]
    public Guid AlertId { get; set; }

    [FromQuery]
    public bool FromCheckAnswers { get; set; }

    public Guid PersonId { get; set; }

    public string? PersonName { get; set; }

    [BindProperty]
    [DateInput(ErrorMessagePrefix = "End date")]
    [Display(Name = "Enter a new end date")]
    public DateOnly? EndDate { get; set; }

    public DateOnly? CurrentEndDate { get; set; }

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
        else if (EndDate == CurrentEndDate)
        {
            ModelState.AddModelError(nameof(EndDate), "Enter a different end date");
        }

        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        await JourneyInstance!.UpdateStateAsync(state => state.EndDate = EndDate);

        return Redirect(FromCheckAnswers
            ? linkGenerator.Alerts.EditAlert.EndDate.CheckAnswers(AlertId, JourneyInstance.InstanceId)
            : linkGenerator.Alerts.EditAlert.EndDate.Reason(AlertId, JourneyInstance!.InstanceId));
    }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await evidenceController.DeleteUploadedFileAsync(JourneyInstance!.State.Evidence.UploadedEvidenceFile);
        await JourneyInstance!.DeleteAsync();
        return Redirect(linkGenerator.Alerts.EditAlert.Details.Index(AlertId, JourneyInstance!.InstanceId));
    }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        var alertInfo = context.HttpContext.GetCurrentAlertFeature();
        var personInfo = context.HttpContext.GetCurrentPersonFeature();

        JourneyInstance!.State.EnsureInitialized(alertInfo);

        PersonId = personInfo.PersonId;
        PersonName = personInfo.Name;
        CurrentEndDate = alertInfo.Alert.EndDate;
        StartDate = alertInfo.Alert.StartDate;
    }
}
