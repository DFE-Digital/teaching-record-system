using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Services.Files;

namespace TeachingRecordSystem.SupportUi.Pages.Alerts.EditAlert.Link;

[Journey(JourneyNames.EditAlertLink), RequireJourneyInstance]
public class CheckAnswersModel(
    TrsDbContext dbContext,
    TrsLinkGenerator linkGenerator,
    IFileService fileService,
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

    public string? CurrentLink { get; set; }

    public Uri? CurrentLinkUri { get; set; }

    public AlertChangeLinkReasonOption ChangeReason { get; set; }

    public string? ChangeReasonDetail { get; set; }

    public string? EvidenceFileName { get; set; }

    public string? EvidenceFileSizeDescription { get; set; }

    public string? UploadedEvidenceFileUrl { get; set; }

    public async Task<IActionResult> OnPostAsync()
    {
        var now = clock.UtcNow;

        var alert = await dbContext.Alerts
            .SingleAsync(a => a.AlertId == AlertId);

        var changes = NewLink != alert.ExternalLink ?
            AlertUpdatedEventChanges.ExternalLink :
            AlertUpdatedEventChanges.None;

        if (changes != AlertUpdatedEventChanges.None)
        {
            var oldAlertEventModel = EventModels.Alert.FromModel(alert);

            alert.ExternalLink = NewLink;
            alert.UpdatedOn = now;

            var updatedEvent = new AlertUpdatedEvent()
            {
                EventId = Guid.NewGuid(),
                CreatedUtc = now,
                RaisedBy = User.GetUserId(),
                PersonId = PersonId,
                Alert = EventModels.Alert.FromModel(alert),
                OldAlert = oldAlertEventModel,
                ChangeReason = ChangeReason!.GetDisplayName(),
                ChangeReasonDetail = ChangeReasonDetail,
                EvidenceFile = JourneyInstance!.State.EvidenceFileId is Guid fileId ?
                    new EventModels.File()
                    {
                        FileId = fileId,
                        Name = JourneyInstance.State.EvidenceFileName!
                    } :
                    null,
                Changes = changes
            };

            dbContext.AddEvent(updatedEvent);

            await dbContext.SaveChangesAsync();
        }

        await JourneyInstance!.CompleteAsync();
        TempData.SetFlashSuccess("Alert changed");

        return Redirect(linkGenerator.PersonAlerts(PersonId));
    }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await JourneyInstance!.DeleteAsync();
        return Redirect(linkGenerator.PersonAlerts(PersonId));
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        if (!JourneyInstance!.State.IsComplete)
        {
            context.Result = Redirect(linkGenerator.AlertEditLink(AlertId, JourneyInstance.InstanceId));
            return;
        }

        var personInfo = context.HttpContext.GetCurrentPersonFeature();
        var alertInfo = context.HttpContext.GetCurrentAlertFeature();

        PersonId = personInfo.PersonId;
        PersonName = personInfo.Name;
        NewLink = JourneyInstance!.State.Link;
        NewLinkUri = TrsUriHelper.TryCreateWebsiteUri(NewLink, out var newLinkUri) ? newLinkUri : null;
        CurrentLink = alertInfo.Alert.ExternalLink;
        CurrentLinkUri = TrsUriHelper.TryCreateWebsiteUri(CurrentLink, out var currentLinkUri) ? currentLinkUri : null;
        ChangeReason = JourneyInstance.State.ChangeReason!.Value;
        ChangeReasonDetail = JourneyInstance.State.ChangeReasonDetail;
        EvidenceFileName = JourneyInstance.State.EvidenceFileName;
        UploadedEvidenceFileUrl = JourneyInstance!.State.EvidenceFileId is not null ?
            await fileService.GetFileUrlAsync(JourneyInstance!.State.EvidenceFileId!.Value, AlertDefaults.FileUrlExpiry) :
            null;

        await next();
    }
}
