using Microsoft.AspNetCore.Mvc;
using TeachingRecordSystem.Core.Services.Files;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Shared.Components.EvidenceFileLink;

public class EvidenceFileLinkViewComponent(
    IFileService fileService,
    SupportUiLinkGenerator linkGenerator) : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync(UploadedEvidenceFile? evidenceFile)
    {
        if (evidenceFile is UploadedEvidenceFile file)
        {
            var fileUrl = await fileService.GetFileUrlAsync(file.FileId, WebConstants.FileUrlExpiry);
            file.PreviewUrl = linkGenerator.Files.File(file.FileName, fileUrl);
        }

        return View(evidenceFile);
    }
}
