using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.Pages.SupportTasks.NpqTrnRequests.Resolve;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.NpqTrnRequests.Reject;

[Journey(JourneyNames.ResolveNpqTrnRequest), RequireJourneyInstance, ActivatesJourney]
public class RejectionReasonModel(TrsDbContext dbContext, TrsLinkGenerator linkGenerator) : ResolveNpqTrnRequestPageModel(dbContext)
{
    public string PersonName => string.Join(" ", RequestData!.Name);

    public TrnRequestMetadata? RequestData { get; set; }

    [BindProperty]
    public RejectionReasonOption RejectionReason { get; set; }

    public void OnGet()
    {
        // stub page
    }

    public void OnPost()
    {
        // stub page
    }

    public IActionResult OnPostCancel()
    {
        // stub page
        return Redirect(linkGenerator.SupportTasks());
    }

    public override Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        RequestData = GetRequestData();
        return base.OnPageHandlerExecutionAsync(context, next);
    }
}
