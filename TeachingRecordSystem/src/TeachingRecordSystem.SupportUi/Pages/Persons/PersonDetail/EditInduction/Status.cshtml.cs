using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditInduction;

[Journey(JourneyNames.EditInduction), ActivatesJourney, RequireJourneyInstance]
public class StatusModel(TrsLinkGenerator linkGenerator, TrsDbContext dbContext) : PageModel
{
    public JourneyInstance<EditInductionState>? JourneyInstance { get; set; }
    public string BackLink => BackPage()(PersonId, JourneyInstance!.InstanceId);

    [FromRoute]
    public Guid PersonId { get; set; }

    [BindProperty]
    public InductionStatus InductionStatus { get; set; }

    public void OnGet()
    {
        InductionStatus = JourneyInstance!.State.InductionStatus;
    }

    public async Task OnPostAsync()
    {
        await JourneyInstance!.UpdateStateAsync(state => state.InductionStatus = InductionStatus);

        // Determine where to go next and redirect to that page
        Redirect(NextPage(InductionStatus)(PersonId, JourneyInstance!.InstanceId));
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        await JourneyInstance!.State.EnsureInitializedAsync(dbContext, PersonId);

        await next();
    }

    private Func<Guid, JourneyInstanceId, string> NextPage(InductionStatus status)
    {
        if (status == InductionStatus.Exempt)
        {
            return (Id, journeyInstanceId) => linkGenerator.InductionEditExemptionReason(Id, journeyInstanceId);
        }
        //if (status == InductionStatus.RequiredToComplete)
        //{
        //    return (personId, journeyInstanceId) => linkGenerator.InductionChangeReason(personId, journeyInstanceId);
        //}
        else
        {
            return (Id, journeyInstanceId) => linkGenerator.InductionEditStartDate(Id, journeyInstanceId);
        }
    }

    private Func<Guid, JourneyInstanceId, string> BackPage()
    {
        return (Id, journeyInstanceId) => linkGenerator.PersonInduction(Id);
    }
}
