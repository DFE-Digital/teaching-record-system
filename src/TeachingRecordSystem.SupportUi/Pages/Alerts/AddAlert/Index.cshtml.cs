using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TeachingRecordSystem.SupportUi.Pages.Alerts.AddAlert;

[Journey(JourneyNames.AddAlert), StartsJourney]
public class IndexModel(AddAlertJourneyCoordinator journey, SupportUiLinkGenerator linkGenerator) : PageModel
{
    [FromQuery]
    public Guid PersonId { get; set; }

    public IActionResult OnGet() =>
        journey.AdvanceTo(
            linkGenerator.Alerts.AddAlert.Type(journey.InstanceId),
            new PushStepOptions { SetAsFirstStep = true });
}
