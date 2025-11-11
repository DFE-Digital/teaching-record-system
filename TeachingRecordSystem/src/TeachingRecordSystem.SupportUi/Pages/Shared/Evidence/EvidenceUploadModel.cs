using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

public class EvidenceUploadModel
{
    [BindProperty]
    [Required(ErrorMessage = "Select yes if you want to upload evidence")]
    public bool? UploadEvidence { get; set; }

    [JsonIgnore]
    [BindProperty]
    [EvidenceFile]
    [FileSize(UiDefaults.MaxFileUploadSizeMb * 1024 * 1024, ErrorMessage = $"The selected file {UiDefaults.MaxFileUploadSizeErrorMessage}")]
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
