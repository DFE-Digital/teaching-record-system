using Microsoft.AspNetCore.Mvc;
using TeachingRecordSystem.Core.Services.Files;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Shared.Components.EvidenceFileLink;

public class EvidenceFileLinkViewComponent(IFileService fileService) : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync(UploadedEvidenceFile? evidenceFile)
    {
        if (evidenceFile is UploadedEvidenceFile file)
        {
            file.PreviewUrl = await fileService.GetFileUrlAsync(file.FileId, UiDefaults.FileUrlExpiry);
        }

        return View(evidenceFile);
    }
}
