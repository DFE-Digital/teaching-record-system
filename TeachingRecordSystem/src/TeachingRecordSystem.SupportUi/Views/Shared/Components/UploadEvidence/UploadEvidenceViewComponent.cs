using Humanizer;
using Microsoft.AspNetCore.Mvc;
using TeachingRecordSystem.Core.Services.Files;
using TeachingRecordSystem.SupportUi.Pages;

namespace TeachingRecordSystem.SupportUi.Views.Shared.Components.UploadEvidence
{
    public class UploadEvidenceViewComponent(IFileService fileService) : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(UploadEvidenceViewModel? model, string prefix)
        {
            model ??= new UploadEvidenceViewModel();

            if (HttpContext.Request.Method == "GET")
            {
                model.UploadedEvidenceFileUrl = model.EvidenceFileId is null ? null :
                    await fileService.GetFileUrlAsync(model.EvidenceFileId.Value, FileUploadDefaults.FileUrlExpiry);
            }
            else if (HttpContext.Request.Method == "POST")
            {
                if (model.UploadEvidence == true && model.EvidenceFileId is null && model.EvidenceFile is null)
                {
                    ModelState.AddModelError(nameof(model.EvidenceFile), "Select a file");
                }

                // Delete any previously uploaded file if they're uploading a new one,
                // or choosing not to upload evidence (check for UploadEvidence != true because if
                // UploadEvidence somehow got set to null we still want to delete the file)
                if (model.EvidenceFileId.HasValue && (model.EvidenceFile is not null || model.UploadEvidence != true))
                {
                    await fileService.DeleteFileAsync(model.EvidenceFileId.Value);

                    model.EvidenceFileName = null;
                    model.EvidenceFileSizeDescription = null;
                    model.UploadedEvidenceFileUrl = null;
                    model.EvidenceFileId = null;
                }

                // Upload the file even if the rest of the form is invalid
                // otherwise the user will have to re-upload every time they re-submit
                if (model.UploadEvidence == true)
                {
                    // Upload the file and set the display fields
                    if (model.EvidenceFile is not null)
                    {
                        using var stream = model.EvidenceFile.OpenReadStream();
                        var evidenceFileId = await fileService.UploadFileAsync(stream, model.EvidenceFile.ContentType);
                        model.EvidenceFileId = evidenceFileId;
                        model.EvidenceFileName = model.EvidenceFile?.FileName;
                        model.EvidenceFileSizeDescription = model.EvidenceFile?.Length.Bytes().Humanize();
                        model.UploadedEvidenceFileUrl = await fileService.GetFileUrlAsync(evidenceFileId, FileUploadDefaults.FileUrlExpiry);
                    }
                }

                //model.EvidenceFileId = model.UploadEvidence is true ? model.EvidenceFileId : null;
                //model.EvidenceFileName = model.UploadEvidence is true ? model.EvidenceFileName : null;
                //model.EvidenceFileSizeDescription = model.UploadEvidence is true ? model.EvidenceFileSizeDescription : null;
            }

            return View((model, prefix));
        }
    }
}
