using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TeachingRecordSystem.SupportUi.Pages;

public class SignOutModel : PageModel
{
    public IActionResult OnGet() => Page();

    public IActionResult OnPost() => SignOut(OpenIdConnectDefaults.AuthenticationScheme);
}
