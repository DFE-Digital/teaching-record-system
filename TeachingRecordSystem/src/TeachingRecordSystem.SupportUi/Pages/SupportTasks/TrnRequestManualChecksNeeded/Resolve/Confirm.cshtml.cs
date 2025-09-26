using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.TrnRequestManualChecksNeeded.Resolve;

public class Confirm(TrsDbContext dbContext, IClock clock, TrsLinkGenerator linkGenerator) : PageModel
{
    [FromRoute]
    public string? SupportTaskReference { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var supportTask = HttpContext.GetCurrentSupportTaskFeature().SupportTask;
        supportTask.Status = SupportTaskStatus.Closed;
        supportTask.UpdatedOn = clock.UtcNow;

        var trnRequest = supportTask.TrnRequestMetadata!;
        trnRequest.SetCompleted();

        await dbContext.SaveChangesAsync();

        TempData.SetFlashSuccess($"TRN request for {trnRequest.FirstName} {trnRequest.MiddleName} {trnRequest.LastName} completed");

        return Redirect(linkGenerator.TrnRequestManualChecksNeeded());
    }
}
