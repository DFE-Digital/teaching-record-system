using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
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
    }
}
