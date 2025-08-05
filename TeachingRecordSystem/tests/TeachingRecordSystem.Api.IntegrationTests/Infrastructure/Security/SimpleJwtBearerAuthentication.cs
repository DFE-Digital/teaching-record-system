using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace TeachingRecordSystem.Api.IntegrationTests.Infrastructure.Security;

public class SimpleJwtBearerAuthentication(
    IOptionsMonitor<SimpleJwtBearerAuthenticationOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder) : AuthenticationHandler<SimpleJwtBearerAuthenticationOptions>(options, logger, encoder)
{
    private JwtSecurityTokenHandler _handler = null!;

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        return Task.FromResult(Core());

        AuthenticateResult Core()
        {
            var authorizationHeader = Context.Request.Headers.Authorization.ToString();
            if (!authorizationHeader.StartsWith("Bearer "))
            {
                return AuthenticateResult.NoResult();
            }

            var token = authorizationHeader[("Bearer ").Length..];

            ClaimsPrincipal principal;
            try
            {
                principal = _handler.ValidateToken(
                    token,
                    new TokenValidationParameters()
                    {
                        ValidateAudience = false,
                        ValidateIssuer = false,
                        RequireExpirationTime = false,
                        IssuerSigningKey = Options.IssuerSigningKey
                    },
                    out _);
            }
            catch (SecurityTokenInvalidIssuerException ex)
            {
                return AuthenticateResult.Fail(ex);
            }

            if (principal is null)
            {
                return AuthenticateResult.Fail("Token is not valid.");
            }

            var ticket = new AuthenticationTicket(principal, Scheme.Name);
            return AuthenticateResult.Success(ticket);
        }
    }

    protected override Task InitializeHandlerAsync()
    {
        _handler = new JwtSecurityTokenHandler()
        {
            MapInboundClaims = false
        };

        return Task.CompletedTask;
    }
}

public class SimpleJwtBearerAuthenticationOptions : AuthenticationSchemeOptions
{
    public SecurityKey? IssuerSigningKey { get; set; }
}
