using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth.Claims;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.UiCommon.Filters;
using static IdentityModel.OidcConstants;

namespace TeachingRecordSystem.AuthorizeAccess;

public static class TestAppConfiguration
{
    public const string AuthenticationSchemeName = "OidcTest";
    public const string ClientId = "test-app";
    public const string ClientSecret = "Devel0pm3ntSecr4t";
    public const string RedirectUriPath = "/test-app/callback";
    public const string PostLogoutRedirectUriPath = "/test-app/logout-callback";

    public static WebApplicationBuilder AddTestApp(this WebApplicationBuilder builder)
    {
        if (builder.Environment.IsDevelopment() || builder.Environment.IsEndToEndTests())
        {
            builder.Services.AddAuthentication()
                .AddCookie()
                .AddOpenIdConnect(AuthenticationSchemeName, options =>
                {
                    options.Authority = "https://localhost:7236";
                    options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.ClientId = ClientId;
                    options.ClientSecret = ClientSecret;
                    options.CallbackPath = RedirectUriPath;
                    options.SignedOutCallbackPath = PostLogoutRedirectUriPath;
                    options.ResponseMode = ResponseModes.Query;
                    options.ResponseType = ResponseTypes.Code;
                    options.MapInboundClaims = false;
                    options.GetClaimsFromUserInfoEndpoint = true;
                    options.SaveTokens = true;

                    options.Scope.Clear();
                    options.Scope.Add("openid");
                    options.Scope.Add("email");
                    options.Scope.Add("profile");
                    options.Scope.Add(CustomScopes.TeachingRecord);

                    options.ClaimActions.Add(new MapJsonClaimAction(ClaimTypes.OneLoginVerifiedNames));
                    options.ClaimActions.Add(new MapJsonClaimAction(ClaimTypes.OneLoginVerifiedBirthDates));

                    options.Events.OnRedirectToIdentityProvider = ctx =>
                    {
                        if (ctx.Properties.Parameters.TryGetValue("TrnToken", out var trnTokenObj) && trnTokenObj is string trnToken)
                        {
                            ctx.ProtocolMessage.SetParameter("trn_token", trnToken);
                        }

                        return Task.CompletedTask;
                    };
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
