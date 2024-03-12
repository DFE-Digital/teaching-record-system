using TeachingRecordSystem.Core.Events.Models;

namespace TeachingRecordSystem.Core.Events;

public record class ApplicationUserUpdatedEvent : EventBase
{
    public required ApplicationUser ApplicationUser { get; init; }
    public required ApplicationUser OldApplicationUser { get; init; }
    public required ApplicationUserUpdatedEventChanges Changes { get; init; }
}

public enum ApplicationUserUpdatedEventChanges
{
    None = 0,
    Name = 1 << 0,
    ApiRoles = 1 << 1,
    OneLoginClientId = 1 << 2,
    OneLoginPrivateKeyPem = 1 << 3,
    IsOidcClient = 1 << 4,
    OneLoginAuthenticationSchemeName = 1 << 5,
    OneLoginRedirectUriPath = 1 << 6,
    OneLoginPostLogoutRedirectUriPath = 1 << 7,
    ClientId = 1 << 8,
    ClientSecret = 1 << 9,
    RedirectUris = 1 << 10,
    PostLogoutRedirectUris = 1 << 11,
}
