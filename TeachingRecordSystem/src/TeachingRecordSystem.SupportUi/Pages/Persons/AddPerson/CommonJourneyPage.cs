using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.AddPerson;

public abstract class CommonJourneyPage(
    TrsDbContext dbContext,
    SupportUiLinkGenerator linkGenerator,
    EvidenceUploadManager evidenceUploadManager) : PageModel
{
    public JourneyInstance<AddPersonState>? JourneyInstance { get; set; }

    protected TrsDbContext DbContext { get; } = dbContext;
    protected SupportUiLinkGenerator LinkGenerator { get; } = linkGenerator;
    protected EvidenceUploadManager EvidenceUploadManager { get; } = evidenceUploadManager;

    [FromQuery]
    public bool FromCheckAnswers { get; set; }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await EvidenceUploadManager.DeleteUploadedFileAsync(JourneyInstance!.State.Evidence.UploadedEvidenceFile);
        await JourneyInstance!.DeleteAsync();
        return Redirect(LinkGenerator.Persons.AddPerson.Index());
    }

    protected string GetPageLink(AddPersonJourneyPage? pageName, bool? fromCheckAnswers = null)
    {
        return pageName switch
        {
            AddPersonJourneyPage.PersonalDetails => LinkGenerator.Persons.AddPerson.PersonalDetails(JourneyInstance!.InstanceId, fromCheckAnswers),
            AddPersonJourneyPage.Reason => LinkGenerator.Persons.AddPerson.Reason(JourneyInstance!.InstanceId, fromCheckAnswers),
            AddPersonJourneyPage.CheckAnswers => LinkGenerator.Persons.AddPerson.CheckAnswers(JourneyInstance!.InstanceId),
            _ => LinkGenerator.Persons.AddPerson.Index()
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

    public virtual Task OnPageHandlerExecutingAsync(PageHandlerExecutingContext context)
        => Task.CompletedTask;

    public virtual Task OnPageHandlerExecutedAsync(PageHandlerExecutedContext context)
        => Task.CompletedTask;
}
