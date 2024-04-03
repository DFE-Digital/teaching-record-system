using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TeachingRecordSystem.AuthorizeAccess.Pages.OidcTest;

public class StartModel : PageModel
{
    public void OnGet()
    {
    }

    public IActionResult OnPost() => Challenge(
        new AuthenticationProperties()
        {
            RedirectUri = Url.Page("SignedIn")
        },
        TestAppConfiguration.AuthenticationSchemeName);
}
