using Microsoft.AspNetCore.Authorization;
using TeachingRecordSystem.SupportUi.Infrastructure.Security.AuthorizationHandlers;
using TeachingRecordSystem.SupportUi.Infrastructure.Security.AuthorizationHandlers.Legacy;
using TeachingRecordSystem.SupportUi.Infrastructure.Security.Requirements;

namespace TeachingRecordSystem.SupportUi.Infrastructure.Security;

public static class SupportTasksAuthorization
{
    public static AuthorizationBuilder AddChangeRequestManagementPolicies(this AuthorizationBuilder builder)
    {
        builder.AddPolicy(
            AuthorizationPolicies.SupportTasksView,
            policy => policy
                .RequireAuthenticatedUser()
                .AddRequirements(new SupportTasksViewRequirement()));

        builder.AddPolicy(
            AuthorizationPolicies.SupportTasksEdit,
            policy => policy
                .RequireAuthenticatedUser()
                .AddRequirements(new ChangeManagementRequirement()));

        builder.Services
            .AddSingleton<IAuthorizationHandler, SupportTasksViewAuthorizationHandler>()
            .AddSingleton<IAuthorizationHandler, ChangeManagementAuthorizationHandler>()
            // AuthorizationHandler for Legacy user roles, delete when existing users have been migrated to new user roles.
            .AddSingleton<IAuthorizationHandler, LegacySupportTasksViewAuthorizationHandler>()
            .AddSingleton<IAuthorizationHandler, LegacyChangeManagementAuthorizationHandler>();

        return builder;
    }
}
