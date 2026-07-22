using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.StaticFiles;
using TeachingRecordSystem.Core.Models.SupportTasks;
using TeachingRecordSystem.Core.Services.Files;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.OneLoginUserMatching.Resolve;

// Serves the proof of identity file that's embedded in the Verify page. It's deliberately outside the
// journey; a journey's pages must all be steps within it and this is not a step the user navigates to.
public class EvidenceModel(ISafeFileService safeFileService) : PageModel
{
    public async Task<IActionResult> OnGetAsync()
    {
        var supportTask = HttpContext.GetCurrentSupportTaskFeature().SupportTask;

        if (supportTask.SupportTaskType != SupportTaskType.OneLoginUserIdVerification)
        {
            return NotFound();
        }

        var data = supportTask.GetData<OneLoginUserIdVerificationData>();

        var fileExtensionContentTypeProvider = new FileExtensionContentTypeProvider();
        if (!fileExtensionContentTypeProvider.TryGetContentType(data.EvidenceFileName, out var mimeType))
        {
            mimeType = "application/octet-stream";
        }

        var stream = await safeFileService.OpenReadStreamAsync(data.EvidenceFileId);
        return File(stream, mimeType);
    }
}
