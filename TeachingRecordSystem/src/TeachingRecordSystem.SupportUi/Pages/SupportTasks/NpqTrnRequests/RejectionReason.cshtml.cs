using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.NpqTrnRequests;

[Journey(JourneyNames.NpqTrnRequest), RequireJourneyInstance, ActivatesJourney]
public class RejectionReasonModel(TrsDbContext dbContext, TrsLinkGenerator linkGenerator) : NpqTrnRequestPageModel(dbContext, linkGenerator)
{
    public string PersonName => string.Join(" ", SupportTask.TrnRequestMetadata!.Name);

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

    public void OnPostCancel()
    {
        // stub page
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        RequestData = GetRequestData();
        await base.OnPageHandlerExecutionAsync(context, next);
    }
}
