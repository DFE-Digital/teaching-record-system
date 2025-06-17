using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.Create;

public partial class IndexModel() : PageModel
{
    public IActionResult OnGet()
    {
        return Page();
    }
}
