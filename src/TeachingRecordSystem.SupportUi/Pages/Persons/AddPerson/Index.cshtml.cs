using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.AddPerson;

public class IndexModel(SupportUiLinkGenerator linkGenerator) : PageModel
{
    public IActionResult OnGet() => Redirect(linkGenerator.Persons.AddPerson.PersonalDetails());
}
