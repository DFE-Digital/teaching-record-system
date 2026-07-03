using Optional;

namespace TeachingRecordSystem.Core.Services.Users;

public record UpdateApplicationUserOptions
{
    public required Guid UserId { get; init; }
    public Option<string> Name { get; init; }
    public Option<string?> ShortName { get; init; }
    public Option<string[]?> ApiRoles { get; init; }
    public Option<bool> IsOidcClient { get; init; }
    public Option<string?> ClientId { get; init; }
    public Option<string?> ClientSecret { get; init; }
    public Option<IReadOnlyCollection<string>?> RedirectUris { get; init; }
    public Option<IReadOnlyCollection<string>?> PostLogoutRedirectUris { get; init; }
    public Option<string?> OneLoginClientId { get; init; }
    public Option<bool?> UseSharedOneLoginSigningKeys { get; init; }
    public Option<string?> OneLoginPrivateKeyPem { get; init; }
    public Option<string?> OneLoginAuthenticationSchemeName { get; init; }
    public Option<string?> OneLoginRedirectUriPath { get; init; }
    public Option<string?> OneLoginPostLogoutRedirectUriPath { get; init; }
    public Option<RecordMatchingPolicy> RecordMatchingPolicy { get; init; }
    public Option<AppContent?> AppContent { get; init; }
}
