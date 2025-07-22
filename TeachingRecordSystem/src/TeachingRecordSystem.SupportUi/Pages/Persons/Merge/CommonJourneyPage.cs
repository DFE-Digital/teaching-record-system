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

    public string? ThisTrn { get; set; }

    [FromQuery]
    public bool FromCheckAnswers { get; set; }

    protected string GetPageLink(MergeJourneyPage? pageName)
    {
        return pageName switch
        {
            MergeJourneyPage.EnterTrn => LinkGenerator.PersonMergeEnterTrn(PersonId, JourneyInstance!.InstanceId),
            MergeJourneyPage.CompareMatchingRecords => LinkGenerator.PersonMergeCompareMatchingRecords(PersonId, JourneyInstance!.InstanceId),
            _ => LinkGenerator.PersonDetail(PersonId)
        };
    }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await JourneyInstance!.DeleteAsync();
        return Redirect(GetPageLink(null));
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);

        JourneyInstance!.State.EnsureInitialized();

        var person = await DbContext.Persons.SingleAsync(q => q.PersonId == PersonId);
        ThisTrn = person.Trn;

        OnPageHandlerExecuting(context);
        await OnPageHandlerExecutingAsync(context);
        if (context.Result == null)
        {
            var executedContext = await next();
            OnPageHandlerExecuted(executedContext);
            await OnPageHandlerExecutedAsync(executedContext);
        }
    }

    protected virtual Task OnPageHandlerExecutingAsync(PageHandlerExecutingContext context)
        => Task.CompletedTask;

    protected virtual Task OnPageHandlerExecutedAsync(PageHandlerExecutedContext context)
        => Task.CompletedTask;
}
