using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.SetStatus;

public abstract class CommonJourneyPage(
    TrsDbContext dbContext,
    TrsLinkGenerator linkGenerator,
    EvidenceUploadManager evidenceController) : PageModel
{
    public JourneyInstance<SetStatusState>? JourneyInstance { get; set; }

    protected TrsDbContext DbContext { get; } = dbContext;
    protected TrsLinkGenerator LinkGenerator { get; } = linkGenerator;
    protected EvidenceUploadManager EvidenceController { get; } = evidenceController;
    protected Person? Person { get; set; }

    [FromRoute]
    public Guid PersonId { get; set; }
    public string? PersonName { get; set; }

    [FromRoute]
    public PersonStatus TargetStatus { get; set; }

    [FromQuery]
    public bool FromCheckAnswers { get; set; }

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

    protected async virtual Task OnPageHandlerExecutingAsync(PageHandlerExecutingContext context)
    {
        JourneyInstance!.State.EnsureInitialized();

        var personInfo = context.HttpContext.GetCurrentPersonFeature();
        PersonId = personInfo.PersonId;
        PersonName = personInfo.Name;

        Person = await DbContext.Persons
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(u => u.PersonId == PersonId);

        if (Person is null)
        {
            context.Result = NotFound();
            return;
        }

        if (Person.Status == TargetStatus)
        {
            context.Result = BadRequest();
            return;
        }

        // Person cannot be reactivated if they were deactivated as part of a merge
        // where they were merged into another Person (i.e. they were the secondary
        // Person and the other Person was primary)
        if (Person.Status == PersonStatus.Deactivated && Person.MergedWithPersonId is not null)
        {
            context.Result = BadRequest();
            return;
        }
    }

    protected virtual Task OnPageHandlerExecutedAsync(PageHandlerExecutedContext context)
        => Task.CompletedTask;

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await EvidenceController.DeleteUploadedFileAsync(JourneyInstance!.State.Evidence.UploadedEvidenceFile);
        await JourneyInstance!.DeleteAsync();
        return Redirect(LinkGenerator.PersonDetail(PersonId));
    }
}
