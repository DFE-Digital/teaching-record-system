using System.ComponentModel.DataAnnotations;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Services.Files;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.SetStatus;

[Journey(JourneyNames.SetStatus), RequireJourneyInstance]
[AllowDeactivatedPerson]
public class ChangeReasonModel(
    TrsLinkGenerator linkGenerator,
    TrsDbContext dbContext,
    IFileService fileService)
    : CommonJourneyPage(dbContext, linkGenerator, fileService)
{
    [BindProperty]
    [Display(Name = "Why are you deactivating this record?")]
    public DeactivateReasonOption? DeactivateReason { get; set; }

    [BindProperty]
    [Display(Name = "Enter details")]
    [MaxLength(FileUploadDefaults.DetailMaxCharacterCount, ErrorMessage = $"Reason details {FileUploadDefaults.DetailMaxCharacterCountErrorMessage}")]
    public string? DeactivateReasonDetail { get; set; }

    [BindProperty]
    [Display(Name = "Why are you reactivating this record?")]
    public ReactivateReasonOption? ReactivateReason { get; set; }

    [BindProperty]
    [Display(Name = "Enter details")]
    [MaxLength(FileUploadDefaults.DetailMaxCharacterCount, ErrorMessage = $"Reason details {FileUploadDefaults.DetailMaxCharacterCountErrorMessage}")]
    public string? ReactivateReasonDetail { get; set; }

    [BindProperty]
    [Display(Name = "Do you want to upload evidence?")]
    [Required(ErrorMessage = "Select yes if you want to upload evidence")]
    public bool? UploadEvidence { get; set; }

    [BindProperty]
    [EvidenceFile]
    [FileSize(FileUploadDefaults.MaxFileUploadSizeMb * 1024 * 1024, ErrorMessage = $"The selected file {FileUploadDefaults.MaxFileUploadSizeErrorMessage}")]
    public IFormFile? EvidenceFile { get; set; }

    [BindProperty]
    public Guid? EvidenceFileId { get; set; }

    [BindProperty]
    public string? EvidenceFileName { get; set; }

    [BindProperty]
    public string? EvidenceFileSizeDescription { get; set; }

    [BindProperty]
    public string? UploadedEvidenceFileUrl { get; set; }

    public string BackLink => FromCheckAnswers
        ? LinkGenerator.PersonSetStatusCheckAnswers(PersonId, TargetStatus, JourneyInstance!.InstanceId)
        : LinkGenerator.PersonDetail(PersonId);

    public async Task OnGetAsync()
    {
        DeactivateReason = JourneyInstance!.State.DeactivateReason;
        DeactivateReasonDetail = JourneyInstance!.State.DeactivateReasonDetail;
        ReactivateReason = JourneyInstance!.State.ReactivateReason;
        ReactivateReasonDetail = JourneyInstance!.State.ReactivateReasonDetail;
        UploadEvidence = JourneyInstance.State.UploadEvidence;
        EvidenceFileId = JourneyInstance.State.EvidenceFileId;
        EvidenceFileName = JourneyInstance.State.EvidenceFileName;
        EvidenceFileSizeDescription = JourneyInstance.State.EvidenceFileSizeDescription;
        UploadedEvidenceFileUrl = JourneyInstance.State.EvidenceFileId is null ? null :
            await FileService.GetFileUrlAsync(JourneyInstance.State.EvidenceFileId.Value, FileUploadDefaults.FileUrlExpiry);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (TargetStatus == PersonStatus.Deactivated)
        {
            if (DeactivateReason is null)
            {
                ModelState.AddModelError(nameof(DeactivateReason), "Select a reason");
            }

            if (DeactivateReason == DeactivateReasonOption.AnotherReason && DeactivateReasonDetail is null)
            {
                ModelState.AddModelError(nameof(DeactivateReasonDetail), "Enter a reason");
            }
        }
        else
        {
            if (ReactivateReason is null)
            {
                ModelState.AddModelError(nameof(ReactivateReason), "Select a reason");
            }

            if (ReactivateReason == ReactivateReasonOption.AnotherReason && ReactivateReasonDetail is null)
            {
                ModelState.AddModelError(nameof(ReactivateReasonDetail), "Enter a reason");
            }
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

        // Upload the file and set the display fields even if the rest of the form is invalid
        // otherwise the user will have to re-upload every time they re-submit
        if (UploadEvidence == true && EvidenceFile is not null)
        {
            using var stream = EvidenceFile.OpenReadStream();
            var evidenceFileId = await FileService.UploadFileAsync(stream, EvidenceFile.ContentType);
            EvidenceFileId = evidenceFileId;
            EvidenceFileName = EvidenceFile?.FileName;
            EvidenceFileSizeDescription = EvidenceFile?.Length.Bytes().Humanize();
            UploadedEvidenceFileUrl = await FileService.GetFileUrlAsync(evidenceFileId, FileUploadDefaults.FileUrlExpiry);
        }

        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        await JourneyInstance!.UpdateStateAsync(state =>
        {
            state.DeactivateReason = DeactivateReason;
            state.DeactivateReasonDetail = DeactivateReason is DeactivateReasonOption.AnotherReason ? DeactivateReasonDetail : null;
            state.ReactivateReason = ReactivateReason;
            state.ReactivateReasonDetail = ReactivateReason is ReactivateReasonOption.AnotherReason ? ReactivateReasonDetail : null;
            state.UploadEvidence = UploadEvidence;
            state.EvidenceFileId = UploadEvidence is true ? EvidenceFileId : null;
            state.EvidenceFileName = UploadEvidence is true ? EvidenceFileName : null;
            state.EvidenceFileSizeDescription = UploadEvidence is true ? EvidenceFileSizeDescription : null;
        });

        return Redirect(LinkGenerator.PersonSetStatusCheckAnswers(PersonId, TargetStatus, JourneyInstance!.InstanceId));
    }
}
