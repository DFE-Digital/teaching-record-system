using Humanizer;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using TeachingRecordSystem.Core.Services.Files;

namespace TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

public class EvidenceController(IFileService fileService)
{
    public async Task ValidateAndUploadAsync(EvidenceModel evidence, ModelStateDictionary modelState)
    {
        if (evidence.UploadEvidence == true && evidence.EvidenceFile is null && evidence.UploadedEvidenceFile is null)
        {
            modelState.AddModelError(nameof(evidence.EvidenceFile), "Select a file");
        }

        // Delete any previously uploaded file if they're uploading a new one,
        // or choosing not to upload evidence (check for UploadEvidence != true because if
        // UploadEvidence somehow got set to null we still want to delete the file)
        if (evidence.UploadedEvidenceFile is UploadedEvidenceFile file && (evidence.EvidenceFile is not null || evidence.UploadEvidence != true))
        {
            await fileService!.DeleteFileAsync(file.FileId);
            evidence.UploadedEvidenceFile = null;
        }

        // Upload the file and set the display fields even if the rest of the form is invalid
        // otherwise the user will have to re-upload every time they re-submit
        if (evidence.UploadEvidence == true && evidence.EvidenceFile is IFormFile uploadedFile)
        {
            using var stream = evidence.EvidenceFile.OpenReadStream();
            var fileId = await fileService!.UploadFileAsync(stream, evidence.EvidenceFile.ContentType);
            evidence.UploadedEvidenceFile = new()
            {
                FileId = fileId,
                FileName = uploadedFile.FileName,
                FileSizeDescription = uploadedFile.Length.Bytes().Humanize()
            };
            evidence.EvidenceFile = null;
        }
    }

    public async Task DeleteUploadedFileAsync(UploadedEvidenceFile? evidenceFile)
    {
        if (evidenceFile is UploadedEvidenceFile file)
        {
            await fileService.DeleteFileAsync(file.FileId);
        }
    }
}
