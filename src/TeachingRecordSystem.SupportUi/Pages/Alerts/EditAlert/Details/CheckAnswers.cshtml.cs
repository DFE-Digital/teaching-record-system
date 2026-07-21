using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Optional;
using TeachingRecordSystem.Core.Events.ChangeReasons;
using TeachingRecordSystem.Core.Services.Alerts;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Alerts.EditAlert.Details;

[Journey(JourneyNames.EditAlertDetails)]
public class CheckAnswersModel(
    EditAlertDetailsJourneyCoordinator journey,
    SupportUiLinkGenerator linkGenerator,
    EvidenceUploadManager evidenceController,
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

    public string? NewDetails { get; set; }

    public string? PreviousDetails { get; set; }

    public AlertChangeDetailsReasonOption ChangeReason { get; set; }

    public string? ChangeReasonDetail { get; set; }

    public string? AdditionalInformation { get; set; }

    public UploadedEvidenceFile? EvidenceFile { get; set; }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        BackLink = journey.GetBackLink();

        var personInfo = context.HttpContext.GetCurrentPersonFeature();
        var alertInfo = context.HttpContext.GetCurrentAlertFeature();

        PersonId = personInfo.PersonId;
        PersonName = personInfo.Name;
        NewDetails = journey.State.Details;
        PreviousDetails = alertInfo.Alert.Details;
        ChangeReason = journey.State.ChangeReason!.Value;
        ChangeReasonDetail = journey.State.ChangeReasonDetail;
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
            ProcessType.AlertUpdating,
            timeProvider.UtcNow,
            User.GetUserId(),
            new ChangeReasonWithDetailsAndEvidence
            {
                Reason = ChangeReason.GetDisplayName()!,
                Details = ChangeReasonDetail,
                EvidenceFile = EvidenceFile?.ToEventModel(),
                AdditionalInformation = AdditionalInformation
            });

        await alertService.UpdateAlertAsync(
            new UpdateAlertOptions
            {
                AlertId = alert.AlertId,
                Details = Option.Some(NewDetails)
            },
            processContext);

        journey.DeleteInstance();
        TempData.SetFlashNotificationBanner("Alert changed");

        return Redirect(linkGenerator.Persons.PersonDetail.Alerts(PersonId));
    }

    private async Task<IActionResult> CancelAsync()
    {
        await evidenceController.DeleteUploadedFileAsync(journey.State.Evidence.UploadedEvidenceFile);
        journey.DeleteInstance();
        return Redirect(linkGenerator.Persons.PersonDetail.Alerts(PersonId));
    }
}
