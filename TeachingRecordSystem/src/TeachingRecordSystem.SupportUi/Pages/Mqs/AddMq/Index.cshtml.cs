using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TeachingRecordSystem.SupportUi.Pages.Mqs.AddMq;

[Journey(JourneyNames.AddMq), ActivatesJourney, RequireJourneyInstance]
public class IndexModel(TrsLinkGenerator linkGenerator) : PageModel
{
    public JourneyInstance<AddMqState>? JourneyInstance { get; set; }

    [FromQuery]
    public Guid PersonId { get; set; }

    public IActionResult OnGet() => Redirect(linkGenerator.MqAddProvider(PersonId, JourneyInstance!.InstanceId));
}
