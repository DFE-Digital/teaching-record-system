using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditDetails;

public abstract class CommonJourneyPage(TrsDbContext dbContext, TrsLinkGenerator linkGenerator) : PageModel
{
    public JourneyInstance<EditDetailsState>? JourneyInstance { get; set; }

    protected TrsLinkGenerator LinkGenerator { get; } = linkGenerator;
    protected TrsDbContext DbContext { get; } = dbContext;

    [FromRoute]
    public Guid PersonId { get; set; }

    [FromQuery]
    public bool FromCheckAnswers { get; set; }

    public string? PersonName { get; set; }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await JourneyInstance!.DeleteAsync();
        return Redirect(LinkGenerator.PersonDetail(PersonId));
    }

    protected string GetPageLink(EditDetailsJourneyPage? pageName, bool fromCheckAnswers = false)
    {
        return pageName switch
        {
            EditDetailsJourneyPage.Index => LinkGenerator.EditDetailsIndex(PersonId, JourneyInstance!.InstanceId, fromCheckAnswers),
            EditDetailsJourneyPage.ChangeReason => LinkGenerator.EditDetailsChangeReason(PersonId, JourneyInstance!.InstanceId, fromCheckAnswers),
            EditDetailsJourneyPage.CheckAnswers => LinkGenerator.EditDetailsCheckAnswers(PersonId, JourneyInstance!.InstanceId),
            _ => LinkGenerator.PersonDetail(PersonId)
        };
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);

        await OnPageHandlerExecutingAsync(context);
        if (context.Result == null)
        {
            await OnPageHandlerExecutedAsync(await next());
        }
    }

    protected virtual async Task OnPageHandlerExecutingAsync(PageHandlerExecutingContext context)
    {
        await JourneyInstance!.State.EnsureInitializedAsync(DbContext, PersonId, EditDetailsJourneyPage.Index);

        var personInfo = context.HttpContext.GetCurrentPersonFeature();
        PersonId = personInfo.PersonId;
        PersonName = personInfo.Name;
    }

    protected virtual Task OnPageHandlerExecutedAsync(PageHandlerExecutedContext context)
        => Task.CompletedTask;
}
