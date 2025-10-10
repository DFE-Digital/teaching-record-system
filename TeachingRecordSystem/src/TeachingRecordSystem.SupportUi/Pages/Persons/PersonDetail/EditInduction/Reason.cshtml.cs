using System.ComponentModel.DataAnnotations;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Services.Files;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditInduction;

[Journey(JourneyNames.EditInduction), RequireJourneyInstance]
public class ReasonModel(
    TrsLinkGenerator linkGenerator,
    TrsDbContext dbContext,
    IFileService fileService)
    : CommonJourneyPage(dbContext, linkGenerator, fileService)
{
    [BindProperty]
    [Required(ErrorMessage = "Select a reason")]
    [Display(Name = "Why are you changing the induction details?")]
    public InductionChangeReasonOption? ChangeReason { get; set; }

    [BindProperty]
    [Display(Name = "Do you want to add more information about why you’re changing the induction details?")]
    [Required(ErrorMessage = "Select yes if you want to add more information about why you’re changing the induction details")]
    public bool? HasAdditionalReasonDetail { get; set; }

    [BindProperty]
    [Display(Name = "Add additional detail")]
    [MaxLength(UiDefaults.DetailMaxCharacterCount, ErrorMessage = $"Additional detail {UiDefaults.DetailMaxCharacterCountErrorMessage}")]
    public string? ChangeReasonDetail { get; set; }

    [BindProperty]
    [Display(Name = "Do you want to upload evidence?")]
    [Required(ErrorMessage = "Select yes if you want to upload evidence")]
    public bool? UploadEvidence { get; set; }

    [BindProperty]
    [EvidenceFile]
    [FileSize(UiDefaults.MaxFileUploadSizeMb * 1024 * 1024, ErrorMessage = $"The selected file {UiDefaults.MaxFileUploadSizeErrorMessage}")]
    public IFormFile? EvidenceFile { get; set; }

    [BindProperty]
    public Guid? EvidenceFileId { get; set; }

    [BindProperty]
    public string? EvidenceFileName { get; set; }

    [BindProperty]
    public string? EvidenceFileSizeDescription { get; set; }

    [BindProperty]
    public string? UploadedEvidenceFileUrl { get; set; }

    protected InductionStatus InductionStatus => JourneyInstance!.State.InductionStatus;

    public InductionJourneyPage NextPage => InductionJourneyPage.CheckAnswers;

    public string BackLink
    {
        get
        {
            if (FromCheckAnswers == JourneyFromCheckYourAnswersPage.CheckYourAnswers)
            {
                return GetPageLink(InductionJourneyPage.CheckAnswers);
            }
            return InductionStatus switch
            {
                _ when InductionStatus.RequiresCompletedDate() => GetPageLink(InductionJourneyPage.CompletedDate),
                _ when InductionStatus.RequiresStartDate() => GetPageLink(InductionJourneyPage.StartDate),
                _ when InductionStatus.RequiresExemptionReasons() => GetPageLink(InductionJourneyPage.ExemptionReason),
                _ => GetPageLink(InductionJourneyPage.Status)
            };
        }
    }

    public async Task OnGetAsync()
    {
        ChangeReason = JourneyInstance!.State.ChangeReason;
        HasAdditionalReasonDetail = JourneyInstance!.State.HasAdditionalReasonDetail;
        ChangeReasonDetail = JourneyInstance?.State.ChangeReasonDetail;
        UploadEvidence = JourneyInstance?.State.UploadEvidence;
        EvidenceFileId = JourneyInstance?.State.EvidenceFileId;
        EvidenceFileName = JourneyInstance?.State.EvidenceFileName;
        EvidenceFileSizeDescription = JourneyInstance?.State.EvidenceFileSizeDescription;
        UploadedEvidenceFileUrl = JourneyInstance?.State.EvidenceFileId is not null ?
            await FileService.GetFileUrlAsync(JourneyInstance.State.EvidenceFileId.Value, UiDefaults.FileUrlExpiry) :
            null;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (HasAdditionalReasonDetail == true && ChangeReasonDetail is null)
        {
            ModelState.AddModelError(nameof(ChangeReasonDetail), "Enter additional detail");
        }

        if (UploadEvidence == true && EvidenceFileId is null && EvidenceFile is null)
        {
            ModelState.AddModelError(nameof(EvidenceFile), "Select a file");
        }

        // Delete any previously uploaded file if they're uploading a new one,
        // or choosing not to upload evidence (check for UploadEvidence != true because if
        // UploadEvidence somehow got set to null we still want to delete the file)
        if (EvidenceFileId.HasValue && (EvidenceFile is not null || UploadEvidence != true))
        {
            await FileService.DeleteFileAsync(EvidenceFileId.Value);
            EvidenceFileName = null;
            EvidenceFileSizeDescription = null;
            UploadedEvidenceFileUrl = null;
            EvidenceFileId = null;
        }

        // Upload the file even if the rest of the form is invalid
        // otherwise the user will have to re-upload every time they re-submit
        if (UploadEvidence == true)
        {
            // Upload the file and set the display fields
            if (EvidenceFile is not null)
            {
                using var stream = EvidenceFile.OpenReadStream();
                var evidenceFileId = await FileService.UploadFileAsync(stream, EvidenceFile.ContentType);
                EvidenceFileId = evidenceFileId;
                EvidenceFileName = EvidenceFile?.FileName;
                EvidenceFileSizeDescription = EvidenceFile?.Length.Bytes().Humanize();
                UploadedEvidenceFileUrl = await FileService.GetFileUrlAsync(evidenceFileId, UiDefaults.FileUrlExpiry);
            }
        }

        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        await JourneyInstance!.UpdateStateAsync(state =>
        {
            state.ChangeReason = ChangeReason;
            state.HasAdditionalReasonDetail = HasAdditionalReasonDetail;
            state.ChangeReasonDetail = HasAdditionalReasonDetail is true ? ChangeReasonDetail : null;
            state.UploadEvidence = UploadEvidence;
            state.EvidenceFileId = UploadEvidence is true ? EvidenceFileId : null;
            state.EvidenceFileName = UploadEvidence is true ? EvidenceFileName : null;
            state.EvidenceFileSizeDescription = UploadEvidence is true ? EvidenceFileSizeDescription : null;

        });

        return Redirect(GetPageLink(NextPage));
    }
}
