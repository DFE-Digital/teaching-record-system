using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.Events.ChangeReasons;
using TeachingRecordSystem.Core.Services.Alerts;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Alerts.DeleteAlert;

[Journey(JourneyNames.DeleteAlert)]
public class CheckAnswersModel(
    DeleteAlertJourneyCoordinator journey,
    SupportUiLinkGenerator linkGenerator,
    EvidenceUploadManager evidenceUploadManager,
    AlertService alertService,
    TimeProvider timeProvider) : PageModel
{
    public JourneyInstanceId InstanceId => journey.InstanceId;

    public string? BackLink { get; set; }

    [FromRoute]
    public Guid AlertId { get; set; }

    [BindProperty]
    public bool Cancel { get; set; }

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

    public string? AdditionalInformation { get; set; }

    public UploadedEvidenceFile? EvidenceFile { get; set; }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        if (!journey.State.IsComplete)
        {
            context.Result = Redirect(linkGenerator.Alerts.DeleteAlert.Index(journey.InstanceId));
            return;
        }

        BackLink = journey.GetBackLink();

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
        DeleteReason = journey.State.DeleteReason!.Value;
        DeleteReasonDetail = journey.State.DeleteReasonDetail;
        EvidenceFile = journey.State.Evidence.UploadedEvidenceFile;
        AdditionalInformation = journey.State.AdditionalInformation;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Cancel)
        {
            return await CancelAsync();
        }

        var alert = HttpContext.GetCurrentAlertFeature().Alert;
        var processContext = new ProcessContext(
            ProcessType.AlertDeleting,
            timeProvider.UtcNow,
            User.GetUserId(),
            new ChangeReasonWithDetailsAndEvidence
            {
                Reason = DeleteReason.GetDisplayName()!,
                Details = DeleteReasonDetail,
                EvidenceFile = EvidenceFile?.ToEventModel(),
                AdditionalInformation = AdditionalInformation
            });

        await alertService.DeleteAlertAsync(
            new DeleteAlertOptions
            {
                AlertId = alert.AlertId
            },
            processContext);

        journey.DeleteInstance();
        TempData.SetFlashNotificationBanner("Alert deleted");

        return Redirect(linkGenerator.Persons.PersonDetail.Alerts(PersonId));
    }

    private async Task<IActionResult> CancelAsync()
    {
        await evidenceUploadManager.DeleteUploadedFileAsync(journey.State.Evidence.UploadedEvidenceFile);
        journey.DeleteInstance();
        return Redirect(EndDate is null ? linkGenerator.Persons.PersonDetail.Alerts(PersonId) : linkGenerator.Alerts.AlertDetail(AlertId));
    }
}
