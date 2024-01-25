using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Dqt.Queries;
using TeachingRecordSystem.Core.Events;
using TeachingRecordSystem.Core.Jobs.Scheduling;
using TeachingRecordSystem.Core.Services.Files;
using TeachingRecordSystem.Core.Services.TrsDataSync;

namespace TeachingRecordSystem.SupportUi.Pages.Mqs.EditMq.Status;

[Journey(JourneyNames.EditMqStatus), RequireJourneyInstance]
public class ConfirmModel(
    ICrmQueryDispatcher crmQueryDispatcher,
    ReferenceDataCache referenceDataCache,
    TrsLinkGenerator linkGenerator,
    IFileService fileService,
    IBackgroundJobScheduler backgroundJobScheduler,
    IClock clock) : PageModel
{
    private static readonly TimeSpan _fileUrlExpiresAfter = TimeSpan.FromMinutes(15);

    public JourneyInstance<EditMqStatusState>? JourneyInstance { get; set; }

    [FromRoute]
    public Guid QualificationId { get; set; }

    public Guid? PersonId { get; set; }

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
        var qualification = (await crmQueryDispatcher.ExecuteQuery(new GetQualificationByIdQuery(QualificationId)))!;

        var dqtMqStatus = qualification.dfeta_MQ_Status ?? (qualification.dfeta_MQ_Date.HasValue ? dfeta_qualification_dfeta_MQ_Status.Passed : null);
        var changes = MandatoryQualificationUpdatedEventChanges.None |
            (NewStatus != dqtMqStatus?.ToMandatoryQualificationStatus() ? MandatoryQualificationUpdatedEventChanges.Status : 0) |
            (NewEndDate != qualification.dfeta_MQ_Date?.ToDateOnlyWithDqtBstFix(isLocalTime: true) ? MandatoryQualificationUpdatedEventChanges.EndDate : 0);

        if (changes != MandatoryQualificationUpdatedEventChanges.None)
        {
            var establishment = qualification.dfeta_MQ_MQEstablishmentId?.Id is Guid establishmentId ?
                await referenceDataCache.GetMqEstablishmentById(establishmentId) :
                null;

            var specialism = qualification.dfeta_MQ_SpecialismId?.Id is Guid specialismId ?
                await referenceDataCache.GetMqSpecialismById(specialismId) :
                null;

            var oldMqEventModel = new Core.Events.Models.MandatoryQualification()
            {
                QualificationId = QualificationId,
                Provider = establishment is not null ?
                    new()
                    {
                        MandatoryQualificationProviderId = null,
                        Name = null,
                        DqtMqEstablishmentId = establishment?.Id,
                        DqtMqEstablishmentName = establishment?.dfeta_name
                    } :
                    null,
                Specialism = specialism?.ToMandatoryQualificationSpecialism(),
                Status = dqtMqStatus?.ToMandatoryQualificationStatus(),
                StartDate = qualification.dfeta_MQStartDate?.ToDateOnlyWithDqtBstFix(isLocalTime: true),
                EndDate = qualification.dfeta_MQ_Date?.ToDateOnlyWithDqtBstFix(isLocalTime: true),
            };

            var updatedEvent = new MandatoryQualificationUpdatedEvent()
            {
                EventId = Guid.NewGuid(),
                CreatedUtc = clock.UtcNow,
                RaisedBy = User.GetUserId(),
                PersonId = PersonId!.Value,
                MandatoryQualification = oldMqEventModel with
                {
                    Status = NewStatus,
                    EndDate = NewEndDate
                },
                OldMandatoryQualification = oldMqEventModel,
                ChangeReason = (IsEndDateChange!.Value && !IsStatusChange!.Value) ? EndDateChangeReason!.GetDisplayName() : StatusChangeReason!.GetDisplayName(),
                ChangeReasonDetail = ChangeReasonDetail,
                EvidenceFile = JourneyInstance!.State.EvidenceFileId is Guid fileId ?
                    new Core.Events.Models.File()
                    {
                        FileId = fileId,
                        Name = JourneyInstance.State.EvidenceFileName!
                    } :
                    null,
                Changes = changes
            };

            await crmQueryDispatcher.ExecuteQuery(
                new UpdateMandatoryQualificationStatusQuery(
                    QualificationId,
                    NewStatus!.Value.GetDqtStatus(),
                    NewEndDate,
                    updatedEvent));

            await backgroundJobScheduler.Enqueue<TrsDataSyncHelper>(
                helper => helper.SyncMandatoryQualification(QualificationId, new[] { updatedEvent }, CancellationToken.None));
        }

        await JourneyInstance!.CompleteAsync();
        TempData.SetFlashSuccess("Mandatory qualification changed");

        return Redirect(linkGenerator.PersonQualifications(PersonId!.Value));
    }

    public async Task<IActionResult> OnPostCancel()
    {
        await JourneyInstance!.DeleteAsync();
        return Redirect(linkGenerator.PersonQualifications(PersonId!.Value));
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
