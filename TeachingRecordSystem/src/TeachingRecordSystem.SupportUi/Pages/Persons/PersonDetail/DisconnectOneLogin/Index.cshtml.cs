using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.DisconnectOneLogin;

[Journey(JourneyNames.DisconnectOneLogin), ActivatesJourney, RequireJourneyInstance]
public class Index : PageModel
{
    public void OnGet()
    {

    }
}
