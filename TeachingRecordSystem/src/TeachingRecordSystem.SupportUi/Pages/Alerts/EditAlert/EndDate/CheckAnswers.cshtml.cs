using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Alerts.EditAlert.EndDate;

[Journey(JourneyNames.EditAlertEndDate), RequireJourneyInstance]
public class CheckAnswersModel(
    TrsDbContext dbContext,
    TrsLinkGenerator linkGenerator,
    EvidenceUploadManager evidenceController,
    IClock clock) : PageModel
{
    public JourneyInstance<EditAlertEndDateState>? JourneyInstance { get; set; }

    [FromRoute]
    public Guid AlertId { get; set; }

    [FromQuery]
    public bool FromCheckAnswers { get; set; }

    public Guid PersonId { get; set; }

    public string? PersonName { get; set; }

    public DateOnly? NewEndDate { get; set; }

    public DateOnly? CurrentEndDate { get; set; }

    public AlertChangeEndDateReasonOption ChangeReason { get; set; }

    public string? ChangeReasonDetail { get; set; }

    public UploadedEvidenceFile? EvidenceFile { get; set; }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        if (!JourneyInstance!.State.IsComplete)
        {
            context.Result = Redirect(linkGenerator.AlertEditEndDate(AlertId, JourneyInstance.InstanceId));
            return;
        }

        var personInfo = context.HttpContext.GetCurrentPersonFeature();
        var alertInfo = context.HttpContext.GetCurrentAlertFeature();

        PersonId = personInfo.PersonId;
        PersonName = personInfo.Name;
        NewEndDate = JourneyInstance!.State.EndDate;
        CurrentEndDate = alertInfo.Alert.EndDate;
        ChangeReason = JourneyInstance.State.ChangeReason!.Value;
        ChangeReasonDetail = JourneyInstance.State.ChangeReasonDetail;
        EvidenceFile = JourneyInstance.State.Evidence.UploadedEvidenceFile;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var alert = HttpContext.GetCurrentAlertFeature().Alert;

        alert.Update(
            a => a.EndDate = NewEndDate,
            ChangeReason.GetDisplayName(),
            ChangeReasonDetail,
            evidenceFile: EvidenceFile?.ToEventModel(),
            User.GetUserId(),
            clock.UtcNow,
            out var updatedEvent);

        if (updatedEvent is not null)
        {
            await dbContext.AddEventAndBroadcastAsync(updatedEvent);
            await dbContext.SaveChangesAsync();
        }

        await JourneyInstance!.CompleteAsync();
        TempData.SetFlashSuccess("Alert changed");

        return Redirect(linkGenerator.PersonAlerts(PersonId));
    }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await evidenceController.DeleteUploadedFileAsync(JourneyInstance!.State.Evidence.UploadedEvidenceFile);
        await JourneyInstance!.DeleteAsync();
        return Redirect(linkGenerator.AlertDetail(AlertId));
    }
}
