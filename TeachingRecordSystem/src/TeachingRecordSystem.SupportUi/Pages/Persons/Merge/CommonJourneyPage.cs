using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Services.Files;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.Merge;

public abstract class CommonJourneyPage(
    TrsDbContext dbContext,
    TrsLinkGenerator linkGenerator,
    IFileService fileService) : PageModel
{
    public JourneyInstance<MergeState>? JourneyInstance { get; set; }

    protected TrsDbContext DbContext { get; } = dbContext;
    protected TrsLinkGenerator LinkGenerator { get; } = linkGenerator;
    protected IFileService FileService { get; } = fileService;

    [FromRoute]
    public Guid PersonId { get; set; }

    [FromQuery]
    public bool FromCheckAnswers { get; set; }

    public string CancelLink => LinkGenerator.PersonMergeCancel(PersonId, JourneyInstance!.InstanceId);

    public string GetPageLink(MergeJourneyPage? pageName)
    {
        return pageName switch
        {
            MergeJourneyPage.EnterTrn => LinkGenerator.PersonMergeEnterTrn(PersonId, JourneyInstance!.InstanceId),
            MergeJourneyPage.CompareMatchingRecords => LinkGenerator.PersonMergeCompareMatchingRecords(PersonId, JourneyInstance!.InstanceId),
            MergeJourneyPage.SelectDetailsToMerge => LinkGenerator.PersonMergeSelectDetailsToMerge(PersonId, JourneyInstance!.InstanceId),
            _ => LinkGenerator.PersonDetail(PersonId)
        };
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);

        OnPageHandlerExecuting(context);
        await OnPageHandlerExecutingAsync(context);
        if (context.Result == null)
        {
            var executedContext = await next();
            OnPageHandlerExecuted(executedContext);
            await OnPageHandlerExecutedAsync(executedContext);
        }
    }

    protected virtual async Task OnPageHandlerExecutingAsync(PageHandlerExecutingContext context)
    {
        await JourneyInstance!.State.EnsureInitializedAsync(PersonId, async () => (await DbContext.Persons.SingleAsync(q => q.PersonId == PersonId)).Trn);
    }

    protected virtual Task OnPageHandlerExecutedAsync(PageHandlerExecutedContext context)
        => Task.CompletedTask;

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await JourneyInstance!.DeleteAsync();
        return Redirect(GetPageLink(null));
    }
}
