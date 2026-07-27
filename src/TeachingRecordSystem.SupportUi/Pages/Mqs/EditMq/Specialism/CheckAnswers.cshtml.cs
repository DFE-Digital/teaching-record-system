using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Optional;
using TeachingRecordSystem.Core.Events.ChangeReasons;
using TeachingRecordSystem.Core.Services.MandatoryQualifications;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Mqs.EditMq.Specialism;

[Journey(JourneyNames.EditMqSpecialism)]
public class CheckAnswersModel(
    EditMqSpecialismJourneyCoordinator journey,
    SupportUiLinkGenerator linkGenerator,
    EvidenceUploadManager evidenceController,
    TimeProvider timeProvider,
    MandatoryQualificationService mandatoryQualificationService) : PageModel
{
    public JourneyInstanceId InstanceId => journey.InstanceId;

    public string? BackLink { get; set; }

    [FromRoute]
    public Guid QualificationId { get; set; }

    [BindProperty]
    public bool Cancel { get; set; }

    public Guid PersonId { get; set; }

    public string? PersonName { get; set; }

    public MandatoryQualificationSpecialism? CurrentSpecialism { get; set; }

    public MandatoryQualificationSpecialism? NewSpecialism { get; set; }

    public MqChangeSpecialismReasonOption ChangeReason { get; set; }

    public string? ChangeReasonDetail { get; set; }

    public string? AdditionalInformation { get; set; }

    public UploadedEvidenceFile? EvidenceFile { get; set; }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        BackLink = journey.GetBackLink();

        var personInfo = context.HttpContext.GetCurrentPersonFeature();

        PersonId = personInfo.PersonId;
        PersonName = personInfo.Name;
        CurrentSpecialism = journey.State.CurrentSpecialism;
        NewSpecialism = journey.State.Specialism;
        ChangeReason = journey.State.ChangeReason!.Value;
        ChangeReasonDetail = journey.State.ChangeReasonDetail;
        AdditionalInformation = journey.State.AdditionalInformation;
        EvidenceFile = journey.State.Evidence.UploadedEvidenceFile;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Cancel)
        {
            return await CancelAsync();
        }

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
                Specialism = Option.Some(NewSpecialism)
            },
            processContext);

        journey.DeleteInstance();
        TempData.SetFlashNotificationBanner("Mandatory qualification changed");

        return Redirect(linkGenerator.Persons.PersonDetail.Qualifications(PersonId));
    }

    private async Task<IActionResult> CancelAsync()
    {
        await evidenceController.DeleteUploadedFileAsync(journey.State.Evidence.UploadedEvidenceFile);
        journey.DeleteInstance();
        return Redirect(linkGenerator.Persons.PersonDetail.Qualifications(PersonId));
    }
}
