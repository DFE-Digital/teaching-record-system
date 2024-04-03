using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TeachingRecordSystem.AuthorizeAccess.Pages.OidcTest;

[Authorize(AuthenticationSchemes = TestAppConfiguration.AuthenticationSchemeName)]
public class SignedInModel : PageModel
{
    public void OnGet()
    {
    }
}
