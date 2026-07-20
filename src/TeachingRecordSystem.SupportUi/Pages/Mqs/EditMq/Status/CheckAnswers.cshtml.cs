using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Optional;
using TeachingRecordSystem.Core.Events.ChangeReasons;
using TeachingRecordSystem.Core.Services.MandatoryQualifications;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Mqs.EditMq.Status;

[TeachingRecordSystem.WebCommon.FormFlow.Journey(JourneyNames.EditMqStatus), RequireJourneyInstance]
public class CheckAnswersModel(
    SupportUiLinkGenerator linkGenerator,
    EvidenceUploadManager evidenceUploadManager,
    TimeProvider timeProvider,
    MandatoryQualificationService mandatoryQualificationService) : PageModel
{
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

    public UploadedEvidenceFile? EvidenceFile { get; set; }

    public bool IsEndDateChange { get; set; }

    public bool IsStatusChange { get; set; }

    public string? AdditionalInformation { get; set; }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        if (!JourneyInstance!.State.IsComplete)
        {
            context.Result = Redirect(linkGenerator.Mqs.EditMq.Status.Index(QualificationId, JourneyInstance.InstanceId));
            return;
        }

        var personInfo = context.HttpContext.GetCurrentPersonFeature();

        PersonId = personInfo.PersonId;
        PersonName = personInfo.Name;
        CurrentStatus = JourneyInstance.State.CurrentStatus;
        NewStatus ??= JourneyInstance.State.Status;
        CurrentEndDate = JourneyInstance.State.CurrentEndDate;
        NewEndDate ??= JourneyInstance.State.EndDate;
        StatusChangeReason = JourneyInstance.State.StatusChangeReason;
        EndDateChangeReason = JourneyInstance.State.EndDateChangeReason;
        ChangeReasonDetail = JourneyInstance.State.ChangeReasonDetail;
        EvidenceFile = JourneyInstance.State.Evidence.UploadedEvidenceFile;
        IsEndDateChange = JourneyInstance.State.IsEndDateChange;
        IsStatusChange = JourneyInstance.State.IsStatusChange;
        AdditionalInformation = JourneyInstance.State.AdditionalInformation;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var qualification = HttpContext.GetCurrentMandatoryQualificationFeature().MandatoryQualification;

        var processContext = new ProcessContext(
            ProcessType.MandatoryQualificationUpdating,
            timeProvider.UtcNow,
            User.GetUserId(),
            new ChangeReasonWithDetailsAndEvidence
            {
                Reason = IsEndDateChange && !IsStatusChange ? EndDateChangeReason!.GetDisplayName() : StatusChangeReason!.GetDisplayName(),
                Details = ChangeReasonDetail,
                EvidenceFile = EvidenceFile?.ToEventModel(),
                AdditionalInformation = AdditionalInformation
            });

        await mandatoryQualificationService.UpdateMandatoryQualificationAsync(
            new UpdateMandatoryQualificationOptions
            {
                QualificationId = qualification.QualificationId,
                Status = Option.Some(NewStatus),
                EndDate = Option.Some(NewEndDate)
            },
            processContext);

        await JourneyInstance!.CompleteAsync();
        TempData.SetFlashNotificationBanner("Mandatory qualification changed");

        return Redirect(linkGenerator.Persons.PersonDetail.Qualifications(PersonId));
    }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await evidenceUploadManager.DeleteUploadedFileAsync(JourneyInstance!.State.Evidence.UploadedEvidenceFile);
        await JourneyInstance!.DeleteAsync();
        return Redirect(linkGenerator.Persons.PersonDetail.Qualifications(PersonId));
    }
}
