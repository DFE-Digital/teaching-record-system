namespace TeachingRecordSystem.Core.Events.Models;

public record ApplicationUser
{
    public required Guid UserId { get; init; }
    public required string Name { get; init; }
    public required string[]? ApiRoles { get; init; }
    public required bool IsOidcClient { get; init; }
    public required string? ClientId { get; init; }
    public required string? ClientSecret { get; init; }
    public required IReadOnlyCollection<string>? RedirectUris { get; init; }
    public required IReadOnlyCollection<string>? PostLogoutRedirectUris { get; init; }
    public required bool? UseSharedOneLoginSigningKeys { get; init; }
    public required string? OneLoginClientId { get; init; }
    public required string? OneLoginPrivateKeyPem { get; init; }
    public required string? OneLoginAuthenticationSchemeName { get; init; }
    public required string? OneLoginRedirectUriPath { get; init; }
    public required string? OneLoginPostLogoutRedirectUriPath { get; init; }
    public string? ShortName { get; init; }

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
        UseSharedOneLoginSigningKeys = user.UseSharedOneLoginSigningKeys,
        OneLoginPrivateKeyPem = user.OneLoginPrivateKeyPem,
        OneLoginAuthenticationSchemeName = user.OneLoginAuthenticationSchemeName,
        OneLoginRedirectUriPath = user.OneLoginRedirectUriPath,
        OneLoginPostLogoutRedirectUriPath = user.OneLoginPostLogoutRedirectUriPath
    };
}
