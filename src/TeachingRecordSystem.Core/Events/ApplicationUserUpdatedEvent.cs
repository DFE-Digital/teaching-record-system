namespace TeachingRecordSystem.Core.Events;

public record ApplicationUserUpdatedEvent : IEvent
{
    public required Guid EventId { get; init; }
    Guid[] IEvent.PersonIds => [];
    string[] IEvent.OneLoginUserSubjects => [];
    string[] IEvent.SupportTaskReferences => [];
    public required EventModels.ApplicationUser ApplicationUser { get; init; }
    public required EventModels.ApplicationUser OldApplicationUser { get; init; }
    public required ApplicationUserUpdatedEventChanges Changes { get; init; }

    public static ApplicationUserUpdatedEventChanges GetChanges(
        EventModels.ApplicationUser oldApplicationUser,
        EventModels.ApplicationUser newApplicationUser)
    {
        var apiRolesChanged = !(oldApplicationUser.ApiRoles ?? [])
            .SequenceEqualIgnoringOrder(newApplicationUser.ApiRoles ?? []);
        var redirectUrisChanged = !(newApplicationUser.RedirectUris ?? [])
            .SequenceEqualIgnoringOrder(oldApplicationUser.RedirectUris ?? []);
        var postLogoutRedirectUrisChanged = !(newApplicationUser.PostLogoutRedirectUris ?? [])
            .SequenceEqualIgnoringOrder(oldApplicationUser.PostLogoutRedirectUris ?? []);

        return ApplicationUserUpdatedEventChanges.None |
            (oldApplicationUser.Name != newApplicationUser.Name ? ApplicationUserUpdatedEventChanges.Name : 0) |
            (oldApplicationUser.ShortName != newApplicationUser.ShortName ? ApplicationUserUpdatedEventChanges.ShortName : 0) |
            (apiRolesChanged ? ApplicationUserUpdatedEventChanges.ApiRoles : 0) |
            (oldApplicationUser.IsOidcClient != newApplicationUser.IsOidcClient ? ApplicationUserUpdatedEventChanges.IsOidcClient : 0) |
            (oldApplicationUser.ClientId != newApplicationUser.ClientId ? ApplicationUserUpdatedEventChanges.ClientId : 0) |
            (oldApplicationUser.ClientSecret != newApplicationUser.ClientSecret ? ApplicationUserUpdatedEventChanges.ClientSecret : 0) |
            (redirectUrisChanged ? ApplicationUserUpdatedEventChanges.RedirectUris : 0) |
            (postLogoutRedirectUrisChanged ? ApplicationUserUpdatedEventChanges.PostLogoutRedirectUris : 0) |
            (oldApplicationUser.OneLoginClientId != newApplicationUser.OneLoginClientId ? ApplicationUserUpdatedEventChanges.OneLoginClientId : 0) |
            (oldApplicationUser.UseSharedOneLoginSigningKeys != newApplicationUser.UseSharedOneLoginSigningKeys ? ApplicationUserUpdatedEventChanges.UseSharedOneLoginSigningKeys : 0) |
            (oldApplicationUser.OneLoginPrivateKeyPem != newApplicationUser.OneLoginPrivateKeyPem ? ApplicationUserUpdatedEventChanges.OneLoginPrivateKeyPem : 0) |
            (oldApplicationUser.OneLoginAuthenticationSchemeName != newApplicationUser.OneLoginAuthenticationSchemeName ? ApplicationUserUpdatedEventChanges.OneLoginAuthenticationSchemeName : 0) |
            (oldApplicationUser.OneLoginRedirectUriPath != newApplicationUser.OneLoginRedirectUriPath ? ApplicationUserUpdatedEventChanges.OneLoginRedirectUriPath : 0) |
            (oldApplicationUser.OneLoginPostLogoutRedirectUriPath != newApplicationUser.OneLoginPostLogoutRedirectUriPath ? ApplicationUserUpdatedEventChanges.OneLoginPostLogoutRedirectUriPath : 0) |
            (oldApplicationUser.RecordMatchingPolicy != newApplicationUser.RecordMatchingPolicy ? ApplicationUserUpdatedEventChanges.RecordMatchingPolicy : 0) |
            // Coalesce null to an empty AppContent so a null <-> all-null-fields transition is not treated as a change.
            ((oldApplicationUser.AppContent ?? new AppContent()) != (newApplicationUser.AppContent ?? new AppContent()) ? ApplicationUserUpdatedEventChanges.AppContent : 0);
    }
}

[Flags]
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
    ShortName = 1 << 12,
    UseSharedOneLoginSigningKeys = 1 << 13,
    RecordMatchingPolicy = 1 << 14,
    AppContent = 1 << 15
}
