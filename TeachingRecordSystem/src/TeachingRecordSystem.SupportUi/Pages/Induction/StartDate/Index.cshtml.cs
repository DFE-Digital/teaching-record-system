using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.SupportUi.Pages.Induction.StartDate;

namespace TeachingRecordSystem.SupportUi.Pages.Induction.EditInduction.StartDate;

[Journey(JourneyNames.EditInductionStartDate)]
public class IndexModel(TrsLinkGenerator linkGenerator) : PageModel
{
    public JourneyInstance<EditInductionStartDateState>? JourneyInstance { get; set; }

    //[FromRoute]
    public Guid PersonId { get; set; }

    public void OnGet(Guid personId)
    {
        PersonId = personId;
    }

    public void OnPost()
    {
        // TODO - store the induction start date

        // TODO - figure out where to go next and redirect to that page
        Redirect(linkGenerator.InductionEditCompletionDate(PersonId, JourneyInstance!.InstanceId));
    }
}
