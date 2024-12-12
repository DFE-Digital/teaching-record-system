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
    public InductionStatus Status { get; set; }


    public async Task OnGetAsync()
    {
        var person = await dbContext.Persons
            .SingleAsync(q => q.PersonId == PersonId);

        Status = person!.InductionStatus;
    }

    public async Task OnPostAsync()
    {
        // TODO - store the induction status
        await JourneyInstance!.UpdateStateAsync(state => state.InductionStatus = Status);

        // TODO - figure out where to go next and redirect to that page
        Redirect(linkGenerator.InductionEditExemptionReason(PersonId, JourneyInstance!.InstanceId));
    }
}
