using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TeachingRecordSystem.SupportUi.Pages.Alerts.AddAlert;

[Journey(JourneyNames.AddAlert), ActivatesJourney, RequireJourneyInstance]
public class IndexModel(SupportUiLinkGenerator linkGenerator) : PageModel
{
    public JourneyInstance<AddAlertState>? JourneyInstance { get; set; }

    [FromQuery]
    public Guid PersonId { get; set; }

    public Task<IActionResult> OnGetAsync() =>
        JourneyInstance!.UpdateStateAndRedirectToNextStepAsync(_ => { }, linkGenerator.Alerts.AddAlert.Type(PersonId, JourneyInstance!.InstanceId));
}
