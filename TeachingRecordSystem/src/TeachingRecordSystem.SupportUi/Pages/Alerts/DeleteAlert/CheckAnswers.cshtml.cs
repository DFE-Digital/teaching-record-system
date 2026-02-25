using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.Events.ChangeReasons;
using TeachingRecordSystem.Core.Services.Alerts;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Alerts.DeleteAlert;

[Journey(JourneyNames.DeleteAlert), RequireJourneyInstance]
public class CheckAnswersModel(
    SupportUiLinkGenerator linkGenerator,
    EvidenceUploadManager evidenceUploadManager,
    AlertService alertService,
    TimeProvider timeProvider) : PageModel
{
    public JourneyInstance<DeleteAlertState>? JourneyInstance { get; set; }

    [FromRoute]
    public Guid AlertId { get; set; }

    public Guid PersonId { get; set; }

    public string? PersonName { get; set; }

    public string? AlertTypeName { get; set; }

    public string? Details { get; set; }

    public string? Link { get; set; }

    public Uri? LinkUri { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public DeleteAlertReasonOption DeleteReason { get; set; }

    public string? DeleteReasonDetail { get; set; }

    public UploadedEvidenceFile? EvidenceFile { get; set; }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        if (!JourneyInstance!.State.IsComplete)
        {
            context.Result = Redirect(linkGenerator.Alerts.DeleteAlert.Index(AlertId, JourneyInstance.InstanceId));
            return;
        }

        var personInfo = context.HttpContext.GetCurrentPersonFeature();
        var alertInfo = context.HttpContext.GetCurrentAlertFeature();

        PersonId = personInfo.PersonId;
        PersonName = personInfo.Name;
        AlertTypeName = alertInfo.Alert.AlertType!.Name;
        Details = alertInfo.Alert.Details;
        Link = alertInfo.Alert.ExternalLink;
        LinkUri = TrsUriHelper.TryCreateWebsiteUri(Link, out var linkUri) ? linkUri : null;
        StartDate = alertInfo.Alert.StartDate;
        EndDate = alertInfo.Alert.EndDate;
        DeleteReason = JourneyInstance.State.DeleteReason!.Value;
        DeleteReasonDetail = JourneyInstance.State.DeleteReasonDetail;
        EvidenceFile = JourneyInstance.State.Evidence.UploadedEvidenceFile;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var alert = HttpContext.GetCurrentAlertFeature().Alert;
        var processContext = new ProcessContext(
            ProcessType.AlertDeleting,
            timeProvider.UtcNow,
            User.GetUserId(),
            new ChangeReasonWithDetailsAndEvidence
            {
                Reason = DeleteReason.GetDisplayName()!,
                Details = DeleteReasonDetail,
                EvidenceFile = EvidenceFile?.ToEventModel()
            });

        await alertService.DeleteAlertAsync(
            new DeleteAlertOptions
            {
                AlertId = alert.AlertId
            },
            processContext);

        await JourneyInstance!.CompleteAsync();
        TempData.SetFlashSuccess("Alert deleted");

        return Redirect(linkGenerator.Persons.PersonDetail.Alerts(PersonId));
    }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await evidenceUploadManager.DeleteUploadedFileAsync(JourneyInstance!.State.Evidence.UploadedEvidenceFile);
        await JourneyInstance!.DeleteAsync();
        return Redirect(EndDate is null ? linkGenerator.Persons.PersonDetail.Alerts(PersonId) : linkGenerator.Alerts.AlertDetail(AlertId));
    }
}
