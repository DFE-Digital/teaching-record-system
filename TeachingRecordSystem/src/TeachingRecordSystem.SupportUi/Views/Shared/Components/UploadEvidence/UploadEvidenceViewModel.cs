using Humanizer;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using TeachingRecordSystem.Core.Services.Files;
using TeachingRecordSystem.SupportUi.Infrastructure.DataAnnotations;
using TeachingRecordSystem.SupportUi.Pages;

namespace TeachingRecordSystem.SupportUi.Views.Shared.Components.UploadEvidence
{
    public class UploadEvidenceViewModel
    {
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

        public bool IsValid => UploadEvidence.HasValue &&
            (UploadEvidence.Value is not true || EvidenceFileId.HasValue);

        public async Task ValidateAsync(string prefix, ModelStateDictionary modelState, IFileService fileService)
        {
            if (UploadEvidence == true && EvidenceFileId is null && EvidenceFile is null)
            {
                modelState.AddModelError($"{prefix}.{nameof(EvidenceFile)}", "Select a file");
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
                // Upload the file and set the display fields
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

            EvidenceFileId = UploadEvidence is true ? EvidenceFileId : null;
            EvidenceFileName = UploadEvidence is true ? EvidenceFileName : null;
            EvidenceFileSizeDescription = UploadEvidence is true ? EvidenceFileSizeDescription : null;
        }
    }
}
