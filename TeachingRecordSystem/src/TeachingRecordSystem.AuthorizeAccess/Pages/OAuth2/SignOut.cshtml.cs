using System.Security.Claims;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.AuthorizeAccess.Pages.OAuth2;

public class SignOutModel(TrsDbContext dbContext) : PageModel
{
    private OpenIddictRequest? _request;
    private AuthenticateResult? _authenticateResult;
    private ApplicationUser? _client;

    public string ServiceName => _client!.Name;
    public string ServiceUrl => new Uri(_request!.PostLogoutRedirectUri!).GetLeftPart(UriPartial.Authority);

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPost()
    {
        // We need to sign out with One Login and then complete the OIDC sign out request.
        // We do it by calling SignOutAsync with OpenIddict first, capturing the Location header from its redirect
        // then redirecting to OneLogin with that URL as the RedirectUri.

        await HttpContext.SignOutAsync(
            OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
            new AuthenticationProperties()
            {
                RedirectUri = "/"
            });

        var authenticationProperties = new AuthenticationProperties()
        {
            RedirectUri = HttpContext.Response.Headers.Location
        };
        var oneLoginIdToken = _authenticateResult!.Principal!.FindFirstValue(ClaimTypes.OneLoginIdToken)!;
        authenticationProperties.SetParameter(OpenIdConnectParameterNames.IdToken, oneLoginIdToken);

        return SignOut(authenticationProperties, _client!.OneLoginAuthenticationSchemeName!);
    }

    public async override Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        // Although the spec allows for logout requests without an id_token_hint, we require one so we can
        // a) extract the One Login ID token and;
        // b) know which authentication scheme to sign out with.

        _request = HttpContext.GetOpenIddictServerRequest() ??
            throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

        if (_request.IdTokenHint is null)
        {
            context.Result = BadRequest();
            return;
        }

        _authenticateResult = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

        string clientId = _authenticateResult.Principal!.GetAudiences().Single();
        _client = await dbContext.ApplicationUsers.SingleAsync(u => u.ClientId == clientId);

        await base.OnPageHandlerExecutionAsync(context, next);
    }
}
