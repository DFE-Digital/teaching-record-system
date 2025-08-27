using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.Merge;

[Journey(JourneyNames.MergePerson), ActivatesJourney, RequireJourneyInstance]
public class IndexModel(TrsLinkGenerator linkGenerator) : PageModel
{
    public JourneyInstance<MergeState>? JourneyInstance { get; set; }

    [FromRoute]
    public Guid PersonId { get; set; }

    public IActionResult OnGet() => Redirect(linkGenerator.PersonMergeEnterTrn(PersonId, JourneyInstance!.InstanceId));
}
