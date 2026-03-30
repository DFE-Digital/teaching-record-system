using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth.Claims;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.WebCommon.Filters;
using static IdentityModel.OidcConstants;

namespace TeachingRecordSystem.AuthorizeAccess;

public static class TestAppConfiguration
{
    public const string AuthenticationSchemeName = "OidcTest";
    public const string ClientId = "test-app";
    public const string ClientSecret = "Devel0pm3ntSecr4t";
    public const string RedirectUriPath = "/test-app/callback";
    public const string PostLogoutRedirectUriPath = "/test-app/logout-callback";
}

public static class DeferredTestAppConfiguration
{
    public const string AuthenticationSchemeName = "OidcTestDeferred";
    public const string ClientId = "test-app-deferred";
    public const string ClientSecret = "Devel0pm3ntSecr4tD3f3rr3d";
    public const string RedirectUriPath = "/test-app-deferred/callback";
    public const string PostLogoutRedirectUriPath = "/test-app-deferred/logout-callback";
}

public static class TestAppConfigurationExtensions
{
    public static WebApplicationBuilder AddTestApp(this WebApplicationBuilder builder)
    {
        if (builder.Environment.IsDevelopment() || builder.Environment.IsEndToEndTests())
        {
            var authBuilder = builder.Services.AddAuthentication()
                .AddCookie();

            authBuilder.AddOpenIdConnect(TestAppConfiguration.AuthenticationSchemeName, options =>
            {
                ConfigureOpenIdConnectOptions(options, TestAppConfiguration.ClientId, TestAppConfiguration.ClientSecret,
                    TestAppConfiguration.RedirectUriPath, TestAppConfiguration.PostLogoutRedirectUriPath);
            });

            authBuilder.AddOpenIdConnect(DeferredTestAppConfiguration.AuthenticationSchemeName, options =>
            {
                ConfigureOpenIdConnectOptions(options, DeferredTestAppConfiguration.ClientId, DeferredTestAppConfiguration.ClientSecret,
                    DeferredTestAppConfiguration.RedirectUriPath, DeferredTestAppConfiguration.PostLogoutRedirectUriPath);
            });
        }
        else
        {
            builder.Services.Configure<RazorPagesOptions>(options =>
                options.Conventions.AddFolderApplicationModelConvention(
                    "/OidcTest", model => model.Filters.Add(new NotFoundResourceFilter())));
        }

        return builder;
    }

    private static void ConfigureOpenIdConnectOptions(OpenIdConnectOptions options, string clientId, string clientSecret, string callbackPath, string signedOutCallbackPath)
    {
        options.Authority = "https://localhost:7236";
        options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.ClientId = clientId;
        options.ClientSecret = clientSecret;
        options.CallbackPath = callbackPath;
        options.SignedOutCallbackPath = signedOutCallbackPath;
        options.ResponseMode = ResponseModes.Query;
        options.ResponseType = ResponseTypes.Code;
        options.MapInboundClaims = false;
        options.GetClaimsFromUserInfoEndpoint = true;
        options.SaveTokens = true;

        options.Scope.Clear();
        options.Scope.Add("openid");
        options.Scope.Add("email");
        options.Scope.Add("profile");
        options.Scope.Add("offline_access");
        options.Scope.Add(CustomScopes.TeachingRecord);

        options.ClaimActions.Add(new MapJsonClaimAction(ClaimTypes.VerifiedName));
        options.ClaimActions.Add(new MapJsonClaimAction(ClaimTypes.VerifiedDateOfBirth));

        options.Events.OnRedirectToIdentityProvider = ctx =>
        {
            if (ctx.Properties.Parameters.TryGetValue("TrnToken", out var trnTokenObj) && trnTokenObj is string trnToken)
            {
                ctx.ProtocolMessage.SetParameter("trn_token", trnToken);
            }

            return Task.CompletedTask;
        };

        options.Events.OnTokenResponseReceived = ctx =>
        {
            // Set a breakpoint here to inspect tokens
            // ctx.TokenEndpointResponse contains access_token, refresh_token, id_token, etc.
            var accessToken = ctx.TokenEndpointResponse.AccessToken;
            var refreshToken = ctx.TokenEndpointResponse.RefreshToken;
            var idToken = ctx.TokenEndpointResponse.IdToken;

            return Task.CompletedTask;
        };
    }
}

file class MapJsonClaimAction(string claimType) : ClaimAction(claimType, JsonClaimValueTypes.Json)
{
    public override void Run(JsonElement userData, ClaimsIdentity identity, string issuer)
    {
        if (userData.TryGetProperty(ClaimType, out var element))
        {
            identity.AddClaim(new Claim(ClaimType, element.ToString(), valueType: ""));
        }
    }
}
