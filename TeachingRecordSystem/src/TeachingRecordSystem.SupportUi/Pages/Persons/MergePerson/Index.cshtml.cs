using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.MergePerson;

[Journey(JourneyNames.MergePerson), ActivatesJourney, RequireJourneyInstance]
public class IndexModel(SupportUiLinkGenerator linkGenerator) : PageModel
{
    public JourneyInstance<MergePersonState>? JourneyInstance { get; set; }

    [FromRoute]
    public Guid PersonId { get; set; }

    public IActionResult OnGet()
    {
        return Redirect(linkGenerator.Persons.MergePerson.EnterTrn(PersonId, JourneyInstance!.InstanceId));
    }
}
