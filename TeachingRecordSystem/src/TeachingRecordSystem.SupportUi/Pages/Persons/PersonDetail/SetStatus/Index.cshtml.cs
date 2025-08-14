using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.SupportUi.Infrastructure.Filters;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.SetStatus;

[RequireFeatureEnabledFilterFactory(FeatureNames.ContactsMigrated)]
[Journey(JourneyNames.SetStatus), ActivatesJourney, RequireJourneyInstance]
[AllowDeactivatedPerson]
public class IndexModel(TrsLinkGenerator linkGenerator) : PageModel
{
    public JourneyInstance<SetStatusState>? JourneyInstance { get; set; }

    [FromRoute]
    public Guid PersonId { get; set; }

    [FromRoute]
    public PersonStatus TargetStatus { get; set; }

    public IActionResult OnGet() => Redirect(linkGenerator.PersonSetStatusChangeReason(PersonId, TargetStatus, JourneyInstance!.InstanceId));
}
