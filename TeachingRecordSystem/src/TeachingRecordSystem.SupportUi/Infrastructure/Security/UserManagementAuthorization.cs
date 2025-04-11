using Microsoft.AspNetCore.Authorization;
using TeachingRecordSystem.SupportUi.Infrastructure.Security.AuthorizationHandlers;
using TeachingRecordSystem.SupportUi.Infrastructure.Security.AuthorizationHandlers.Legacy;
using TeachingRecordSystem.SupportUi.Infrastructure.Security.Requirements;

namespace TeachingRecordSystem.SupportUi.Infrastructure.Security;

public static class UserManagementAuthorization
{
    public static AuthorizationBuilder AddUserManagementPolicies(this AuthorizationBuilder builder)
    {
        builder.AddPolicy(
            AuthorizationPolicies.UserManagement,
            policy => policy
                .RequireAuthenticatedUser()
                .AddRequirements(new UserManagementRequirement()));

        builder.Services
            .AddSingleton<IAuthorizationHandler, UserManagementAuthorizationHandler>()
            // AuthorizationHandler for Legacy user roles, delete when existing users have been migrated to new user roles.
            .AddSingleton<IAuthorizationHandler, LegacyUserManagementAuthorizationHandler>();

        return builder;
    }
}
