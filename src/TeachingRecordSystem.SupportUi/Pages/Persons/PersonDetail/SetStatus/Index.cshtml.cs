using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.SetStatus;

[Journey(JourneyNames.SetStatus), StartsJourney]
[AllowDeactivatedPerson]
public class IndexModel(SetStatusJourneyCoordinator journey, SupportUiLinkGenerator linkGenerator) : PageModel
{
    [FromRoute]
    public Guid PersonId { get; set; }

    [FromRoute]
    public PersonStatus TargetStatus { get; set; }

    public IActionResult OnGet() =>
        journey.AdvanceTo(
            linkGenerator.Persons.PersonDetail.SetStatus.Reason(journey.InstanceId),
            new PushStepOptions { SetAsFirstStep = true });
}
