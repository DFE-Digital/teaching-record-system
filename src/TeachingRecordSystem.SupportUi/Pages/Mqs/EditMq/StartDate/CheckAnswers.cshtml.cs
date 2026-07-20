using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Optional;
using TeachingRecordSystem.Core.Events.ChangeReasons;
using TeachingRecordSystem.Core.Services.MandatoryQualifications;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Mqs.EditMq.StartDate;

[TeachingRecordSystem.WebCommon.FormFlow.Journey(JourneyNames.EditMqStartDate), RequireJourneyInstance]
public class CheckAnswersModel(
    SupportUiLinkGenerator linkGenerator,
    EvidenceUploadManager evidenceController,
    TimeProvider timeProvider,
    MandatoryQualificationService mandatoryQualificationService) : PageModel
{
    public JourneyInstance<EditMqStartDateState>? JourneyInstance { get; set; }

    [FromRoute]
    public Guid QualificationId { get; set; }

    public Guid PersonId { get; set; }

    public string? PersonName { get; set; }

    public DateOnly? CurrentStartDate { get; set; }

    public DateOnly? NewStartDate { get; set; }

    public MqChangeStartDateReasonOption ChangeReason { get; set; }

    public string? ChangeReasonDetail { get; set; }

    public UploadedEvidenceFile? EvidenceFile { get; set; }

    public string? AdditionalInformation { get; set; }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        if (!JourneyInstance!.State.IsComplete)
        {
            context.Result = Redirect(linkGenerator.Mqs.EditMq.StartDate.Index(QualificationId, JourneyInstance.InstanceId));
            return;
        }

        var personInfo = context.HttpContext.GetCurrentPersonFeature();

        PersonId = personInfo.PersonId;
        PersonName = personInfo.Name;
        CurrentStartDate = JourneyInstance.State.CurrentStartDate;
        NewStartDate ??= JourneyInstance.State.StartDate;
        ChangeReason = JourneyInstance.State.ChangeReason!.Value;
        ChangeReasonDetail = JourneyInstance.State.ChangeReasonDetail;
        EvidenceFile = JourneyInstance.State.Evidence.UploadedEvidenceFile;
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
                Reason = ChangeReason.GetDisplayName(),
                Details = ChangeReasonDetail,
                EvidenceFile = EvidenceFile?.ToEventModel(),
                AdditionalInformation = AdditionalInformation
            });

        await mandatoryQualificationService.UpdateMandatoryQualificationAsync(
            new UpdateMandatoryQualificationOptions
            {
                QualificationId = qualification.QualificationId,
                StartDate = Option.Some(NewStartDate)
            },
            processContext);

        await JourneyInstance!.CompleteAsync();
        TempData.SetFlashNotificationBanner("Mandatory qualification changed");

        return Redirect(linkGenerator.Persons.PersonDetail.Qualifications(PersonId));
    }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await evidenceController.DeleteUploadedFileAsync(JourneyInstance!.State.Evidence.UploadedEvidenceFile);
        await JourneyInstance!.DeleteAsync();
        return Redirect(linkGenerator.Persons.PersonDetail.Qualifications(PersonId));
    }
}
