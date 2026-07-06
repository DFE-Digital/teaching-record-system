using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.Services.Users;

public class UserService(TrsDbContext dbContext, IEventPublisher eventPublisher)
{
    public async Task<User> CreateUserAsync(CreateUserOptions options, ProcessContext processContext)
    {
        var user = new User
        {
            UserId = Guid.NewGuid(),
            Active = true,
            Name = options.Name,
            Email = options.Email,
            AzureAdUserId = options.AzureAdUserId,
            Role = options.Role
        };

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        await eventPublisher.PublishSingleEventAsync(
            new UserAddedEvent
            {
                EventId = Guid.NewGuid(),
                User = EventModels.User.FromModel(user)
            },
            processContext);

        return user;
    }

    public async Task<UserUpdatedEventChanges> UpdateUserAsync(UpdateUserOptions options, ProcessContext processContext)
    {
        var user = await dbContext.Users.FindOrThrowAsync(options.UserId);

        var oldUser = EventModels.User.FromModel(user);

        user.Name = options.Name;
        user.Role = options.Role;

        var changes = UserUpdatedEventChanges.None |
            (user.Name != oldUser.Name ? UserUpdatedEventChanges.Name : 0) |
            (user.Role != oldUser.Role ? UserUpdatedEventChanges.Roles : 0);

        if (changes == UserUpdatedEventChanges.None)
        {
            return changes;
        }

        await dbContext.SaveChangesAsync();

        await eventPublisher.PublishSingleEventAsync(
            new UserUpdatedEvent
            {
                EventId = Guid.NewGuid(),
                User = EventModels.User.FromModel(user),
                Changes = changes
            },
            processContext);

        return changes;
    }

    public async Task ActivateUserAsync(Guid userId, ProcessContext processContext)
    {
        var user = await dbContext.Users.FindOrThrowAsync(userId);

        if (user.Active)
        {
            throw new InvalidOperationException("User is already active.");
        }

        user.Active = true;
        await dbContext.SaveChangesAsync();

        await eventPublisher.PublishSingleEventAsync(
            new UserActivatedEvent
            {
                EventId = Guid.NewGuid(),
                User = EventModels.User.FromModel(user)
            },
            processContext);
    }

    public async Task DeactivateUserAsync(DeactivateUserOptions options, ProcessContext processContext)
    {
        var user = await dbContext.Users.FindOrThrowAsync(options.UserId);

        if (!user.Active)
        {
            throw new InvalidOperationException("User is not active.");
        }

        user.Active = false;
        await dbContext.SaveChangesAsync();

        await eventPublisher.PublishSingleEventAsync(
            new UserDeactivatedEvent
            {
                EventId = Guid.NewGuid(),
                User = EventModels.User.FromModel(user),
                DeactivatedReason = options.DeactivatedReason,
                DeactivatedReasonDetail = options.DeactivatedReasonDetail,
                EvidenceFileId = options.EvidenceFileId
            },
            processContext);
    }

    public async Task<ApplicationUser> CreateApplicationUserAsync(CreateApplicationUserOptions options, ProcessContext processContext)
    {
        var applicationUser = new ApplicationUser
        {
            UserId = Guid.NewGuid(),
            Name = options.Name,
            ShortName = options.ShortName,
            ApiRoles = []
        };

        dbContext.ApplicationUsers.Add(applicationUser);
        await dbContext.SaveChangesAsync();

        await eventPublisher.PublishSingleEventAsync(
            new ApplicationUserCreatedEvent
            {
                EventId = Guid.NewGuid(),
                ApplicationUser = EventModels.ApplicationUser.FromModel(applicationUser)
            },
            processContext);

        return applicationUser;
    }

