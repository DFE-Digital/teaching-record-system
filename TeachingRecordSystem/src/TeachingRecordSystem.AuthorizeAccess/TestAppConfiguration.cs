using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.AuthorizeAccess.Infrastructure.Filters;
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
                    options.SaveTokens = true;
                    options.Scope.Clear();
                    options.Scope.Add("openid");
                    options.Scope.Add("email");
                    options.Scope.Add("profile");
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
