using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.SupportUi.Infrastructure.Filters;

namespace TeachingRecordSystem.SupportUi.Pages;

[RequireFeatureEnabledFilterFactory(FeatureNames.SwitchRoles)]
public class MyRolesModel : PageModel
{
    public IReadOnlyCollection<string> AllRoles => UserRoles.All;

    [Display(Name = "My roles")]
    [BindProperty]
    public string[]? MyRoles { get; set; }

    public void OnGet()
    {
        MyRoles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray();
    }

    public async Task<IActionResult> OnPost()
    {
        var newRoles = MyRoles ?? [];

        var identity = (ClaimsIdentity)User.Identity!;

        // Remove any existing Role claims
        foreach (var claim in identity.Claims.Where(c => c.Type == ClaimTypes.Role).ToArray())
        {
            identity.RemoveClaim(claim);
        }

        // Add a Role claim for every selected role
        foreach (var role in newRoles)
        {
            identity.AddClaim(new Claim(ClaimTypes.Role, role));
        }

        var authenticateResult = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        var properties = authenticateResult.Properties!;
        properties.RedirectUri = Url.Page("MyRoles");

        TempData.SetFlashSuccess("Roles updated");

        return SignIn(
            User,
            properties,
            authenticationScheme: CookieAuthenticationDefaults.AuthenticationScheme);
    }
}
