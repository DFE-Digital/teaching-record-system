using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditInduction;

public class ExemptionReasonModel(TrsLinkGenerator linkGenerator) : PageModel
{
    public JourneyInstance<EditInductionState>? JourneyInstance { get; set; }

    [FromRoute]
    public Guid PersonId { get; set; }

    public void OnGet()
    {
    }

    public void OnPost()
    {
        // TODO - store the induction status

        // TODO - figure out where to go next and redirect to that page
        Redirect(linkGenerator.InductionEditStartDate(PersonId, JourneyInstance!.InstanceId));
    }
}
