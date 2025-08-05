using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.NpqTrnRequests.Reject;

[Journey(JourneyNames.RejectNpqTrnRequest), RequireJourneyInstance]
public class CheckAnswersModel(
    TrsDbContext dbContext,
    TrsLinkGenerator linkGenerator,
    IClock clock) : PageModel
{
    public string? SourceApplicationUserName { get; set; }

    public string PersonName => StringHelper.JoinNonEmpty(' ', RequestData!.Name);

    public TrnRequestMetadata? RequestData { get; set; }

    public JourneyInstance<RejectNpqTrnRequestState>? JourneyInstance { get; set; }

    public RejectionReasonOption? RejectionReason { get; set; }

    [FromRoute]
    public string SupportTaskReference { get; set; } = null!;

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var supportTask = HttpContext.GetCurrentSupportTaskFeature().SupportTask;
        supportTask.Status = SupportTaskStatus.Closed;
        supportTask.UpdatedOn = clock.UtcNow;
        await dbContext.SaveChangesAsync();

        TempData.SetFlashSuccess(
            $"{SourceApplicationUserName} request for {PersonName} rejected");

        await JourneyInstance!.CompleteAsync();
        return Redirect(linkGenerator.NpqTrnRequests());
    }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await JourneyInstance!.DeleteAsync();
        return Redirect(linkGenerator.NpqTrnRequests());
    }

    public override Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        var supportTask = HttpContext.GetCurrentSupportTaskFeature().SupportTask;
        RequestData = supportTask.TrnRequestMetadata!;
        SourceApplicationUserName = RequestData.ApplicationUser!.Name;

        RejectionReason = JourneyInstance!.State.RejectionReason;

        return base.OnPageHandlerExecutionAsync(context, next);
    }
}
