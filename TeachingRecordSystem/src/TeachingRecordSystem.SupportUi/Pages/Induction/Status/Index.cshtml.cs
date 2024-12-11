using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TeachingRecordSystem.SupportUi.Pages.Induction.Status;


[Journey(JourneyNames.EditInductionStatus), ActivatesJourney, RequireJourneyInstance]
public class IndexModel(TrsLinkGenerator linkGenerator) : PageModel
{
    public JourneyInstance<EditInductionStatusState>? JourneyInstance { get; set; }

    //[FromRoute]
    public Guid PersonId { get; set; }

    public void OnGet(Guid personId)
    {
        PersonId = personId;
    }

    public void OnPost()
    {
        // TODO - store the induction status

        // TODO - figure out where to go next and redirect to that page
        Redirect(linkGenerator.InductionEditStartDate(PersonId, JourneyInstance!.InstanceId));
    }
}
