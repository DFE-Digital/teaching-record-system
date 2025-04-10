using Microsoft.AspNetCore.Authorization;
using TeachingRecordSystem.SupportUi.Infrastructure.Security.AuthorizationHandlers;
using TeachingRecordSystem.SupportUi.Infrastructure.Security.AuthorizationHandlers.Legacy;
using TeachingRecordSystem.SupportUi.Infrastructure.Security.Requirements;

namespace TeachingRecordSystem.SupportUi.Infrastructure.Security;

public static class ChangeRequestManagementAuthorization
{
    public static AuthorizationBuilder AddChangeRequestManagementPolicies(this AuthorizationBuilder builder)
    {
        builder.AddPolicy(
            AuthorizationPolicies.ChangeRequestManagement,
            policy => policy
                .RequireAuthenticatedUser()
                .AddRequirements(new ChangeManagementRequirement()));

        builder.Services
            .AddSingleton<IAuthorizationHandler, ChangeManagementAuthorizationHandler>()
            // AuthorizationHandler for Legacy user roles, delete when existing users have been migrated to new user roles.
            .AddSingleton<IAuthorizationHandler, LegacyChangeManagementAuthorizationHandler>();

        return builder;
    }
}
