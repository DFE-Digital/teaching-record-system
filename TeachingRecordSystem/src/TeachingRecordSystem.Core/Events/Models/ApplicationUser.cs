namespace TeachingRecordSystem.Core.Events.Models;

public record ApplicationUser
{
    public required Guid UserId { get; init; }
    public required string Name { get; init; }
    public string[]? ApiRoles { get; init; }
    public bool IsOidcClient { get; init; }
    public string? ClientId { get; init; }
    public string? ClientSecret { get; init; }
    public IReadOnlyCollection<string>? RedirectUris { get; init; }
    public IReadOnlyCollection<string>? PostLogoutRedirectUris { get; init; }
    public string? OneLoginClientId { get; init; }
    public string? OneLoginPrivateKeyPem { get; init; }
    public string? OneLoginAuthenticationSchemeName { get; init; }
    public string? OneLoginRedirectUriPath { get; init; }
    public string? OneLoginPostLogoutRedirectUriPath { get; init; }

    public static ApplicationUser FromModel(DataStore.Postgres.Models.ApplicationUser user) => new()
    {
        UserId = user.UserId,
        Name = user.Name,
        ApiRoles = user.ApiRoles,
        IsOidcClient = user.IsOidcClient,
        ClientId = user.ClientId,
        ClientSecret = user.ClientSecret,
        RedirectUris = user.RedirectUris,
        PostLogoutRedirectUris = user.PostLogoutRedirectUris,
        OneLoginClientId = user.OneLoginClientId,
        OneLoginPrivateKeyPem = user.OneLoginPrivateKeyPem,
        OneLoginAuthenticationSchemeName = user.OneLoginAuthenticationSchemeName,
        OneLoginRedirectUriPath = user.OneLoginRedirectUriPath,
        OneLoginPostLogoutRedirectUriPath = user.OneLoginPostLogoutRedirectUriPath
    };
}
