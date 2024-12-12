using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditInduction;

[Journey(JourneyNames.EditInduction), ActivatesJourney, RequireJourneyInstance]
public class StatusModel(TrsLinkGenerator linkGenerator, TrsDbContext dbContext) : PageModel
{
    public JourneyInstance<EditInductionState>? JourneyInstance { get; set; }

    [FromRoute]
    public Guid PersonId { get; set; }

    [BindProperty]
    public InductionStatus InductionStatus { get; set; }

    public async Task OnGetAsync()
    {
        InductionStatus = JourneyInstance!.State.InductionStatus;
        var person = await dbContext.Persons
            .SingleAsync(q => q.PersonId == PersonId);
        InductionStatus = person!.InductionStatus;
    }

    public async Task OnPostAsync()
    {
        await JourneyInstance!.UpdateStateAsync(state => state.InductionStatus = InductionStatus);

        // Use the delegate to determine where to go next and redirect to that page
        Redirect(SetNextPage(InductionStatus)(PersonId, JourneyInstance!.InstanceId));
    }

    private Func<Guid, JourneyInstanceId, string> SetNextPage(InductionStatus status)
    {
        if (InductionStatus == InductionStatus.Exempt)
        {
            return (personId, journeyInstanceId) => linkGenerator.InductionEditExemptionReason(personId, journeyInstanceId);
        }
        //if (InductionStatus == InductionStatus.RequiredToComplete)
        //{
        //    return (personId, journeyInstanceId) => linkGenerator.InductionChangeReason(personId, journeyInstanceId);
        //}
        else 
        {
            return (personId, journeyInstanceId) => linkGenerator.InductionEditStartDate(personId, journeyInstanceId);
        }
    }
}
