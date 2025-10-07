using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.AddPerson;

public class IndexModel(TrsLinkGenerator linkGenerator) : PageModel
{
    public IActionResult OnGet() => Redirect(linkGenerator.PersonCreatePersonalDetails());
}
