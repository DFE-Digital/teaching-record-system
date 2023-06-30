using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TeachingRecordSystem.SupportUi.Pages;

[AllowAnonymous]
public class SignedOutModel : PageModel
{
    public IActionResult OnGet([FromServices] TrsLinkGenerator linkGenerator) =>
        User.Identity?.IsAuthenticated == true ? Redirect(linkGenerator.Index()) : Page();
}
