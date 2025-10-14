using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.SetStatus;

[Journey(JourneyNames.SetStatus), ActivatesJourney, RequireJourneyInstance]
[AllowDeactivatedPerson]
public class IndexModel(SupportUiLinkGenerator linkGenerator) : PageModel
{
    public JourneyInstance<SetStatusState>? JourneyInstance { get; set; }

    [FromRoute]
    public Guid PersonId { get; set; }

    [FromRoute]
    public PersonStatus TargetStatus { get; set; }

    public IActionResult OnGet() => Redirect(linkGenerator.Persons.PersonDetail.SetStatus.Reason(PersonId, TargetStatus, JourneyInstance!.InstanceId));
}
