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
        var cancellationToken = HttpContext.RequestAborted;

        UploadedEvidenceFile? uploadedEvidenceFile = null;
        if (evidenceFile is EventModels.File file)
        {
            uploadedEvidenceFile = new UploadedEvidenceFile(file.FileId, file.Name);

            var fileUrl = await fileService.TryGetFileUrlAsync(file.FileId, WebConstants.FileUrlExpiry, cancellationToken);
            if (fileUrl is not null)
            {
                uploadedEvidenceFile.PreviewUrl = linkGenerator.Files.File(file.Name, fileUrl);
            }
            else
            {
                uploadedEvidenceFile.IsDeleted = true;
            }
        }

        return View(uploadedEvidenceFile);
    }
}
