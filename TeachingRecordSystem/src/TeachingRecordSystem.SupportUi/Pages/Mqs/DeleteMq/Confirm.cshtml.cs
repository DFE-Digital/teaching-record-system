using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Services.Files;

namespace TeachingRecordSystem.SupportUi.Pages.Mqs.DeleteMq;

[Journey(JourneyNames.DeleteMq), RequireJourneyInstance]
public class ConfirmModel(
    TrsDbContext dbContext,
    TrsLinkGenerator linkGenerator,
    IFileService fileService,
    IClock clock) : PageModel
{
    private static readonly TimeSpan _fileUrlExpiresAfter = TimeSpan.FromMinutes(15);

    public JourneyInstance<DeleteMqState>? JourneyInstance { get; set; }

    [FromRoute]
    public Guid QualificationId { get; set; }

    public Guid PersonId { get; set; }

    public string? PersonName { get; set; }

    public string? ProviderName { get; set; }

    public MandatoryQualificationSpecialism? Specialism { get; set; }

    public MandatoryQualificationStatus? Status { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public MqDeletionReasonOption? DeletionReason { get; set; }

    public string? DeletionReasonDetail { get; set; }

    public string? EvidenceFileName { get; set; }

    public string? UploadedEvidenceFileUrl { get; set; }

    public async Task<IActionResult> OnPost()
    {
        var now = clock.UtcNow;

        var qualification = await dbContext.MandatoryQualifications
            .Include(q => q.Provider)
            .SingleAsync(q => q.QualificationId == QualificationId);

        qualification.DeletedOn = now;

        var deletedEvent = new MandatoryQualificationDeletedEvent()
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = now,
            RaisedBy = User.GetUserId(),
            PersonId = PersonId,
            MandatoryQualification = EventModels.MandatoryQualification.FromModel(qualification),
            DeletionReason = DeletionReason!.GetDisplayName(),
            DeletionReasonDetail = DeletionReasonDetail,
            EvidenceFile = JourneyInstance!.State.EvidenceFileId is Guid fileId ?
                new EventModels.File()
                {
                    FileId = fileId,
                    Name = JourneyInstance.State.EvidenceFileName!
                } :
                null
        };
        dbContext.AddEvent(deletedEvent);

        await dbContext.SaveChangesAsync();

        await JourneyInstance!.CompleteAsync();
        TempData.SetFlashSuccess("Mandatory qualification deleted");

        return Redirect(linkGenerator.PersonQualifications(PersonId));
    }

    public async Task<IActionResult> OnPostCancel()
    {
        if (JourneyInstance!.State.EvidenceFileId is not null)
        {
            await fileService.DeleteFile(JourneyInstance!.State.EvidenceFileId.Value);
        }

        await JourneyInstance!.DeleteAsync();
        return Redirect(linkGenerator.PersonQualifications(PersonId));
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        if (!JourneyInstance!.State.IsComplete)
        {
            context.Result = Redirect(linkGenerator.MqDelete(QualificationId, JourneyInstance.InstanceId));
            return;
        }

        var qualificationInfo = context.HttpContext.GetCurrentMandatoryQualificationFeature();
        var personInfo = context.HttpContext.GetCurrentPersonFeature();

        PersonId = personInfo.PersonId;
        PersonName = personInfo.Name;
        ProviderName = qualificationInfo.MandatoryQualification.Provider?.Name;
        Specialism = qualificationInfo.MandatoryQualification.Specialism;
        Status = qualificationInfo.MandatoryQualification.Status;
        StartDate = qualificationInfo.MandatoryQualification.StartDate;
        EndDate = qualificationInfo.MandatoryQualification.EndDate;
        DeletionReason = JourneyInstance!.State.DeletionReason;
        DeletionReasonDetail = JourneyInstance?.State.DeletionReasonDetail;
        EvidenceFileName = JourneyInstance!.State.EvidenceFileName;
        UploadedEvidenceFileUrl = JourneyInstance!.State.EvidenceFileId is not null ?
            await fileService.GetFileUrl(JourneyInstance.State.EvidenceFileId.Value, _fileUrlExpiresAfter) :
            null;

        await next();
    }
}
