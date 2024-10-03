using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Services.Files;
using TeachingRecordSystem.SupportUi.Infrastructure.DataAnnotations;

namespace TeachingRecordSystem.SupportUi.Pages.Alerts.DeleteAlert;

[Journey(JourneyNames.DeleteAlert), RequireJourneyInstance]
public class ConfirmModel(
    TrsDbContext dbContext,
    TrsLinkGenerator linkGenerator,
    IFileService fileService,
    IClock clock) : PageModel
{
    public const int MaxFileSizeMb = 50;

    private static readonly TimeSpan _fileUrlExpiresAfter = TimeSpan.FromMinutes(15);

    public JourneyInstance<DeleteAlertState>? JourneyInstance { get; set; }

    [FromRoute]
    public Guid AlertId { get; set; }

    public Guid PersonId { get; set; }

    public string? PersonName { get; set; }

    public string? AlertTypeName { get; set; }

    public string? Details { get; set; }

    public string? Link { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    [BindProperty]
    [Display(Name = "Do you want to add additional detail?")]
    [Required(ErrorMessage = "Select yes if you want to add additional detail")]
    public bool? HasAdditionalDetail { get; set; }

    [BindProperty]
    [Display(Name = "Add additional detail")]
    public string? AdditionalDetail { get; set; }

    [BindProperty]
    [Display(Name = "Upload evidence")]
    [Required(ErrorMessage = "Select yes if you want to upload evidence")]
    public bool? UploadEvidence { get; set; }

    [BindProperty]
    [EvidenceFile]
    [FileSize(MaxFileSizeMb * 1024 * 1024, ErrorMessage = "The selected file must be smaller than 50MB")]
    public IFormFile? EvidenceFile { get; set; }

    public async Task<IActionResult> OnPost()
    {
        if (HasAdditionalDetail == true && AdditionalDetail is null)
        {
            ModelState.AddModelError(nameof(AdditionalDetail), "Add additional detail");
        }

        if (UploadEvidence == true && EvidenceFile is null)
        {
            ModelState.AddModelError(nameof(EvidenceFile), "Select a file");
        }

        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        var now = clock.UtcNow;
        Guid? evidenceFileId = null;
        if (UploadEvidence == true)
        {
            using var stream = EvidenceFile!.OpenReadStream();
            evidenceFileId = await fileService.UploadFile(stream, EvidenceFile.ContentType);
        }

        var alert = await dbContext.Alerts
            .SingleAsync(a => a.AlertId == AlertId);

        var oldAlertEventModel = EventModels.Alert.FromModel(alert);
        alert.DeletedOn = now;
        alert.UpdatedOn = now;

        var deletedEvent = new AlertDeletedEvent()
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = now,
            RaisedBy = User.GetUserId(),
            PersonId = PersonId,
            Alert = EventModels.Alert.FromModel(alert),
            Reason = HasAdditionalDetail == true ? AdditionalDetail : null,
            EvidenceFile = evidenceFileId is Guid fileId ?
                new EventModels.File()
                {
                    FileId = fileId,
                    Name = EvidenceFile!.FileName
                } :
                null
        };

        dbContext.AddEvent(deletedEvent);

        await dbContext.SaveChangesAsync();

        await JourneyInstance!.CompleteAsync();
        TempData.SetFlashSuccess("Alert deleted");

        return Redirect(linkGenerator.PersonAlerts(PersonId));
    }

    public async Task<IActionResult> OnPostCancel()
    {
        await JourneyInstance!.DeleteAsync();
        return Redirect(EndDate is null ? linkGenerator.PersonAlerts(PersonId) : linkGenerator.Alert(AlertId));
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        if (JourneyInstance!.State.ConfirmDelete is null)
        {
            context.Result = Redirect(linkGenerator.AlertDelete(AlertId, JourneyInstance.InstanceId));
            return;
        }

        var personInfo = context.HttpContext.GetCurrentPersonFeature();
        var alertInfo = context.HttpContext.GetCurrentAlertFeature();

        PersonId = personInfo.PersonId;
        PersonName = personInfo.Name;
        AlertTypeName = alertInfo.Alert.AlertType.Name;
        Details = alertInfo.Alert.Details;
        Link = alertInfo.Alert.ExternalLink;
        StartDate = alertInfo.Alert.StartDate;
        EndDate = alertInfo.Alert.EndDate;

        await next();
    }
}
