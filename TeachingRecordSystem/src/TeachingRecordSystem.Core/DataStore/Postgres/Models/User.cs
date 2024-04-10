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
    public required string? Email { get; set; }
    public string? AzureAdUserId { get; set; }
    public required string[] Roles { get; set; }
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

    public string[]? ApiRoles { get; set; }
    public ICollection<ApiKey> ApiKeys { get; } = null!;
    public bool IsOidcClient { get; set; }
    public string? ClientId { get; set; }
    public string? ClientSecret { get; set; }
    public List<string>? RedirectUris { get; set; }
    public List<string>? PostLogoutRedirectUris { get; set; }
    public string? OneLoginClientId { get; set; }
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

    public OpenIddictEntityFrameworkCoreApplication<Guid>? ToOpenIddictApplication()
    {
        if (!IsOidcClient)
        {
            return null;
        }

        return new OpenIddictEntityFrameworkCoreApplication<Guid>()
        {
            ApplicationType = ApplicationTypes.Web,
            ClientId = ClientId,
            ClientSecret = ClientSecret,
            ClientType = ClientTypes.Confidential,
            ConsentType = ConsentTypes.Implicit,
            DisplayName = Name,
            Id = UserId,
            Permissions = CreateJsonArray(
                Permissions.Endpoints.Authorization,
                Permissions.Endpoints.Token,
                Permissions.Endpoints.Logout,
                Permissions.GrantTypes.AuthorizationCode,
                Permissions.ResponseTypes.Code,
                Permissions.Scopes.Email,
                Permissions.Scopes.Profile,
                $"{Permissions.Prefixes.Scope}teaching_record"),
            RedirectUris = CreateJsonArray(RedirectUris!.ToArray()),
            PostLogoutRedirectUris = CreateJsonArray(PostLogoutRedirectUris!.ToArray()),
            Requirements = CreateJsonArray(Requirements.Features.ProofKeyForCodeExchange)
        };

        static string CreateJsonArray(params string[] values) => JsonSerializer.Serialize(values);
    }
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
