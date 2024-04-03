using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TeachingRecordSystem.AuthorizeAccess.Pages.OidcTest;

[Authorize(AuthenticationSchemes = TestAppConfiguration.AuthenticationSchemeName)]
public class SignOutModel : PageModel
{
    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPost()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        return SignOut(
            new AuthenticationProperties()
            {
                RedirectUri = Url.Page("Start")
            },
            TestAppConfiguration.AuthenticationSchemeName);
    }
}
