using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Optional;
using TeachingRecordSystem.Core.Events.ChangeReasons;
using TeachingRecordSystem.Core.Services.Alerts;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Alerts.ReopenAlert;

[Journey(JourneyNames.ReopenAlert)]
public class CheckAnswersModel(
    ReopenAlertJourneyCoordinator journey,
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

    public ReopenAlertReasonOption ChangeReason { get; set; }

    public string? ChangeReasonDetail { get; set; }

    public UploadedEvidenceFile? EvidenceFile { get; set; }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
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
        ChangeReason = journey.State.ChangeReason!.Value;
        ChangeReasonDetail = journey.State.ChangeReasonDetail;
        EvidenceFile = journey.State.Evidence.UploadedEvidenceFile;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Cancel)
        {
            return await CancelAsync();
        }

        var alert = HttpContext.GetCurrentAlertFeature().Alert;
        var processContext = new ProcessContext(
            ProcessType.AlertUpdating,
            timeProvider.UtcNow,
            User.GetUserId(),
            new ChangeReasonWithDetailsAndEvidence
            {
                Reason = ChangeReason.GetDisplayName()!,
                Details = ChangeReasonDetail,
                EvidenceFile = EvidenceFile?.ToEventModel(),
                AdditionalInformation = null
            });

        await alertService.UpdateAlertAsync(
            new UpdateAlertOptions
            {
                AlertId = alert.AlertId,
                EndDate = Option.Some<DateOnly?>(null)
            },
            processContext);

        journey.DeleteInstance();
        TempData.SetFlashNotificationBanner("Alert re-opened");

        return Redirect(linkGenerator.Persons.PersonDetail.Alerts(PersonId));
    }

    private async Task<IActionResult> CancelAsync()
    {
        await evidenceUploadManager.DeleteUploadedFileAsync(journey.State.Evidence.UploadedEvidenceFile);
        journey.DeleteInstance();
        return Redirect(linkGenerator.Alerts.AlertDetail(AlertId));
    }
}
