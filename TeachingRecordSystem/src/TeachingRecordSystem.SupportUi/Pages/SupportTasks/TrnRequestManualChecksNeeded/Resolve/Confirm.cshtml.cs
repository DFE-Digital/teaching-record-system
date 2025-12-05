using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.Models.SupportTasks;
using TeachingRecordSystem.Core.Services.SupportTasks;
using TeachingRecordSystem.Core.Services.TrnRequests;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.TrnRequestManualChecksNeeded.Resolve;

public class Confirm(TrnRequestService trnRequestService, SupportTaskService supportTaskService, IClock clock, SupportUiLinkGenerator linkGenerator) : PageModel
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

        var processContext = new ProcessContext(ProcessType.TrnRequestManualChecksNeededTaskCompleting, clock.UtcNow, User.GetUserId());

        await trnRequestService.CompleteResolvedTrnRequestAsync(trnRequest, processContext);

        await supportTaskService.UpdateSupportTaskAsync(
            new UpdateSupportTaskOptions<TrnRequestManualChecksNeededData>
            {
                SupportTaskReference = SupportTaskReference!,
                UpdateData = data => data,
                Status = SupportTaskStatus.Closed
            },
            processContext);

        TempData.SetFlashSuccess($"TRN request for {trnRequest.FirstName} {trnRequest.MiddleName} {trnRequest.LastName} completed");

        return Redirect(linkGenerator.SupportTasks.TrnRequestManualChecksNeeded.Index());
    }
}
