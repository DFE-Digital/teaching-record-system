using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.Services.SupportTasks.TrnRequests;
using TeachingRecordSystem.Core.Services.TrnRequests;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.TrnRequestManualChecksNeeded.Resolve;

public class Confirm(TrnRequestService trnRequestService, TrnRequestSupportTaskService trnRequestSupportTaskService, TimeProvider timeProvider, SupportUiLinkGenerator linkGenerator) : PageModel
{
    [FromRoute]
    public string? SupportTaskReference { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var supportTask = HttpContext.GetCurrentSupportTaskFeature().SupportTask;
        var trnRequest = supportTask.TrnRequestMetadata!;

        var processContext = new ProcessContext(ProcessType.TrnRequestManualChecksNeededTaskCompleting, timeProvider.UtcNow, User.GetUserId());

        await trnRequestService.CompleteResolvedTrnRequestAsync(trnRequest, processContext);

        await trnRequestSupportTaskService.CompleteManualChecksNeededSupportTaskAsync(
            new()
            {
                SupportTaskReference = SupportTaskReference!
            },
            processContext);

        TempData.SetFlashNotificationBanner($"TRN request for {trnRequest.FirstName} {trnRequest.MiddleName} {trnRequest.LastName} completed");

        return Redirect(linkGenerator.SupportTasks.TrnRequestManualChecksNeeded.Index());
    }
}
