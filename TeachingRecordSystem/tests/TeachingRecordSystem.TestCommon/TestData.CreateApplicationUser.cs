using System.Security.Cryptography;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.TestCommon;

public partial class TestData
{
    public async Task<ApplicationUser> CreateApplicationUser(
        string? name = null,
        string[]? apiRoles = null,
        bool? isOidcClient = false)
    {
        var user = await WithDbContext(async dbContext =>
        {
            name ??= GenerateApplicationUserName();
            apiRoles ??= [];
            isOidcClient ??= false;
            string? clientId = null;
            string? clientSecret = null;
            List<string>? redirectUris = null;
            List<string>? postLogoutRedirectUris = null;
            string? oneLoginClientId = null;
            string? oneLoginPrivateKeyPem = null;
            string? oneLoginAuthenticationSchemeName = null;
            string? oneLoginRedirectUriPath = null;
            string? oneLoginPostLogoutRedirectUriPath = null;

            if (isOidcClient == true)
            {
                clientId = Guid.NewGuid().ToString();
                clientSecret = Guid.NewGuid().ToString();
                redirectUris = ["https://localhost:3000/callback"];
                postLogoutRedirectUris = ["https://localhost:3000/logout-callback"];
                oneLoginClientId = Guid.NewGuid().ToString();
                oneLoginPrivateKeyPem = GeneratePrivateKeyPem();
                oneLoginAuthenticationSchemeName = Guid.NewGuid().ToString();
                oneLoginRedirectUriPath = $"/_onelogin/{oneLoginAuthenticationSchemeName}/callback";
                oneLoginPostLogoutRedirectUriPath = $"/_onelogin/{oneLoginAuthenticationSchemeName}/logout-callback";
            }

            var user = new ApplicationUser()
            {
                Name = name,
                UserId = Guid.NewGuid(),
                ApiRoles = apiRoles,
                IsOidcClient = isOidcClient.Value,
                ClientId = clientId,
                ClientSecret = clientSecret,
                RedirectUris = redirectUris,
                PostLogoutRedirectUris = postLogoutRedirectUris,
                OneLoginClientId = oneLoginClientId,
                OneLoginPrivateKeyPem = oneLoginPrivateKeyPem,
                OneLoginAuthenticationSchemeName = oneLoginAuthenticationSchemeName,
                OneLoginRedirectUriPath = oneLoginRedirectUriPath,
                OneLoginPostLogoutRedirectUriPath = oneLoginPostLogoutRedirectUriPath
            };

            dbContext.ApplicationUsers.Add(user);

            var @event = new ApplicationUserCreatedEvent()
            {
                EventId = Guid.NewGuid(),
                RaisedBy = SystemUser.SystemUserId,
                CreatedUtc = Clock.UtcNow,
                ApplicationUser = EventModels.ApplicationUser.FromModel(user)
            };
            dbContext.AddEvent(@event);

            await dbContext.SaveChangesAsync();

            return user;
        });

        return user;
    }

    public static string GeneratePrivateKeyPem()
    {
        using var rsa = RSA.Create();
        return rsa.ExportRSAPrivateKeyPem();
    }
}
