using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TeachingRecordSystem.AuthorizeAccess.Pages.OidcTest;

public class StartModel : PageModel
{
    [BindProperty]
    [Display(Name = "TRN token")]
    public string? TrnToken { get; set; }

    public void OnGet()
    {
    }

    public IActionResult OnPost() => Challenge(
        new AuthenticationProperties()
        {
            RedirectUri = Url.Page("SignedIn"),
            Parameters =
            {
                { "TrnToken", TrnToken }
            }
        },
        TestAppConfiguration.AuthenticationSchemeName);
}
