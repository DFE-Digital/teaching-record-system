using Microsoft.AspNetCore.Authorization;
using TeachingRecordSystem.SupportUi.Infrastructure.Security.AuthorizationHandlers;
using TeachingRecordSystem.SupportUi.Infrastructure.Security.AuthorizationHandlers.Legacy;
using TeachingRecordSystem.SupportUi.Infrastructure.Security.Requirements;

namespace TeachingRecordSystem.SupportUi.Infrastructure.Security;

public static class AdminAreaAuthorization
{
    public static AuthorizationBuilder AddAdminAreaPolicies(this AuthorizationBuilder builder)
    {
        builder.AddPolicy(
            AuthorizationPolicies.AdminOnly,
            policy => policy
                .RequireAuthenticatedUser()
                .AddRequirements(new HangFireRequirement()));

        builder.Services
            .AddSingleton<IAuthorizationHandler, HangFireAuthorizationHandler>()
            // AuthorizationHandler for Legacy user roles, delete when existing users have been migrated to new user roles.
            .AddSingleton<IAuthorizationHandler, LegacyHangFireAuthorizationHandler>();

        return builder;
    }
}
