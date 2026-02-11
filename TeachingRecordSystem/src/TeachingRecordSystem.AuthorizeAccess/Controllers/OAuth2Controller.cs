using System.Diagnostics;
using System.Security.Claims;
using Dfe.Analytics.AspNetCore;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using TeachingRecordSystem.AuthorizeAccess.Infrastructure.Security;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace TeachingRecordSystem.AuthorizeAccess.Controllers;

public class OAuth2Controller(
    TrsDbContext dbContext,
    IJourneyInstanceProvider journeyInstanceProvider,
    IEventPublisher eventPublisher,
    IClock clock,
    IOpenIddictAuthorizationManager authorizationManager,
    IOpenIddictScopeManager scopeManager) : Controller
{
    [HttpGet("~/oauth2/authorize")]
    [HttpPost("~/oauth2/authorize")]
    [IgnoreAntiforgeryToken]
    [Journey(SignInJourneyCoordinator.JourneyName, Optional = true)]
    public async Task<IActionResult> AuthorizeAsync()
    {
        var request = HttpContext.GetOpenIddictServerRequest() ??
            throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

        if (!request.HasScope(CustomScopes.TeachingRecord))
        {
            return Forbid(
                authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                properties: new AuthenticationProperties(new Dictionary<string, string?>()
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidRequest,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                        $"Requests must include the {CustomScopes.TeachingRecord} scope."
                }));
        }

        var clientId = request.ClientId!;
        var client = await dbContext.ApplicationUsers.SingleAsync(u => u.ClientId == clientId);

        if (HttpContext.GetWebRequestEvent() is Dfe.Analytics.Event webRequestEvent)
        {
            webRequestEvent.Data[DfeAnalyticsEventDataKeys.ApplicationUserId] = [client.UserId.ToString()];
        }

        var childAuthenticationScheme = AuthenticationSchemes.MatchToTeachingRecord;
        var authenticateResult = await HttpContext.AuthenticateAsync(childAuthenticationScheme);

        var coordinator = journeyInstanceProvider.GetJourneyInstance(HttpContext);

        if (!authenticateResult.Succeeded)
        {
            var parameters = Request.HasFormContentType ? Request.Form.ToList() : Request.Query.ToList();

            var serviceUrl = new Uri(request.RedirectUri!).GetLeftPart(UriPartial.Authority);
            var trnToken = parameters.GroupBy(kvp => kvp.Key).FirstOrDefault(kvp => kvp.Key == "trn_token")?.Select(kvp => kvp.Value).FirstOrDefault();

            var redirectToSelf = Request.PathBase + Request.Path + QueryString.Create(parameters);

            coordinator ??= await journeyInstanceProvider.TryCreateNewInstanceAsync(
                HttpContext,
                async ctx =>
                {
                    var processContext = new ProcessContext(ProcessType.TeacherSigningIn, clock.UtcNow, SystemUser.SystemUserId);

                    await eventPublisher.PublishEventAsync(
                        new AuthorizeAccessRequestStartedEvent
                        {
                            EventId = Guid.NewGuid(),
                            ApplicationUserId = client.UserId,
                            ClientId = clientId,
                            JourneyInstanceId = ctx.InstanceId.ToString()
                        },
                        processContext);

                    redirectToSelf += $"&{JourneyInstanceId.KeyRouteValueName}={Uri.EscapeDataString(ctx.InstanceId.Key)}";

                    var state = new SignInJourneyState(
                        processContext.ProcessId,
                        redirectToSelf,
                        client.Name,
                        serviceUrl,
                        client.OneLoginAuthenticationSchemeName!,
                        client.UserId,
                        trnToken);

                    return state;
                });
            Debug.Assert(coordinator is not null);

            var authenticationProperties = new AuthenticationProperties
            {
                RedirectUri = redirectToSelf
            };

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

        var processContext = await ProcessContext.FromDbAsync(dbContext, ((SignInJourneyState)coordinator!.State).SigningInProcessId, clock.UtcNow);

        await eventPublisher.PublishEventAsync(
            new AuthorizeAccessRequestCompletedEvent { EventId = Guid.NewGuid() },
            processContext);

        return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    [HttpPost("~/oauth2/token")]
    [IgnoreAntiforgeryToken]
    [Produces("application/json")]
    public async Task<IActionResult> TokenAsync()
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
    [HttpGet("~/oauth2/userinfo")]
    [HttpPost("~/oauth2/userinfo")]
    [Produces("application/json")]
    public async Task<IActionResult> UserInfoAsync()
    {
        var subject = User.GetClaim(ClaimTypes.Subject)!;

        var claims = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            [ClaimTypes.Subject] = subject
        };

        var oneLoginUser = await dbContext.OneLoginUsers
            .Include(o => o.Person)
            .SingleAsync(u => u.Subject == subject);

        if (oneLoginUser.Person is Person p)
        {
            claims.Add(ClaimTypes.Trn, p.Trn);
        }

        if (User.HasScope(Scopes.Email))
        {
            claims.Add(ClaimTypes.Email, oneLoginUser.EmailAddress!);
        }

        claims.Add(ClaimTypes.VerifiedName, oneLoginUser.VerifiedNames!.First());
        claims.Add(ClaimTypes.VerifiedDateOfBirth, oneLoginUser.VerifiedDatesOfBirth!.First());

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
                if (claim.Subject!.HasScope(Scopes.Email))
                {
                    yield return Destinations.IdentityToken;
                }

                yield break;

            case ClaimTypes.OneLoginIdToken:
                yield return Destinations.IdentityToken;
                yield break;

            case ClaimTypes.TrsUserId:
                yield return Destinations.AccessToken;
                yield break;

            default:
                yield break;
        }
    }
}
