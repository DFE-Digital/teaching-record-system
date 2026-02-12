using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.ConnectOneLogin;

[Journey(JourneyNames.ConnectOneLogin), RequireJourneyInstance]
public class ReasonModel(SupportUiLinkGenerator linkGenerator) : PageModel
{
    public JourneyInstance<ConnectOneLoginState>? JourneyInstance { get; set; }

    [FromRoute]
    public Guid PersonId { get; set; }

    public string BackLink => linkGenerator.Persons.PersonDetail.ConnectOneLogin.Match(PersonId, JourneyInstance!.InstanceId);

    public void OnGet()
    {
        // TODO: Implement reason page
    }

    public IActionResult OnPost()
    {
        // TODO: Implement reason page
        return Redirect(linkGenerator.Persons.PersonDetail.Index(PersonId));
    }
}