    public async Task<ApplicationUserUpdatedEventChanges> UpdateApplicationUserAsync(UpdateApplicationUserOptions options, ProcessContext processContext)
    {
        var applicationUser = await dbContext.ApplicationUsers.FindOrThrowAsync(options.UserId);

        var oldApplicationUser = EventModels.ApplicationUser.FromModel(applicationUser);

        options.Name.MatchSome(name => applicationUser.Name = name);
        options.ShortName.MatchSome(shortName => applicationUser.ShortName = shortName);
        options.ApiRoles.MatchSome(apiRoles => applicationUser.ApiRoles = apiRoles);
        options.IsOidcClient.MatchSome(isOidcClient => applicationUser.IsOidcClient = isOidcClient);
        options.ClientId.MatchSome(clientId => applicationUser.ClientId = clientId);
        options.ClientSecret.MatchSome(clientSecret => applicationUser.ClientSecret = clientSecret);
        options.RedirectUris.MatchSome(redirectUris => applicationUser.RedirectUris = redirectUris?.ToList());
        options.PostLogoutRedirectUris.MatchSome(postLogoutRedirectUris => applicationUser.PostLogoutRedirectUris = postLogoutRedirectUris?.ToList());
        options.OneLoginClientId.MatchSome(oneLoginClientId => applicationUser.OneLoginClientId = oneLoginClientId);
        options.UseSharedOneLoginSigningKeys.MatchSome(useSharedOneLoginSigningKeys => applicationUser.UseSharedOneLoginSigningKeys = useSharedOneLoginSigningKeys);
        options.OneLoginPrivateKeyPem.MatchSome(oneLoginPrivateKeyPem => applicationUser.OneLoginPrivateKeyPem = oneLoginPrivateKeyPem);
        options.OneLoginAuthenticationSchemeName.MatchSome(oneLoginAuthenticationSchemeName => applicationUser.OneLoginAuthenticationSchemeName = oneLoginAuthenticationSchemeName);
        options.OneLoginRedirectUriPath.MatchSome(oneLoginRedirectUriPath => applicationUser.OneLoginRedirectUriPath = oneLoginRedirectUriPath);
        options.OneLoginPostLogoutRedirectUriPath.MatchSome(oneLoginPostLogoutRedirectUriPath => applicationUser.OneLoginPostLogoutRedirectUriPath = oneLoginPostLogoutRedirectUriPath);
        options.RecordMatchingPolicy.MatchSome(recordMatchingPolicy => applicationUser.RecordMatchingPolicy = recordMatchingPolicy);
        options.AppContent.MatchSome(appContent => applicationUser.AppContent = appContent);

        var newApplicationUser = EventModels.ApplicationUser.FromModel(applicationUser);
        var changes = ApplicationUserUpdatedEvent.GetChanges(oldApplicationUser, newApplicationUser);

        if (changes == ApplicationUserUpdatedEventChanges.None)
        {
            return changes;
        }

        await dbContext.SaveChangesAsync();

        await eventPublisher.PublishSingleEventAsync(
            new ApplicationUserUpdatedEvent
            {
                EventId = Guid.NewGuid(),
                ApplicationUser = newApplicationUser,
                OldApplicationUser = oldApplicationUser,
                Changes = changes
            },
            processContext);

        // Notify TeacherAuth about changes to the application user
        await dbContext.Database.ExecuteSqlRawAsync($"NOTIFY {ChannelNames.OneLoginClient}");

        return changes;
    }

    public async Task<ApiKey> CreateApiKeyAsync(CreateApiKeyOptions options, ProcessContext processContext)
    {
        var apiKey = new ApiKey
        {
            ApiKeyId = Guid.NewGuid(),
            CreatedOn = processContext.Now,
            UpdatedOn = processContext.Now,
            ApplicationUserId = options.ApplicationUserId,
            Key = options.Key,
            Expires = null
        };

        dbContext.ApiKeys.Add(apiKey);
        await dbContext.SaveChangesAsync();

        await eventPublisher.PublishSingleEventAsync(
            new ApiKeyCreatedEvent
            {
                EventId = Guid.NewGuid(),
                ApiKey = EventModels.ApiKey.FromModel(apiKey)
            },
            processContext);

        return apiKey;
    }

    public async Task<ApiKeyUpdatedEventChanges> UpdateApiKeyAsync(UpdateApiKeyOptions options, ProcessContext processContext)
    {
        var apiKey = await dbContext.ApiKeys.FindOrThrowAsync(options.ApiKeyId);

        var oldApiKey = EventModels.ApiKey.FromModel(apiKey);

        options.Expires.MatchSome(expires => apiKey.Expires = expires);

        var changes = ApiKeyUpdatedEventChanges.None |
            (apiKey.Expires != oldApiKey.Expires ? ApiKeyUpdatedEventChanges.Expires : 0);

        if (changes == ApiKeyUpdatedEventChanges.None)
        {
            return changes;
        }

        await dbContext.SaveChangesAsync();

        await eventPublisher.PublishSingleEventAsync(
            new ApiKeyUpdatedEvent
            {
                EventId = Guid.NewGuid(),
                ApiKey = EventModels.ApiKey.FromModel(apiKey),
                OldApiKey = oldApiKey,
                Changes = changes
            },
            processContext);

        return changes;
    }
}
