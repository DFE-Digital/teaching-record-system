using System.ComponentModel.DataAnnotations;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Core.Services.Files;
using TeachingRecordSystem.SupportUi.Infrastructure.DataAnnotations;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.AddRoute;

[Journey(JourneyNames.AddRouteToProfessionalStatus), RequireJourneyInstance]
public class ChangeReasonModel(TrsLinkGenerator linkGenerator, ReferenceDataCache referenceDataCache, IFileService fileService)
    : AddRoutePostStatusPageModel(AddRoutePage.ChangeReason, linkGenerator, referenceDataCache)
{
    [BindProperty]
    [Required(ErrorMessage = "Select a reason")]
    [Display(Name = "Why are you adding this route?")]
    public ChangeReasonOption? ChangeReason { get; set; }

    [BindProperty]
    [Display(Name = "Do you want to provide more information?")]
    [Required(ErrorMessage = "Select yes if you want to add more information about why you\u2019re adding this route")]
    public bool? HasAdditionalReasonDetail { get; set; }

    [BindProperty]
    [Display(Name = "Add more information")]
    [MaxLength(FileUploadDefaults.DetailMaxCharacterCount, ErrorMessage = $"Additional detail {FileUploadDefaults.DetailMaxCharacterCountErrorMessage}")]
    public string? ChangeReasonDetail { get; set; }

    [BindProperty]
    [Display(Name = "Do you want to upload evidence?")]
    [Required(ErrorMessage = "Select yes if you want to upload evidence")]
    public bool? UploadEvidence { get; set; }

    [BindProperty]
    [EvidenceFile]
    [FileSize(FileUploadDefaults.MaxFileUploadSizeMb * 1024 * 1024, ErrorMessage = $"The selected file {FileUploadDefaults.MaxFileUploadSizeErrorMessage}")]
    public IFormFile? EvidenceFile { get; set; }

    public Guid? EvidenceFileId { get; set; }

    public string? EvidenceFileName { get; set; }

    public string? EvidenceFileSizeDescription { get; set; }

    public string? UploadedEvidenceFileUrl { get; set; }

    public async Task OnGetAsync()
    {
        ChangeReason = JourneyInstance!.State.ChangeReason;
        HasAdditionalReasonDetail = JourneyInstance!.State.ChangeReasonDetail.HasAdditionalReasonDetail;
        ChangeReasonDetail = JourneyInstance?.State.ChangeReasonDetail.ChangeReasonDetail;
        UploadEvidence = JourneyInstance?.State.ChangeReasonDetail.UploadEvidence;
        UploadedEvidenceFileUrl = JourneyInstance?.State.ChangeReasonDetail.EvidenceFileId is not null ?
            await fileService.GetFileUrlAsync(JourneyInstance.State.ChangeReasonDetail.EvidenceFileId.Value, FileUploadDefaults.FileUrlExpiry) :
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
            await fileService.DeleteFileAsync(EvidenceFileId.Value);
            EvidenceFileName = null;
            EvidenceFileSizeDescription = null;
            UploadedEvidenceFileUrl = null;
            EvidenceFileId = null;
        }
        // Upload the file even if the rest of the form is invalid
        // otherwise the user will have to re-upload every time they re-submit
        if (UploadEvidence == true)
        {
            if (EvidenceFile is not null)
            {
                using var stream = EvidenceFile.OpenReadStream();
                var evidenceFileId = await fileService.UploadFileAsync(stream, EvidenceFile.ContentType);
                EvidenceFileId = evidenceFileId;
                EvidenceFileName = EvidenceFile?.FileName;
                EvidenceFileSizeDescription = EvidenceFile?.Length.Bytes().Humanize();
                UploadedEvidenceFileUrl = await fileService.GetFileUrlAsync(evidenceFileId, FileUploadDefaults.FileUrlExpiry);
            }
        }

        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        await JourneyInstance!.UpdateStateAsync(state =>
        {
            state.ChangeReason = ChangeReason;
            state.ChangeReasonDetail.HasAdditionalReasonDetail = HasAdditionalReasonDetail;
            state.ChangeReasonDetail.ChangeReasonDetail = ChangeReasonDetail;
            state.ChangeReasonDetail.HasAdditionalReasonDetail = HasAdditionalReasonDetail;
            state.ChangeReasonDetail.UploadEvidence = UploadEvidence;
            state.ChangeReasonDetail.EvidenceFileId = UploadEvidence is true ? EvidenceFileId : null;
            state.ChangeReasonDetail.EvidenceFileName = UploadEvidence is true ? EvidenceFileName : null;
            state.ChangeReasonDetail.EvidenceFileSizeDescription = UploadEvidence is true ? EvidenceFileSizeDescription : null;
        });

        return await ContinueAsync();
    }

    public override async Task OnPageHandlerExecutingAsync(PageHandlerExecutingContext context)
    {
        await base.OnPageHandlerExecutingAsync(context);

        EvidenceFileId = JourneyInstance!.State.ChangeReasonDetail.EvidenceFileId;
        EvidenceFileName = JourneyInstance!.State.ChangeReasonDetail.EvidenceFileName;
        EvidenceFileSizeDescription = JourneyInstance!.State.ChangeReasonDetail.EvidenceFileSizeDescription;
    }
}
