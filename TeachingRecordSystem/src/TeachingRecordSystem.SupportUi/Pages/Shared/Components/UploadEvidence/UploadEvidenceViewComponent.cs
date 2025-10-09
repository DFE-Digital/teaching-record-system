using Microsoft.AspNetCore.Mvc;
using TeachingRecordSystem.Core.Services.Files;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Shared.Components.UploadEvidence;

public class UploadEvidenceViewComponent(IFileService fileService) : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync(EvidenceModel evidence)
    {
        if (evidence.UploadedEvidenceFile is UploadedEvidenceFile file)
        {
            file.PreviewUrl = await fileService.GetFileUrlAsync(file.FileId, FileUploadDefaults.FileUrlExpiry);
        }

        return View(evidence);
    }
}
