using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Optional;
using TeachingRecordSystem.Core.Events.ChangeReasons;
using TeachingRecordSystem.Core.Services.Alerts;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Alerts.EditAlert.Link;

[Journey(JourneyNames.EditAlertLink), RequireJourneyInstance]
public class CheckAnswersModel(
    SupportUiLinkGenerator linkGenerator,
    EvidenceUploadManager evidenceController,
    AlertService alertService,
    IClock clock) : PageModel
{
    public JourneyInstance<EditAlertLinkState>? JourneyInstance { get; set; }

    [FromRoute]
    public Guid AlertId { get; set; }

    [FromQuery]
    public bool FromCheckAnswers { get; set; }

    public Guid PersonId { get; set; }

    public string? PersonName { get; set; }

    public string? NewLink { get; set; }

    public Uri? NewLinkUri { get; set; }

    public string? PreviousLink { get; set; }

    public Uri? PreviousLinkUri { get; set; }

    public AlertChangeLinkReasonOption ChangeReason { get; set; }

    public string? ChangeReasonDetail { get; set; }

    public UploadedEvidenceFile? EvidenceFile { get; set; }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        if (!JourneyInstance!.State.IsComplete)
        {
            context.Result = Redirect(linkGenerator.Alerts.EditAlert.Link.Index(AlertId, JourneyInstance.InstanceId));
            return;
        }

        var personInfo = context.HttpContext.GetCurrentPersonFeature();
        var alertInfo = context.HttpContext.GetCurrentAlertFeature();

        PersonId = personInfo.PersonId;
        PersonName = personInfo.Name;
        NewLink = JourneyInstance!.State.Link;
        NewLinkUri = TrsUriHelper.TryCreateWebsiteUri(NewLink, out var newLinkUri) ? newLinkUri : null;
        PreviousLink = alertInfo.Alert.ExternalLink;
        PreviousLinkUri = TrsUriHelper.TryCreateWebsiteUri(PreviousLink, out var currentLinkUri) ? currentLinkUri : null;
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
                ExternalLink = Option.Some<string?>(NewLink)
            },
            processContext);

        await JourneyInstance!.CompleteAsync();
        TempData.SetFlashSuccess("Alert changed");

        return Redirect(linkGenerator.Persons.PersonDetail.Alerts(PersonId));
    }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await evidenceController.DeleteUploadedFileAsync(JourneyInstance!.State.Evidence.UploadedEvidenceFile);
        await JourneyInstance!.DeleteAsync();
        return Redirect(linkGenerator.Persons.PersonDetail.Alerts(PersonId));
    }
}
