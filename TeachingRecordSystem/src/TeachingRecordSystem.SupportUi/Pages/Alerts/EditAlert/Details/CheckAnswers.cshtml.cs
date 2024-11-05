using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Services.Files;

namespace TeachingRecordSystem.SupportUi.Pages.Alerts.EditAlert.Details;

[Journey(JourneyNames.EditAlertDetails), RequireJourneyInstance]
public class CheckAnswersModel(
    TrsDbContext dbContext,
    TrsLinkGenerator linkGenerator,
    IFileService fileService,
    IClock clock) : PageModel
{
    private static readonly TimeSpan _fileUrlExpiresAfter = TimeSpan.FromMinutes(AlertDefaults.FileUrlExpiryMinutes);

    public JourneyInstance<EditAlertDetailsState>? JourneyInstance { get; set; }

    [FromRoute]
    public Guid AlertId { get; set; }

    [FromQuery]
    public bool FromCheckAnswers { get; set; }

    public Guid PersonId { get; set; }

    public string? PersonName { get; set; }

    public string? NewDetails { get; set; }

    public string? CurrentDetails { get; set; }

    public AlertChangeDetailsReasonOption ChangeReason { get; set; }

    public string? ChangeReasonDetail { get; set; }

    public string? EvidenceFileName { get; set; }

    public string? EvidenceFileSizeDescription { get; set; }

    public string? UploadedEvidenceFileUrl { get; set; }

    public async Task<IActionResult> OnPost()
    {
        var now = clock.UtcNow;

        var alert = await dbContext.Alerts
            .SingleAsync(a => a.AlertId == AlertId);

        var changes = NewDetails != alert.Details ?
            AlertUpdatedEventChanges.Details :
            AlertUpdatedEventChanges.None;

        if (changes != AlertUpdatedEventChanges.None)
        {
            var oldAlertEventModel = EventModels.Alert.FromModel(alert);

            alert.Details = NewDetails;
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

    public async Task<IActionResult> OnPostCancel()
    {
        await JourneyInstance!.DeleteAsync();
        return Redirect(linkGenerator.PersonAlerts(PersonId));
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        if (!JourneyInstance!.State.IsComplete)
        {
            context.Result = Redirect(linkGenerator.AlertEditDetails(AlertId, JourneyInstance.InstanceId));
            return;
        }

        var personInfo = context.HttpContext.GetCurrentPersonFeature();
        var alertInfo = context.HttpContext.GetCurrentAlertFeature();

        PersonId = personInfo.PersonId;
        PersonName = personInfo.Name;
        NewDetails = JourneyInstance!.State.Details;
        CurrentDetails = alertInfo.Alert.Details;
        ChangeReason = JourneyInstance.State.ChangeReason!.Value;
        ChangeReasonDetail = JourneyInstance.State.ChangeReasonDetail;
        EvidenceFileName = JourneyInstance.State.EvidenceFileName;
        UploadedEvidenceFileUrl = JourneyInstance!.State.EvidenceFileId is not null ?
            await fileService.GetFileUrl(JourneyInstance!.State.EvidenceFileId!.Value, _fileUrlExpiresAfter) :
            null;

        await next();
    }
}
