using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TeachingRecordSystem.SupportUi.Pages.Induction.CompletionDate;

[Journey(JourneyNames.EditInductionCompletionDate)]
public class IndexModel(TrsLinkGenerator linkGenerator) : PageModel
{
    public JourneyInstance<EditInductionCompletionDateState>? JourneyInstance { get; set; }

    [FromRoute]
    public Guid PersonId { get; set; }

    public void OnGet()
    {
    }

    public void OnPost()
    {
        Redirect(linkGenerator.InductionEditCompletionDate(PersonId, JourneyInstance!.InstanceId));
    }
}
