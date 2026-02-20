using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Optional;
using TeachingRecordSystem.Core.Events.ChangeReasons;
using TeachingRecordSystem.Core.Services.Alerts;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Alerts.EditAlert.StartDate;

[Journey(JourneyNames.EditAlertStartDate), RequireJourneyInstance]
public class CheckAnswersModel(
    SupportUiLinkGenerator linkGenerator,
    EvidenceUploadManager evidenceUploadManager,
    AlertService alertService,
    IClock clock) : PageModel
{
    public JourneyInstance<EditAlertStartDateState>? JourneyInstance { get; set; }

    [FromRoute]
    public Guid AlertId { get; set; }

    [FromQuery]
    public bool FromCheckAnswers { get; set; }

    public Guid PersonId { get; set; }

    public string? PersonName { get; set; }

    public DateOnly NewStartDate { get; set; }

    public DateOnly PreviousStartDate { get; set; }

    public AlertChangeStartDateReasonOption ChangeReason { get; set; }

    public string? ChangeReasonDetail { get; set; }

    public UploadedEvidenceFile? EvidenceFile { get; set; }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        if (!JourneyInstance!.State.IsComplete)
        {
            context.Result = Redirect(linkGenerator.Alerts.EditAlert.StartDate.Index(AlertId, JourneyInstance.InstanceId));
            return;
        }

        var personInfo = context.HttpContext.GetCurrentPersonFeature();
        var alertInfo = context.HttpContext.GetCurrentAlertFeature();

        PersonId = personInfo.PersonId;
        PersonName = personInfo.Name;
        NewStartDate = JourneyInstance.State.StartDate!.Value;
        PreviousStartDate = alertInfo.Alert.StartDate!.Value;
        ChangeReason = JourneyInstance.State.ChangeReason!.Value;
        ChangeReasonDetail = JourneyInstance.State.ChangeReasonDetail;
        EvidenceFile = JourneyInstance.State.Evidence.UploadedEvidenceFile;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var alert = HttpContext.GetCurrentAlertFeature().Alert;
        var processContext = new ProcessContext(
            ProcessType.AlertUpdating,
            clock.UtcNow,
            User.GetUserId(),
            new ChangeReasonWithDetailsAndEvidence
            {
                Reason = ChangeReason.GetDisplayName()!,
                Details = ChangeReasonDetail,
                EvidenceFile = EvidenceFile?.ToEventModel()
            });

        var changes = await alertService.UpdateAlertAsync(
            new UpdateAlertOptions
            {
                AlertId = alert.AlertId,
                StartDate = Option.Some<DateOnly?>(NewStartDate)
            },
            processContext);

        await JourneyInstance!.CompleteAsync();
        TempData.SetFlashSuccess("Alert changed");

        return Redirect(linkGenerator.Persons.PersonDetail.Alerts(PersonId));
    }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await evidenceUploadManager.DeleteUploadedFileAsync(JourneyInstance!.State.Evidence.UploadedEvidenceFile);
        await JourneyInstance!.DeleteAsync();
        return Redirect(linkGenerator.Persons.PersonDetail.Alerts(PersonId));
    }
}
