using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Services.Files;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.AddPerson;

public abstract class CommonJourneyPage(
    TrsDbContext dbContext,
    TrsLinkGenerator linkGenerator,
    IFileService fileService) : PageModel
{
    public JourneyInstance<AddPersonState>? JourneyInstance { get; set; }

    protected TrsDbContext DbContext { get; } = dbContext;
    protected TrsLinkGenerator LinkGenerator { get; } = linkGenerator;
    protected IFileService FileService { get; } = fileService;

    [FromQuery]
    public bool FromCheckAnswers { get; set; }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        if (JourneyInstance!.State.EvidenceFileId.HasValue)
        {
            await FileService.DeleteFileAsync(JourneyInstance!.State.EvidenceFileId.Value);
        }

        await JourneyInstance!.DeleteAsync();
        return Redirect(LinkGenerator.PersonCreate());
    }

    protected string GetPageLink(AddPersonJourneyPage? pageName, bool? fromCheckAnswers = null)
    {
        return pageName switch
        {
            AddPersonJourneyPage.PersonalDetails => LinkGenerator.PersonCreatePersonalDetails(JourneyInstance!.InstanceId, fromCheckAnswers),
            AddPersonJourneyPage.Reason => LinkGenerator.PersonCreateCreateReason(JourneyInstance!.InstanceId, fromCheckAnswers),
            AddPersonJourneyPage.CheckAnswers => LinkGenerator.PersonCreateCheckAnswers(JourneyInstance!.InstanceId),
            _ => LinkGenerator.PersonCreate()
        };
    }

    public AddPersonJourneyPage NextIncompletePage =>
        !JourneyInstance!.State.IsPersonalDetailsComplete
            ? AddPersonJourneyPage.PersonalDetails
            : !JourneyInstance!.State.IsCreateReasonComplete
                ? AddPersonJourneyPage.Reason
                : AddPersonJourneyPage.CheckAnswers;

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);

        JourneyInstance!.State.EnsureInitialized();

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
