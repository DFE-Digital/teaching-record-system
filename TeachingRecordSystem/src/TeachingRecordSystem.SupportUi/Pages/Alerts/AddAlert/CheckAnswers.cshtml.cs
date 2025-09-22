using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Services.Files;

namespace TeachingRecordSystem.SupportUi.Pages.Alerts.AddAlert;

[Journey(JourneyNames.AddAlert), RequireJourneyInstance]
public class CheckAnswersModel(
    TrsDbContext dbContext,
    IEventPublisher eventPublisher,
    TrsLinkGenerator linkGenerator,
    IFileService fileService,
    IClock clock) : PageModel
{
    public JourneyInstance<AddAlertState>? JourneyInstance { get; set; }

    [FromQuery]
    public Guid PersonId { get; set; }

    [FromQuery]
    public bool FromCheckAnswers { get; set; }

    public string? PersonName { get; set; }

    public Guid AlertTypeId { get; set; }

    public string? AlertTypeName { get; set; }

    public string? Details { get; set; }

    public string? Link { get; set; }

    public Uri? LinkUri { get; set; }

    public DateOnly StartDate { get; set; }

    public AddAlertReasonOption AddReason { get; set; }

    public string? AddReasonDetail { get; set; }

    public string? EvidenceFileName { get; set; }

    public string? EvidenceFileSizeDescription { get; set; }

    public string? UploadedEvidenceFileUrl { get; set; }

    public async Task<IActionResult> OnPostAsync()
    {
        var now = clock.UtcNow;

        var alert = Alert.Create(
            AlertTypeId,
            PersonId,
            Details,
            Link,
            StartDate,
            endDate: null,
            AddReason.GetDisplayName(),
            AddReasonDetail,
            evidenceFile: JourneyInstance!.State.EvidenceFileId is Guid fileId
                ? new EventModels.File()
                {
                    FileId = fileId,
                    Name = JourneyInstance.State.EvidenceFileName!
                }
                : null,
            User.GetUserId(),
            clock.UtcNow,
            out var createdEvent);

        dbContext.Alerts.Add(alert);
        await dbContext.SaveChangesAsync();

        await eventPublisher.PublishEventAsync(createdEvent);

        await JourneyInstance!.CompleteAsync();
        TempData.SetFlashSuccess("Alert added");

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
            context.Result = Redirect(linkGenerator.AlertAddReason(PersonId, JourneyInstance.InstanceId));
            return;
        }

        var personInfo = context.HttpContext.GetCurrentPersonFeature();

        PersonName = personInfo.Name;
        AlertTypeId = JourneyInstance!.State.AlertTypeId!.Value;
        AlertTypeName = JourneyInstance!.State.AlertTypeName;
        Details = JourneyInstance!.State.Details;
        Link = JourneyInstance!.State.Link;
        LinkUri = TrsUriHelper.TryCreateWebsiteUri(Link, out var linkUri) ? linkUri : null;
        StartDate = JourneyInstance!.State.StartDate!.Value;
        AddReason = JourneyInstance!.State.AddReason!.Value;
        AddReasonDetail = JourneyInstance!.State.AddReasonDetail;
        EvidenceFileName = JourneyInstance.State.EvidenceFileName;
        UploadedEvidenceFileUrl = JourneyInstance!.State.EvidenceFileId is not null ?
            await fileService.GetFileUrlAsync(JourneyInstance!.State.EvidenceFileId!.Value, FileUploadDefaults.FileUrlExpiry) :
            null;

        await next();
    }
}
