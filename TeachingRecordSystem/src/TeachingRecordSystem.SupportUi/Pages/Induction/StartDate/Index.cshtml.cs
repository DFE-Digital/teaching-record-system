using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TeachingRecordSystem.SupportUi.Pages.Induction.StartDate;

[Journey(JourneyNames.EditInductionStartDate), RequireJourneyInstance]
public class IndexModel(TrsLinkGenerator linkGenerator) : PageModel
{
    public JourneyInstance<EditInductionStartDateState>? JourneyInstance { get; set; }

    [FromRoute]
    public Guid PersonId { get; set; }

    public void OnGet()
    {
    }

    public void OnPost()
    {
        // TODO - store the induction start date

        // TODO - figure out where to go next and redirect to that page
        Redirect(linkGenerator.InductionEditCompletionDate(PersonId, JourneyInstance!.InstanceId));
    }
}
