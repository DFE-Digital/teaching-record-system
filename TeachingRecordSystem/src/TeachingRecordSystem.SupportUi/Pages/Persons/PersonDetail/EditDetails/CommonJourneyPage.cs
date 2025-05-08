using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Graph.Beta.Models;
using TeachingRecordSystem.Core;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditInduction;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditDetails;

public abstract class CommonJourneyPage(TrsDbContext dbContext, TrsLinkGenerator linkGenerator) : PageModel
{
    public JourneyInstance<EditDetailsState>? JourneyInstance { get; set; }

    protected TrsLinkGenerator LinkGenerator { get; } = linkGenerator;
    protected TrsDbContext DbContext { get; } = dbContext;

    [FromRoute]
    public Guid PersonId { get; set; }
    public string? PersonName { get; set; }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await JourneyInstance!.DeleteAsync();
        return Redirect(LinkGenerator.PersonDetail(PersonId));
    }

    protected string GetPageLink(EditDetailsJourneyPage? pageName)
    {
        return pageName switch
        {
            EditDetailsJourneyPage.Index => LinkGenerator.EditDetailsIndex(PersonId, JourneyInstance!.InstanceId),
            EditDetailsJourneyPage.ChangeReason => LinkGenerator.EditDetailsChangeReason(PersonId, JourneyInstance!.InstanceId),
            EditDetailsJourneyPage.CheckAnswers => LinkGenerator.EditDetailsCheckAnswers(PersonId, JourneyInstance!.InstanceId),
            _ => LinkGenerator.PersonDetail(PersonId)
        };
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        await JourneyInstance!.State.EnsureInitializedAsync(DbContext, PersonId, EditDetailsJourneyPage.Index);

        var personInfo = context.HttpContext.GetCurrentPersonFeature();
        PersonId = personInfo.PersonId;
        PersonName = personInfo.Name;

        await next();
    }
}
