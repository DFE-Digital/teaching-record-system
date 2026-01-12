using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using OpenIddict.EntityFrameworkCore.Models;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace TeachingRecordSystem.Core.DataStore.Postgres.Models;

public abstract class UserBase
{
    public const int NameMaxLength = 200;

    public required Guid UserId { get; init; }
    public bool Active { get; set; } = true;
    public UserType UserType { get; }
    public required string Name { get; set; }
}

public class User : UserBase
{
    public const int RoleMaxLength = 50;
    public const int EmailMaxLength = 200;
    public const int AzureAdUserIdMaxLength = 100;

    public required string? Email { get; set; }
    public string? AzureAdUserId { get; set; }
    public required string? Role { get; set; }
    public Guid? DqtUserId { get; set; }
}

public class ApplicationUser : UserBase
{
    public const int AuthenticationSchemeNameMaxLength = 50;
    public const int RedirectUriPathMaxLength = 100;
    public const int ClientIdMaxLength = 50;
    public const int ClientSecretMaxLength = 200;
    public const int ClientSecretMinLength = 16;
    public const int OneLoginClientIdMaxLength = 50;
    public const string NameUniqueIndexName = "ix_users_application_user_name";
    public const string ClientIdUniqueIndexName = "ix_users_client_id";
    public const string OneLoginAuthenticationSchemeNameUniqueIndexName = "ix_users_one_login_authentication_scheme_name";

    public static Guid NpqApplicationUserGuid { get; } = new("0F18F1EC-A102-4023-843F-1CADEF3E6E14");
    public static Guid CapitaTpsImportGuid { get; } = new("14e1fa20-b364-446d-805d-699525671111");

    public string[]? ApiRoles { get; set; }
    public ICollection<ApiKey>? ApiKeys { get; }
    public bool IsOidcClient { get; set; }
    public string? ClientId { get; set; }
    public string? ClientSecret { get; set; }
    public List<string>? RedirectUris { get; set; }
    public List<string>? PostLogoutRedirectUris { get; set; }
    public string? OneLoginClientId { get; set; }
    public bool UseSharedOneLoginSigningKeys { get; set; }
    public string? OneLoginPrivateKeyPem { get; set; }
    public string? OneLoginAuthenticationSchemeName { get; set; }
    public string? OneLoginRedirectUriPath { get; set; }
    public string? OneLoginPostLogoutRedirectUriPath { get; set; }

    [MemberNotNull(
        nameof(OneLoginClientId),
        nameof(OneLoginPrivateKeyPem),
        nameof(OneLoginAuthenticationSchemeName),
        nameof(OneLoginRedirectUriPath),
        nameof(OneLoginPostLogoutRedirectUriPath))]
    public void EnsureConfiguredForOneLogin()
    {
        if (OneLoginClientId is null)
        {
            throw new InvalidOperationException($"{nameof(OneLoginClientId)} is not set.");
        }

        if (OneLoginPrivateKeyPem is null)
        {
            throw new InvalidOperationException($"{nameof(OneLoginPrivateKeyPem)} is not set.");
        }

        if (OneLoginAuthenticationSchemeName is null)
        {
            throw new InvalidOperationException($"{nameof(OneLoginAuthenticationSchemeName)} is not set.");
        }

        if (OneLoginRedirectUriPath is null)
        {
            throw new InvalidOperationException($"{nameof(OneLoginRedirectUriPath)} is not set.");
        }

        if (OneLoginPostLogoutRedirectUriPath is null)
        {
            throw new InvalidOperationException($"{nameof(OneLoginPostLogoutRedirectUriPath)} is not set.");
        }
    }

    public void PopulateOpenIddictApplication(OpenIddictEntityFrameworkCoreApplication<Guid> app)
    {
        if (!IsOidcClient)
        {
            app.Permissions = "[]";
            return;
        }

        app.ApplicationType = ApplicationTypes.Web;
        app.ClientId = ClientId;
        app.ClientSecret = ClientSecret;
        app.ClientType = ClientTypes.Confidential;
        app.ConsentType = ConsentTypes.Implicit;
        app.DisplayName = Name;
        app.Id = UserId;
        app.Permissions = CreateJsonArray(
            Permissions.Endpoints.Authorization,
            Permissions.Endpoints.Token,
            Permissions.Endpoints.EndSession,
            Permissions.GrantTypes.AuthorizationCode,
            Permissions.ResponseTypes.Code,
            Permissions.Scopes.Email,
            Permissions.Scopes.Profile,
            $"{Permissions.Prefixes.Scope}teaching_record");
        app.RedirectUris = CreateJsonArray(RedirectUris!.ToArray());
        app.PostLogoutRedirectUris = CreateJsonArray(PostLogoutRedirectUris!.ToArray());
        app.Requirements = CreateJsonArray(Requirements.Features.ProofKeyForCodeExchange);

        static string CreateJsonArray(params string[] values) => JsonSerializer.Serialize(values);
    }

    public static ApplicationUser NpqApplicationUser { get; } = new()
    {
        UserId = NpqApplicationUserGuid,
        Name = "NPQ",
        Active = true
    };

    public static ApplicationUser CapitaTpsImportUser { get; } = new()
    {
        UserId = CapitaTpsImportGuid,
        Name = "CapitaTpsImport",
        Active = true
    };
}

public class SystemUser : UserBase
{
    public static Guid SystemUserId { get; } = new Guid("a81394d1-a498-46d8-af3e-e077596ab303");
    public const string SystemUserName = "System";

    public static SystemUser Instance { get; } = new()
    {
        UserId = SystemUserId,
        Name = SystemUserName,
        Active = true
    };
}
