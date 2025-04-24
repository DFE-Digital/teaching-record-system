using Microsoft.AspNetCore.Authorization;
using TeachingRecordSystem.SupportUi.Infrastructure.Security.AuthorizationHandlers;
using TeachingRecordSystem.SupportUi.Infrastructure.Security.AuthorizationHandlers.Legacy;
using TeachingRecordSystem.SupportUi.Infrastructure.Security.Requirements;

namespace TeachingRecordSystem.SupportUi.Infrastructure.Security;

public static class AdministratorAuthorization
{
    public static AuthorizationBuilder AddAdminOnlyPolicies(this AuthorizationBuilder builder)
    {
        builder.AddPolicy(
            AuthorizationPolicies.AdminOnly,
            policy => policy
                .RequireAuthenticatedUser()
                .AddRequirements(new AdminOnlyRequirement()));

        builder.Services
            .AddSingleton<IAuthorizationHandler, AdminOnlyAuthorizationHandler>()
            // AuthorizationHandler for Legacy user roles, delete when existing users have been migrated to new user roles.
            .AddSingleton<IAuthorizationHandler, LegacyAdminOnlyAuthorizationHandler>();

        return builder;
    }
}
