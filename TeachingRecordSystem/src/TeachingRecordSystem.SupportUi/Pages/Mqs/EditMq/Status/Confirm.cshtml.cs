using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Services.Files;

namespace TeachingRecordSystem.SupportUi.Pages.Mqs.EditMq.Status;

[Journey(JourneyNames.EditMqStatus), RequireJourneyInstance]
public class ConfirmModel(
    TrsDbContext dbContext,
    TrsLinkGenerator linkGenerator,
    IFileService fileService,
    IClock clock) : PageModel
{
    private static readonly TimeSpan _fileUrlExpiresAfter = TimeSpan.FromMinutes(15);

    public JourneyInstance<EditMqStatusState>? JourneyInstance { get; set; }

    [FromRoute]
    public Guid QualificationId { get; set; }

    public Guid PersonId { get; set; }

    public string? PersonName { get; set; }

    public MandatoryQualificationStatus? CurrentStatus { get; set; }

    public MandatoryQualificationStatus? NewStatus { get; set; }

    public DateOnly? CurrentEndDate { get; set; }

    public DateOnly? NewEndDate { get; set; }

    public MqChangeStatusReasonOption? StatusChangeReason { get; set; }

    public MqChangeEndDateReasonOption? EndDateChangeReason { get; set; }

    public string? ChangeReasonDetail { get; set; }

    public string? EvidenceFileName { get; set; }

    public string? EvidenceFileSizeDescription { get; set; }

    public string? UploadedEvidenceFileUrl { get; set; }

    public bool? IsEndDateChange { get; set; }

    public bool? IsStatusChange { get; set; }

    public async Task<IActionResult> OnPost()
    {
        var now = clock.UtcNow;

        var qualification = await dbContext.MandatoryQualifications
            .Include(q => q.Provider)
            .SingleAsync(q => q.QualificationId == QualificationId);

        var changes = MandatoryQualificationUpdatedEventChanges.None |
            (NewStatus != qualification.Status ? MandatoryQualificationUpdatedEventChanges.Status : 0) |
            (NewEndDate != qualification.EndDate ? MandatoryQualificationUpdatedEventChanges.EndDate : 0);

        if (changes != MandatoryQualificationUpdatedEventChanges.None)
        {
            var oldMqEventModel = EventModels.MandatoryQualification.FromModel(qualification);

            qualification.Status = NewStatus;
            qualification.EndDate = NewEndDate;
            qualification.UpdatedOn = now;

            var updatedEvent = new MandatoryQualificationUpdatedEvent()
            {
                EventId = Guid.NewGuid(),
                CreatedUtc = now,
                RaisedBy = User.GetUserId(),
                PersonId = PersonId,
                MandatoryQualification = EventModels.MandatoryQualification.FromModel(qualification),
                OldMandatoryQualification = oldMqEventModel,
                ChangeReason = (IsEndDateChange!.Value && !IsStatusChange!.Value) ? EndDateChangeReason!.GetDisplayName() : StatusChangeReason!.GetDisplayName(),
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
        TempData.SetFlashSuccess("Mandatory qualification changed");

        return Redirect(linkGenerator.PersonQualifications(PersonId));
    }

    public async Task<IActionResult> OnPostCancel()
    {
        await JourneyInstance!.DeleteAsync();
        return Redirect(linkGenerator.PersonQualifications(PersonId));
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        if (!JourneyInstance!.State.IsComplete)
        {
            context.Result = Redirect(linkGenerator.MqEditStatus(QualificationId, JourneyInstance.InstanceId));
            return;
        }

        var personInfo = context.HttpContext.GetCurrentPersonFeature();

        PersonId = personInfo.PersonId;
        PersonName = personInfo.Name;
        CurrentStatus = JourneyInstance!.State.CurrentStatus;
        NewStatus ??= JourneyInstance!.State.Status;
        CurrentEndDate = JourneyInstance!.State.CurrentEndDate;
        NewEndDate ??= JourneyInstance!.State.EndDate;
        StatusChangeReason = JourneyInstance!.State.StatusChangeReason;
        EndDateChangeReason = JourneyInstance!.State.EndDateChangeReason;
        ChangeReasonDetail = JourneyInstance?.State.ChangeReasonDetail;
        EvidenceFileName = JourneyInstance!.State.EvidenceFileName;
        UploadedEvidenceFileUrl ??= JourneyInstance?.State.EvidenceFileId is not null ?
            await fileService.GetFileUrl(JourneyInstance.State.EvidenceFileId.Value, _fileUrlExpiresAfter) :
            null;
        IsEndDateChange = JourneyInstance!.State.IsEndDateChange;
        IsStatusChange = JourneyInstance!.State.IsStatusChange;

        await next();
    }
}
