using Microsoft.AspNetCore.Mvc;
using TeachingRecordSystem.Core.Services.Files;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Shared.Components.UploadEvidence;

public class UploadEvidenceViewComponent(
    IFileService fileService,
    SupportUiLinkGenerator linkGenerator) : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync(EvidenceUploadModel evidence)
    {
        if (evidence.UploadedEvidenceFile is UploadedEvidenceFile file)
        {
            var fileUrl = await fileService.GetFileUrlAsync(file.FileId, UiDefaults.FileUrlExpiry);
            file.PreviewUrl = linkGenerator.Files.File(file.FileName, fileUrl);
        }

        return View(evidence);
    }
}
