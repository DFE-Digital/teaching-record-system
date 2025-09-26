using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using static IdentityModel.OidcConstants;

namespace TeachingRecordSystem.AuthorizeAccess.Pages.OidcTest;

[Authorize(AuthenticationSchemes = TestAppConfiguration.AuthenticationSchemeName)]
public class SignedInModel : PageModel
{
    private static JsonSerializerOptions _serializerOptions = new() { WriteIndented = true };

    [Display(Name = "Access token")]
    public string? AccessToken { get; set; }

    [Display(Name = "Claims")]
    public string? ClaimsJson { get; set; }

    public async Task OnGetAsync()
    {
        AccessToken = await HttpContext.GetTokenAsync(TestAppConfiguration.AuthenticationSchemeName, TokenTypes.AccessToken);

        ClaimsJson = JsonSerializer.Serialize(
            User.Claims.ToDictionary(c => c.Type, c => c.Value),
            _serializerOptions);
    }
}
