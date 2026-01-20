using Microsoft.AspNetCore.Mvc;
using TeachingRecordSystem.Core.Services.Files;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Shared.Components.EvidenceFileLink;

public class EventEvidenceFileLinkViewComponent(
    IFileService fileService,
    SupportUiLinkGenerator linkGenerator) : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync(EventModels.File? evidenceFile)
    {
        UploadedEvidenceFile? uploadedEvidenceFile = null;
        if (evidenceFile is EventModels.File file)
        {
            uploadedEvidenceFile = new UploadedEvidenceFile(file.FileId, file.Name);

            var fileUrl = await fileService.GetFileUrlAsync(file.FileId, WebConstants.FileUrlExpiry);
            uploadedEvidenceFile.PreviewUrl = linkGenerator.Files.File(file.Name, fileUrl);
        }

        return View(uploadedEvidenceFile);
    }
}
