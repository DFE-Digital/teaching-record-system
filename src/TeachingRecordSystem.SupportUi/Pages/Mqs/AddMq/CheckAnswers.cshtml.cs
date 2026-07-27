using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Events.ChangeReasons;
using TeachingRecordSystem.Core.Services.MandatoryQualifications;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Mqs.AddMq;

[Journey(JourneyNames.AddMq)]
public class CheckAnswersModel(
    AddMqJourneyCoordinator journey,
    SupportUiLinkGenerator linkGenerator,
    TimeProvider timeProvider,
    EvidenceUploadManager evidenceUploadManager,
    MandatoryQualificationService mandatoryQualificationService) : PageModel
{
    public JourneyInstanceId InstanceId => journey.InstanceId;

    public string? BackLink { get; set; }

    [FromQuery]
    public Guid PersonId { get; set; }

    [BindProperty]
    public bool Cancel { get; set; }

    public string? PersonName { get; set; }

    public Guid ProviderId { get; set; }

    public string? ProviderName { get; set; }

    public MandatoryQualificationSpecialism Specialism { get; set; }

    public DateOnly StartDate { get; set; }

    public MandatoryQualificationStatus Status { get; set; }

    public DateOnly? EndDate { get; set; }

    public AddMqReasonOption AddReason { get; set; }

    public string? AddReasonDetail { get; set; }

    public string? AdditionalInformation { get; set; }

    [BindProperty]
    public UploadedEvidenceFile? EvidenceFile { get; set; }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Cancel)
        {
            return await CancelAsync();
        }

        var processContext = new ProcessContext(
            ProcessType.MandatoryQualificationCreating,
            timeProvider.UtcNow,
            User.GetUserId(),
            new ChangeReasonWithDetailsAndEvidence
            {
                Reason = AddReason.GetDisplayName(),
                Details = AddReasonDetail,
                EvidenceFile = EvidenceFile?.ToEventModel(),
                AdditionalInformation = AdditionalInformation
            });

        await mandatoryQualificationService.CreateMandatoryQualificationAsync(
            new CreateMandatoryQualificationOptions
            {
                PersonId = PersonId,
                ProviderId = ProviderId,
                Specialism = Specialism,
                Status = Status,
                StartDate = StartDate,
                EndDate = EndDate
            },
            processContext);

        journey.DeleteInstance();
        TempData.SetFlashNotificationBanner("Mandatory qualification added");

        return Redirect(linkGenerator.Persons.PersonDetail.Qualifications(PersonId));
    }

    private async Task<IActionResult> CancelAsync()
    {
        await evidenceUploadManager.DeleteUploadedFileAsync(journey.State.Evidence.UploadedEvidenceFile);
        journey.DeleteInstance();
        return Redirect(linkGenerator.Persons.PersonDetail.Index(PersonId));
    }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        BackLink = journey.GetBackLink();

        var personInfo = context.HttpContext.GetCurrentPersonFeature();

        PersonName = personInfo.Name;
        ProviderId = journey.State.ProviderId!.Value;
        ProviderName = MandatoryQualificationProvider.GetById(ProviderId).Name;
        Specialism = journey.State.Specialism!.Value;
        StartDate = journey.State.StartDate!.Value;
        Status = journey.State.Status!.Value;
        EndDate = journey.State.EndDate;
        AddReason = journey.State.AddReason!.Value;
        AddReasonDetail = journey.State.AddReasonDetail;
        AdditionalInformation = journey.State.AdditionalInformation;
        EvidenceFile = journey.State.Evidence.UploadedEvidenceFile;
    }
}
