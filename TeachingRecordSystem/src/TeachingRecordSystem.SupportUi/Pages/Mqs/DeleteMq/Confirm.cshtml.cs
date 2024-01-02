using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Dqt.Queries;
using TeachingRecordSystem.Core.Events;
using TeachingRecordSystem.Core.Jobs.Scheduling;
using TeachingRecordSystem.Core.Services.Files;
using TeachingRecordSystem.Core.Services.TrsDataSync;

namespace TeachingRecordSystem.SupportUi.Pages.Mqs.DeleteMq;

[Journey(JourneyNames.DeleteMq), RequireJourneyInstance]
public class ConfirmModel(
    ICrmQueryDispatcher crmQueryDispatcher,
    TrsLinkGenerator linkGenerator,
    IFileService fileService,
    ReferenceDataCache referenceDataCache,
    IClock clock,
    IBackgroundJobScheduler backgroundJobScheduler) : PageModel
{
    private static readonly TimeSpan _fileUrlExpiresAfter = TimeSpan.FromMinutes(15);

    public JourneyInstance<DeleteMqState>? JourneyInstance { get; set; }

    [FromRoute]
    public Guid QualificationId { get; set; }

    public Guid? PersonId { get; set; }

    public string? PersonName { get; set; }

    public string? TrainingProvider { get; set; }

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
        var qualification = (await crmQueryDispatcher.ExecuteQuery(new GetQualificationByIdQuery(QualificationId)))!;

        var establishment = qualification.dfeta_MQ_MQEstablishmentId?.Id is Guid establishmentId ?
            await referenceDataCache.GetMqEstablishmentById(establishmentId) :
            null;

        MandatoryQualificationProvider.TryMapFromDqtMqEstablishment(establishment, out var provider);

        var specialism = qualification.dfeta_MQ_SpecialismId?.Id is Guid specialismId ?
            await referenceDataCache.GetMqSpecialismById(specialismId) :
            null;

        var deletedEvent = new MandatoryQualificationDeletedEvent()
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = clock.UtcNow,
            RaisedBy = User.GetUserId(),
            PersonId = PersonId!.Value,
            MandatoryQualification = new()
            {
                QualificationId = QualificationId,
                Provider = provider is not null || establishment is not null ?
                    new()
                    {
                        MandatoryQualificationProviderId = provider?.MandatoryQualificationProviderId,
                        Name = provider?.Name,
                        DqtMqEstablishmentId = establishment?.Id,
                        DqtMqEstablishmentName = establishment?.dfeta_name
                    } :
                    null,
                Specialism = specialism?.ToMandatoryQualificationSpecialism(),
                Status = qualification.dfeta_MQ_Status?.ToMandatoryQualificationStatus(),
                StartDate = qualification.dfeta_MQStartDate?.ToDateOnlyWithDqtBstFix(isLocalTime: true),
                EndDate = qualification.dfeta_MQ_Date?.ToDateOnlyWithDqtBstFix(isLocalTime: true),
            },
            DeletionReason = DeletionReason!.GetDisplayName(),
            DeletionReasonDetail = DeletionReasonDetail,
            EvidenceFile = JourneyInstance!.State.EvidenceFileId is Guid fileId ?
                new Core.Events.Models.File()
                {
                    FileId = fileId,
                    Name = JourneyInstance.State.EvidenceFileName!
                } :
                null
        };

        await crmQueryDispatcher.ExecuteQuery(
            new DeleteQualificationQuery(
                QualificationId,
                EventInfo.Create(deletedEvent).Serialize()));

        await backgroundJobScheduler.Enqueue<TrsDataSyncHelper>(helper => helper.SyncMandatoryQualification(QualificationId, CancellationToken.None));

        await JourneyInstance!.CompleteAsync();
        TempData.SetFlashSuccess("Mandatory qualification deleted");

        return Redirect(linkGenerator.PersonQualifications(PersonId!.Value));
    }

    public async Task<IActionResult> OnPostCancel()
    {
        if (JourneyInstance!.State.EvidenceFileId is not null)
        {
            await fileService.DeleteFile(JourneyInstance!.State.EvidenceFileId.Value);
        }

        await JourneyInstance!.DeleteAsync();
        return Redirect(linkGenerator.PersonQualifications(PersonId!.Value));
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        if (!JourneyInstance!.State.IsComplete)
        {
            context.Result = Redirect(linkGenerator.MqDelete(QualificationId, JourneyInstance.InstanceId));
            return;
        }

        PersonId = JourneyInstance!.State.PersonId;
        PersonName = JourneyInstance!.State.PersonName;
        TrainingProvider = JourneyInstance!.State.ProviderName;
        Specialism = JourneyInstance!.State.Specialism;
        Status = JourneyInstance!.State.Status;
        StartDate = JourneyInstance!.State.StartDate;
        EndDate = JourneyInstance!.State.EndDate;
        DeletionReason = JourneyInstance!.State.DeletionReason;
        DeletionReasonDetail = JourneyInstance?.State.DeletionReasonDetail;
        EvidenceFileName = JourneyInstance!.State.EvidenceFileName;
        UploadedEvidenceFileUrl = JourneyInstance!.State.EvidenceFileId is not null ? await fileService.GetFileUrl(JourneyInstance.State.EvidenceFileId.Value, _fileUrlExpiresAfter) : null;

        await next();
    }
}
