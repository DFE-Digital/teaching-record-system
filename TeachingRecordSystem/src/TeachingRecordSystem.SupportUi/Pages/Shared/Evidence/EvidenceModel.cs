using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

public class EvidenceModel
{
    [BindProperty]
    [Display(Name = "Do you want to upload evidence?")]
    [Required(ErrorMessage = "Select yes if you want to upload evidence")]
    public bool? UploadEvidence { get; set; }

    [JsonIgnore]
    [BindProperty]
    [EvidenceFile]
    [FileSize(FileUploadDefaults.MaxFileUploadSizeMb * 1024 * 1024, ErrorMessage = $"The selected file {FileUploadDefaults.MaxFileUploadSizeErrorMessage}")]
    public IFormFile? EvidenceFile { get; set; }

    [BindProperty]
    public UploadedEvidenceFile? UploadedEvidenceFile { get; set; }

    public bool IsComplete => UploadEvidence is bool uploadEvidence &&
        (!uploadEvidence || UploadedEvidenceFile is not null);

    public void Clear()
    {
        UploadEvidence = null;
        EvidenceFile = null;
        UploadedEvidenceFile = null;
    }
}
