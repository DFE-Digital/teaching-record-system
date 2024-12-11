using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.SupportUi.Pages.Induction.CompletionDate;

namespace TeachingRecordSystem.SupportUi.Pages.Induction.EditInduction.CompletionDate;

[Journey(JourneyNames.EditInductionCompletionDate)]
public class IndexModel(TrsLinkGenerator linkGenerator) : PageModel
{
    public JourneyInstance<EditInductionCompletionDateState>? JourneyInstance { get; set; }

    public Guid PersonId { get; set; }

    public void OnGet(Guid personId)
    {
        PersonId = personId;
    }

    public void OnPost()
    {
        Redirect(linkGenerator.InductionEditCompletionDate(PersonId, JourneyInstance!.InstanceId));
    }
}
