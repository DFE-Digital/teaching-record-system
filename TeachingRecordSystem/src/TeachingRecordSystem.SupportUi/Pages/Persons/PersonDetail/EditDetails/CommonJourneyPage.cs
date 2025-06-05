using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Services.Files;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditDetails;

public abstract class CommonJourneyPage(
    TrsDbContext dbContext,
    TrsLinkGenerator linkGenerator,
    IFileService fileService) : PageModel
{
    public JourneyInstance<EditDetailsState>? JourneyInstance { get; set; }

    protected TrsDbContext DbContext { get; } = dbContext;
    protected TrsLinkGenerator LinkGenerator { get; } = linkGenerator;
    protected IFileService FileService { get; } = fileService;

    [FromRoute]
    public Guid PersonId { get; set; }
    public string? PersonName { get; set; }

    [FromQuery]
    public bool FromCheckAnswers { get; set; }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        if (JourneyInstance!.State.OtherDetailsChangeEvidenceFileId.HasValue)
        {
            await FileService.DeleteFileAsync(JourneyInstance!.State.OtherDetailsChangeEvidenceFileId.Value);
        }

        await JourneyInstance!.DeleteAsync();
        return Redirect(LinkGenerator.PersonDetail(PersonId));
    }

    protected string GetPageLink(EditDetailsJourneyPage? pageName, bool? fromCheckAnswers = null)
    {
        return pageName switch
        {
            EditDetailsJourneyPage.Index => LinkGenerator.EditDetailsIndex(PersonId, JourneyInstance!.InstanceId, fromCheckAnswers),
            EditDetailsJourneyPage.NameChangeReason => LinkGenerator.EditDetailsNameChangeReason(PersonId, JourneyInstance!.InstanceId, fromCheckAnswers),
            EditDetailsJourneyPage.OtherDetailsChangeReason => LinkGenerator.EditDetailsOtherDetailsChangeReason(PersonId, JourneyInstance!.InstanceId, fromCheckAnswers),
            EditDetailsJourneyPage.CheckAnswers => LinkGenerator.EditDetailsCheckAnswers(PersonId, JourneyInstance!.InstanceId),
            _ => LinkGenerator.PersonDetail(PersonId)
        };
    }

    public EditDetailsJourneyPage NextIncompletePage =>
        !JourneyInstance!.State.IsIndexComplete
            ? EditDetailsJourneyPage.Index
            : !JourneyInstance!.State.IsNameChangeReasonComplete
                ? EditDetailsJourneyPage.NameChangeReason
                : !JourneyInstance!.State.IsOtherDetailsChangeReasonComplete
                    ? EditDetailsJourneyPage.OtherDetailsChangeReason
                    : EditDetailsJourneyPage.CheckAnswers;

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);

        var person = await DbContext.Persons.SingleAsync(q => q.PersonId == PersonId);
        JourneyInstance!.State.EnsureInitialized(person);

        var personInfo = context.HttpContext.GetCurrentPersonFeature();
        PersonId = personInfo.PersonId;
        PersonName = personInfo.Name;

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
