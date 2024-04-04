using System.Security.Claims;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using TeachingRecordSystem.AuthorizeAccess.Infrastructure.Security;
using TeachingRecordSystem.Core.DataStore.Postgres;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace TeachingRecordSystem.AuthorizeAccess.Controllers;

public class OidcController(
    TrsDbContext dbContext,
    IOpenIddictAuthorizationManager authorizationManager,
    IOpenIddictScopeManager scopeManager) : Controller
{
    [HttpGet("~/connect/authorize")]
    [HttpPost("~/connect/authorize")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Authorize()
    {
        var request = HttpContext.GetOpenIddictServerRequest() ??
            throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

        var clientId = request.ClientId!;
        var client = await dbContext.ApplicationUsers.SingleAsync(u => u.ClientId == clientId);

        var childAuthenticationScheme = AuthenticationSchemes.MatchToTeachingRecord;
        var authenticateResult = await HttpContext.AuthenticateAsync(childAuthenticationScheme);

        if (!authenticateResult.Succeeded)
        {
            var parameters = Request.HasFormContentType ? Request.Form.ToList() : Request.Query.ToList();

            var serviceUrl = new Uri(request.RedirectUri!).GetLeftPart(UriPartial.Authority);

            var authenticationProperties = new AuthenticationProperties()
            {
                RedirectUri = Request.PathBase + Request.Path + QueryString.Create(parameters)
            };
            authenticationProperties.Items.Add(MatchToTeachingRecordAuthenticationHandler.AuthenticationPropertiesItemKeys.OneLoginAuthenticationScheme, client.OneLoginAuthenticationSchemeName);
            authenticationProperties.Items.Add(MatchToTeachingRecordAuthenticationHandler.AuthenticationPropertiesItemKeys.ServiceName, client.Name);
            authenticationProperties.Items.Add(MatchToTeachingRecordAuthenticationHandler.AuthenticationPropertiesItemKeys.ServiceUrl, serviceUrl);

            return Challenge(authenticationProperties, childAuthenticationScheme);
        }

        var user = authenticateResult.Principal;
        var subject = user.FindFirstValue(ClaimTypes.Subject) ??
            throw new InvalidOperationException($"Principal does not contain a '{ClaimTypes.Subject}' claim.");

        var authorizations = await authorizationManager.FindAsync(
            subject: subject,
            client: client.UserId.ToString(),
            status: Statuses.Valid,
            type: AuthorizationTypes.Permanent,
            scopes: request.GetScopes()).ToListAsync();

        var identity = new ClaimsIdentity(
            claims: user.Claims,
            authenticationType: TokenValidationParameters.DefaultAuthenticationType,
            nameType: ClaimTypes.Subject,
            roleType: null);

        identity.SetScopes(request.GetScopes());
        identity.SetResources(await scopeManager.ListResourcesAsync(identity.GetScopes()).ToListAsync());

        var authorization = authorizations.LastOrDefault();
        authorization ??= await authorizationManager.CreateAsync(
            identity: identity,
            subject: subject,
            client: client.UserId.ToString(),
            type: AuthorizationTypes.Permanent,
            scopes: identity.GetScopes());

        identity.SetAuthorizationId(await authorizationManager.GetIdAsync(authorization));
        identity.SetDestinations(GetDestinations);

        return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    [HttpPost("~/connect/token")]
    [IgnoreAntiforgeryToken]
    [Produces("application/json")]
    public async Task<IActionResult> Token()
    {
        var request = HttpContext.GetOpenIddictServerRequest() ??
            throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

        if (request.IsAuthorizationCodeGrantType() || request.IsRefreshTokenGrantType())
        {
            var result = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

            var identity = new ClaimsIdentity(
                result.Principal!.Claims,
                authenticationType: TokenValidationParameters.DefaultAuthenticationType,
                nameType: ClaimTypes.Subject,
                roleType: null);

            identity.SetDestinations(GetDestinations);

            return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        throw new InvalidOperationException("The specified grant type is not supported.");
    }

    [Authorize(AuthenticationSchemes = OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)]
    [HttpGet("~/connect/userinfo")]
    [HttpPost("~/connect/userinfo")]
    [Produces("application/json")]
    public IActionResult UserInfo()
    {
        var claims = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            [ClaimTypes.Subject] = User.GetClaim(ClaimTypes.Subject)!,
            [ClaimTypes.Trn] = User.GetClaim(ClaimTypes.Trn)!
        };

        if (User.HasScope(Scopes.Email))
        {
            claims[ClaimTypes.Email] = User.GetClaim(ClaimTypes.Email)!;
        }

        return Ok(claims);
    }

    private static IEnumerable<string> GetDestinations(Claim claim)
    {
        switch (claim.Type)
        {
            case ClaimTypes.Subject:
            case ClaimTypes.Trn:
                yield return Destinations.AccessToken;
                yield return Destinations.IdentityToken;
                yield break;

            case ClaimTypes.Email:
                if (claim.Subject!.HasScope(Scopes.Profile))
                {
                    yield return Destinations.IdentityToken;
                }

                yield break;

            case ClaimTypes.OneLoginIdToken:
                yield return Destinations.IdentityToken;
                yield break;

            default:
                yield break;
        }
    }
}
