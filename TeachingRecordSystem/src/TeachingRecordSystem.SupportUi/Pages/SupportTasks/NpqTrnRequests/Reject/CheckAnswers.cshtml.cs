using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Events.Legacy;
using TeachingRecordSystem.Core.Models.SupportTasks;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.NpqTrnRequests.Reject;

[Journey(JourneyNames.RejectNpqTrnRequest), RequireJourneyInstance]
public class CheckAnswersModel(
    TrsDbContext dbContext,
    SupportUiLinkGenerator linkGenerator,
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
        var oldSupportTaskEventModel = EventModels.SupportTask.FromModel(supportTask);

        supportTask.Status = SupportTaskStatus.Closed;
        supportTask.UpdatedOn = clock.UtcNow;
        supportTask.UpdateData<NpqTrnRequestData>(data => data with
        {
            SupportRequestOutcome = SupportRequestOutcome.Rejected
        });

        var @event = new NpqTrnRequestSupportTaskRejectedEvent()
        {
            RequestData = EventModels.TrnRequestMetadata.FromModel(supportTask.TrnRequestMetadata!),
            SupportTask = EventModels.SupportTask.FromModel(supportTask),
            OldSupportTask = oldSupportTaskEventModel,
            RejectionReason = JourneyInstance!.State.RejectionReason!.GetDisplayName(),
            EventId = Guid.NewGuid(),
            CreatedUtc = clock.UtcNow,
            RaisedBy = User.GetUserId()
        };

        await dbContext.AddEventAndBroadcastAsync(@event);

        await dbContext.SaveChangesAsync();

        TempData.SetFlashSuccess(
            $"TRN request for {PersonName} rejected");

        await JourneyInstance!.CompleteAsync();
        return Redirect(linkGenerator.SupportTasks.NpqTrnRequests.Index());
    }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await JourneyInstance!.DeleteAsync();
        return Redirect(linkGenerator.SupportTasks.NpqTrnRequests.Index());
    }

    public override Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        var supportTask = HttpContext.GetCurrentSupportTaskFeature().SupportTask;
        RequestData = supportTask.TrnRequestMetadata!;
        SourceApplicationUserName = RequestData.ApplicationUser!.Name;

        RejectionReason = JourneyInstance!.State.RejectionReason;
        if (!RejectionReason.HasValue)
        {
            Redirect(linkGenerator.SupportTasks.NpqTrnRequests.Reject.Reason(supportTask.SupportTaskReference, JourneyInstance!.InstanceId));
        }

        return next();
    }
}
