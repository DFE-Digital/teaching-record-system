using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using static IdentityModel.OidcConstants;

namespace TeachingRecordSystem.AuthorizeAccess.Pages.OidcTest;

[Authorize(AuthenticationSchemes = TestAppConfiguration.AuthenticationSchemeName)]
public class RefreshTokenModel(IHttpContextAccessor httpContextAccessor) : PageModel
{
    private static readonly HttpClient _httpClient = new();
    private static JsonSerializerOptions _serializerOptions = new() { WriteIndented = true };

    [Display(Name = "Old access token")]
    public string? OldAccessToken { get; set; }

    [Display(Name = "Old refresh token")]
    public string? OldRefreshToken { get; set; }

    [Display(Name = "Current access token")]
    [BindProperty]
    public string? CurrentAccessToken { get; set; }

    [Display(Name = "Current refresh token")]
    [BindProperty]
    public string? CurrentRefreshToken { get; set; }

    [Display(Name = "Token response")]
    public string? TokenResponseJson { get; set; }

    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync()
    {
        CurrentAccessToken = await HttpContext.GetTokenAsync(TestAppConfiguration.AuthenticationSchemeName, TokenTypes.AccessToken);
        CurrentRefreshToken = await HttpContext.GetTokenAsync(TestAppConfiguration.AuthenticationSchemeName, TokenTypes.RefreshToken);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        OldAccessToken = CurrentAccessToken;
        OldRefreshToken = CurrentRefreshToken;

        if (string.IsNullOrEmpty(OldRefreshToken))
        {
            ErrorMessage = "No refresh token available";
            return Page();
        }

        try
        {
            var tokenEndpointUrl = $"{httpContextAccessor.HttpContext!.Request.Scheme}://{httpContextAccessor.HttpContext.Request.Host}/oauth2/token";
            var request = new HttpRequestMessage(HttpMethod.Post, tokenEndpointUrl);

            var formData = new Dictionary<string, string>
            {
                ["grant_type"] = GrantTypes.RefreshToken,
                ["refresh_token"] = OldRefreshToken,
                ["client_id"] = TestAppConfiguration.ClientId,
                ["client_secret"] = TestAppConfiguration.ClientSecret
            };

            request.Content = new FormUrlEncodedContent(formData);

            var response = await _httpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var tokenResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
                CurrentAccessToken = tokenResponse.GetProperty("access_token").GetString();

                if (tokenResponse.TryGetProperty("refresh_token", out var refreshTokenElement))
                {
                    CurrentRefreshToken = refreshTokenElement.GetString();
                }

                TokenResponseJson = JsonSerializer.Serialize(tokenResponse, _serializerOptions);

                await UpdateAuthenticationTokensAsync(CurrentAccessToken!, CurrentRefreshToken);
            }
            else
            {
                ErrorMessage = $"Token refresh failed: {response.StatusCode}";
                TokenResponseJson = responseContent;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error: {ex.Message}";
        }

        return Page();
    }

    private async Task UpdateAuthenticationTokensAsync(string newAccessToken, string? newRefreshToken)
    {
        var authenticateResult = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        if (authenticateResult.Succeeded && authenticateResult.Properties != null)
        {
            authenticateResult.Properties.UpdateTokenValue(TokenTypes.AccessToken, newAccessToken);

            if (newRefreshToken is not null)
            {
                authenticateResult.Properties.UpdateTokenValue(TokenTypes.RefreshToken, newRefreshToken);
            }

            if (authenticateResult.Properties.GetTokenValue("expires_at") is string expiresAt)
            {
                var newExpiresAt = DateTimeOffset.UtcNow.AddHours(1).ToString("o");
                authenticateResult.Properties.UpdateTokenValue("expires_at", newExpiresAt);
            }

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                authenticateResult.Principal,
                authenticateResult.Properties);
        }
    }
}
