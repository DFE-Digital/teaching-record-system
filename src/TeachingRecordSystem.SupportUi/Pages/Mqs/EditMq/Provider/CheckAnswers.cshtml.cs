using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Optional;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Events.ChangeReasons;
using TeachingRecordSystem.Core.Services.MandatoryQualifications;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Mqs.EditMq.Provider;

[Journey(JourneyNames.EditMqProvider)]
public class CheckAnswersModel(
    EditMqProviderJourneyCoordinator journey,
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

    public string? CurrentProviderName { get; set; }

    public Guid ProviderId { get; set; }

    public string? ProviderName { get; set; }

    public MqChangeProviderReasonOption ChangeReason { get; set; }

    public string? ChangeReasonDetail { get; set; }

    public UploadedEvidenceFile? EvidenceFile { get; set; }

    public string? AdditionalInformation { get; set; }

    public void OnGet() { }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        BackLink = journey.GetBackLink();

        var qualificationInfo = context.HttpContext.GetCurrentMandatoryQualificationFeature();
        var personInfo = context.HttpContext.GetCurrentPersonFeature();

        PersonId = personInfo.PersonId;
        PersonName = personInfo.Name;
        CurrentProviderName = qualificationInfo.MandatoryQualification.Provider?.Name;
        ProviderId = journey.State.ProviderId!.Value;
        ProviderName = MandatoryQualificationProvider.GetById(ProviderId).Name;
        ChangeReason = journey.State.ChangeReason!.Value;
        ChangeReasonDetail = journey.State.ChangeReasonDetail;
        EvidenceFile = journey.State.Evidence.UploadedEvidenceFile;
        AdditionalInformation = journey.State.AdditionalInformation;
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
                ProviderId = Option.Some<Guid?>(ProviderId)
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
