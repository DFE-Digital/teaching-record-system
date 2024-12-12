using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditInduction;

[Journey(JourneyNames.EditInduction), ActivatesJourney, RequireJourneyInstance]
public class StatusModel(TrsLinkGenerator linkGenerator) : PageModel
{
    public JourneyInstance<EditInductionState>? JourneyInstance { get; set; }

    [FromRoute]
    public Guid PersonId { get; set; }

    [BindProperty]
    public InductionStatus Status { get; set; }


    public void OnGet()
    {
    }

    public void OnPost()
    {
        // TODO - store the induction status

        // TODO - figure out where to go next and redirect to that page
        Redirect(linkGenerator.InductionEditExemptionReason(PersonId, JourneyInstance!.InstanceId));
    }
}
