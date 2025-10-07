using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.MergePerson;

[Journey(JourneyNames.MergePerson), ActivatesJourney, RequireJourneyInstance]
public class IndexModel(TrsLinkGenerator linkGenerator) : PageModel
{
    public JourneyInstance<MergePersonState>? JourneyInstance { get; set; }

    [FromRoute]
    public Guid PersonId { get; set; }

    public IActionResult OnGet() => Redirect(linkGenerator.PersonMergeEnterTrn(PersonId, JourneyInstance!.InstanceId));
}
