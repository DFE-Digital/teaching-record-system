using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Legacy;
using TeachingRecordSystem.SupportUi.Infrastructure.Filters;

namespace TeachingRecordSystem.SupportUi.Pages;

[RequireFeatureEnabledFilterFactory(FeatureNames.SwitchRoles)]
public class MyRolesModel(TrsDbContext dbContext) : PageModel
{
    private string[]? _dbRoles;

    public IReadOnlyCollection<string>? AvailableRoles { get; set; }

    [Display(Name = "My roles")]
    [BindProperty]
    public string[]? MyRoles { get; set; }

    public void OnGet()
    {
        MyRoles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray();
    }

    public Task<IActionResult> OnPostAsync()
    {
        var newRoles = (MyRoles ?? []).Intersect(AvailableRoles!);
        return SetRolesAsync(newRoles);
    }

    public Task<IActionResult> OnPostResetAsync()
    {
        return SetRolesAsync(_dbRoles!);
    }

    private async Task<IActionResult> SetRolesAsync(IEnumerable<string> roles)
    {
        var identity = (ClaimsIdentity)User.Identity!;

        // Remove any existing Role claims
        foreach (var claim in identity.Claims.Where(c => c.Type == ClaimTypes.Role).ToArray())
        {
            identity.RemoveClaim(claim);
        }

        // Add a Role claim for every selected role
        foreach (var role in roles)
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

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        _dbRoles = (await dbContext.Users.SingleAsync(u => u.UserId == User.GetUserId())).Roles;

        // Only make Administrator available to actual admins
        AvailableRoles = (_dbRoles.Contains(LegacyUserRoles.Administrator) ?
            LegacyUserRoles.All :
            LegacyUserRoles.All.Except([LegacyUserRoles.Administrator])).ToArray();

        await base.OnPageHandlerExecutionAsync(context, next);
    }
}
