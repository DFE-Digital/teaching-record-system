using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TeachingRecordSystem.SupportUi.Pages.Mqs.AddMq;

[Journey(JourneyNames.AddMq), StartsJourney]
public class IndexModel(AddMqJourneyCoordinator journey, SupportUiLinkGenerator linkGenerator) : PageModel
{
    [FromQuery]
    public Guid PersonId { get; set; }

    public IActionResult OnGet() =>
        journey.AdvanceTo(
            linkGenerator.Mqs.AddMq.Provider(journey.InstanceId),
            new PushStepOptions { SetAsFirstStep = true });
}
