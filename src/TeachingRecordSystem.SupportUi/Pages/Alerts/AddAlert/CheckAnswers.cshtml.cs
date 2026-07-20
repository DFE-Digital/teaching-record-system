using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.Events.ChangeReasons;
using TeachingRecordSystem.Core.Services.Alerts;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Alerts.AddAlert;

[Journey(JourneyNames.AddAlert)]
public class CheckAnswersModel(
    AddAlertJourneyCoordinator journey,
    SupportUiLinkGenerator linkGenerator,
    EvidenceUploadManager evidenceUploadManager,
    AlertService alertService,
    TimeProvider timeProvider) : PageModel
{
    public JourneyInstanceId InstanceId => journey.InstanceId;

    public string? BackLink { get; set; }

    [FromQuery]
    public Guid PersonId { get; set; }

    [BindProperty]
    public bool Cancel { get; set; }

    public string? PersonName { get; set; }

    public Guid AlertTypeId { get; set; }

    public string? AlertTypeName { get; set; }

    public string? Details { get; set; }

    public string? Link { get; set; }

    public Uri? LinkUri { get; set; }

    public DateOnly StartDate { get; set; }

    public AddAlertReasonOption AddReason { get; set; }

    public string? AddReasonDetail { get; set; }

    public string? AdditionalInformation { get; set; }

    [BindProperty]
    public UploadedEvidenceFile? EvidenceFile { get; set; }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        if (!journey.State.IsComplete)
        {
            context.Result = Redirect(linkGenerator.Alerts.AddAlert.Reason(journey.InstanceId));
            return;
        }

        BackLink = journey.GetBackLink();

        var personInfo = context.HttpContext.GetCurrentPersonFeature();

        PersonName = personInfo.Name;
        AlertTypeId = journey.State.AlertTypeId!.Value;
        AlertTypeName = journey.State.AlertTypeName;
        Details = journey.State.Details;
        Link = journey.State.Link;
        LinkUri = TrsUriHelper.TryCreateWebsiteUri(Link, out var linkUri) ? linkUri : null;
        StartDate = journey.State.StartDate!.Value;
        AddReason = journey.State.AddReason!.Value;
        AddReasonDetail = journey.State.AddReasonDetail;
        EvidenceFile = journey.State.Evidence.UploadedEvidenceFile;
        AdditionalInformation = journey.State.AdditionalInformation;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Cancel)
        {
            return await CancelAsync();
        }

        var processContext = new ProcessContext(
            ProcessType.AlertCreating,
            timeProvider.UtcNow,
            User.GetUserId(),
            new ChangeReasonWithDetailsAndEvidence
            {
                Reason = AddReason.GetDisplayName()!,
                Details = AddReasonDetail,
                EvidenceFile = EvidenceFile?.ToEventModel(),
                AdditionalInformation = AdditionalInformation
            });

        await alertService.CreateAlertAsync(
            new CreateAlertOptions
            {
                PersonId = PersonId,
                AlertTypeId = AlertTypeId,
                Details = Details,
                ExternalLink = Link,
                StartDate = StartDate,
                EndDate = null
            },
            processContext);

        journey.DeleteInstance();
        TempData.SetFlashNotificationBanner("Alert added");

        return Redirect(linkGenerator.Persons.PersonDetail.Alerts(PersonId));
    }

    private async Task<IActionResult> CancelAsync()
    {
        await evidenceUploadManager.DeleteUploadedFileAsync(journey.State.Evidence.UploadedEvidenceFile);
        journey.DeleteInstance();
        return Redirect(linkGenerator.Persons.PersonDetail.Alerts(PersonId));
    }
}
