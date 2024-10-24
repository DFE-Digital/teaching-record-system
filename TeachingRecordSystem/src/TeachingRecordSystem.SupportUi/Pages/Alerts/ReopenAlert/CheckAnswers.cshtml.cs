using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Services.Files;

namespace TeachingRecordSystem.SupportUi.Pages.Alerts.ReopenAlert;

[Journey(JourneyNames.ReopenAlert), RequireJourneyInstance]
public class CheckAnswersModel(
    TrsDbContext dbContext,
    TrsLinkGenerator linkGenerator,
    IFileService fileService,
    IClock clock,
    ReferenceDataCache referenceDataCache) : PageModel
{
    private static readonly TimeSpan _fileUrlExpiresAfter = TimeSpan.FromMinutes(15);

    public JourneyInstance<ReopenAlertState>? JourneyInstance { get; set; }

    [FromRoute]
    public Guid AlertId { get; set; }

    [FromQuery]
    public bool FromCheckAnswers { get; set; }

    public Guid PersonId { get; set; }

    public string? PersonName { get; set; }

    public string? AlertTypeName { get; set; }

    public string? Details { get; set; }

    public string? Link { get; set; }

    public DateOnly? StartDate { get; set; }

    public string? ChangeReason { get; set; }

    public string? EvidenceFileName { get; set; }

    public string? EvidenceFileSizeDescription { get; set; }

    public string? UploadedEvidenceFileUrl { get; set; }

    public async Task<IActionResult> OnPost()
    {
        var now = clock.UtcNow;

        var alert = await dbContext.Alerts
            .SingleAsync(a => a.AlertId == AlertId);

        var oldAlertEventModel = EventModels.Alert.FromModel(alert);
        alert.EndDate = null;
        alert.UpdatedOn = now;

        var updatedEvent = new AlertUpdatedEvent()
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = now,
            RaisedBy = User.GetUserId(),
            PersonId = PersonId,
            Alert = EventModels.Alert.FromModel(alert),
            OldAlert = oldAlertEventModel,
            ChangeReason = null!,
            ChangeReasonDetail = ChangeReason,  // FIXME
            EvidenceFile = JourneyInstance!.State.EvidenceFileId is Guid fileId ?
                new EventModels.File()
                {
                    FileId = fileId,
                    Name = JourneyInstance.State.EvidenceFileName!
                } :
                null,
            Changes = AlertUpdatedEventChanges.EndDate
        };

        dbContext.AddEvent(updatedEvent);

        await dbContext.SaveChangesAsync();

        await JourneyInstance!.CompleteAsync();
        TempData.SetFlashSuccess("Alert re-opened");

        return Redirect(linkGenerator.PersonAlerts(PersonId));
    }

    public async Task<IActionResult> OnPostCancel()
    {
        await JourneyInstance!.DeleteAsync();
        return Redirect(linkGenerator.AlertDetail(AlertId));
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        if (!JourneyInstance!.State.IsComplete)
        {
            context.Result = Redirect(linkGenerator.AlertReopen(AlertId, JourneyInstance.InstanceId));
            return;
        }

        var personInfo = context.HttpContext.GetCurrentPersonFeature();
        var alertInfo = context.HttpContext.GetCurrentAlertFeature();
        var alertType = await referenceDataCache.GetAlertTypeById(alertInfo.Alert.AlertTypeId);

        PersonId = personInfo.PersonId;
        PersonName = personInfo.Name;
        AlertTypeName = alertType.Name;
        Details = alertInfo.Alert.Details;
        Link = alertInfo.Alert.ExternalLink;
        StartDate = alertInfo.Alert.StartDate;
        ChangeReason = JourneyInstance.State.ChangeReason != ReopenAlertReasonOption.AnotherReason ?
            JourneyInstance.State.ChangeReason!.GetDisplayName() :
            JourneyInstance!.State.ChangeReasonDetail;
        EvidenceFileName = JourneyInstance.State.EvidenceFileName;
        UploadedEvidenceFileUrl = JourneyInstance!.State.EvidenceFileId is not null ?
            await fileService.GetFileUrl(JourneyInstance!.State.EvidenceFileId!.Value, _fileUrlExpiresAfter) :
            null;

        await next();
    }
}
