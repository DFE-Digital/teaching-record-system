using Microsoft.AspNetCore.Authorization;
using TeachingRecordSystem.SupportUi.Infrastructure.Security.AuthorizationHandlers;
using TeachingRecordSystem.SupportUi.Infrastructure.Security.AuthorizationHandlers.Legacy;
using TeachingRecordSystem.SupportUi.Infrastructure.Security.Requirements;

namespace TeachingRecordSystem.SupportUi.Infrastructure.Security;

public static class RoutesAuthorization
{
    public static AuthorizationBuilder AddRoutesPolicies(this AuthorizationBuilder builder)
    {
        builder.AddPolicy(
            AuthorizationPolicies.RoutesView,
            policy => policy
                .RequireAuthenticatedUser()
                .AddRequirements(new RoutesViewRequirement()));

        builder.AddPolicy(
            AuthorizationPolicies.RoutesEdit,
            policy => policy
                .RequireAuthenticatedUser()
                .AddRequirements(new RoutesEditRequirement()));

        builder.Services
            .AddSingleton<IAuthorizationHandler, RoutesViewAuthorizationHandler>()
            .AddSingleton<IAuthorizationHandler, RoutesEditAuthorizationHandler>()
            // AuthorizationHandler for Legacy user roles, delete when existing users have been migrated to new user roles.
            .AddSingleton<IAuthorizationHandler, LegacyRoutesViewAuthorizationHandler>()
            .AddSingleton<IAuthorizationHandler, LegacyRoutesEditAuthorizationHandler>();

        return builder;
    }
}
