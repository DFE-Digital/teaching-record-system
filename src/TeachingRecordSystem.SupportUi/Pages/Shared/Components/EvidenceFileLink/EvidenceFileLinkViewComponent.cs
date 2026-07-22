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
            var fileUrl = await fileService.TryGetFileUrlAsync(file.FileId, WebConstants.FileUrlExpiry);
            if (fileUrl is not null)
            {
                file.PreviewUrl = linkGenerator.Files.File(file.FileName, fileUrl);
            }
            else
            {
                file.IsDeleted = true;
            }
        }

        return View(evidenceFile);
    }
}
