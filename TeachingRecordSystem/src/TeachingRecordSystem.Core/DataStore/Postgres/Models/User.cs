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
    public const int OneLoginClientIdMaxLength = 50;
    public const string NameUniqueIndexName = "ix_users_application_user_name";
    public const string OneLoginAuthenticationSchemeNameUniqueIndexName = "ix_users_one_login_authentication_scheme_name";

    public required string[] ApiRoles { get; set; }
    public ICollection<ApiKey> ApiKeys { get; } = null!;
    public bool IsOidcClient { get; set; }
    public string? OneLoginClientId { get; set; }
    public string? OneLoginPrivateKeyPem { get; set; }
    public string? OneLoginAuthenticationSchemeName { get; set; }
    public string? OneLoginRedirectUriPath { get; set; }
    public string? OneLoginPostLogoutRedirectUriPath { get; set; }
}

public class SystemUser : UserBase
{
    public static Guid SystemUserId { get; } = new Guid("a81394d1-a498-46d8-af3e-e077596ab303");
}
