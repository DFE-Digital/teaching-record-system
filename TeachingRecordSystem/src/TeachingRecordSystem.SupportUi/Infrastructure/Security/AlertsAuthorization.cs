using Microsoft.AspNetCore.Authorization;
using TeachingRecordSystem.SupportUi.Infrastructure.Security.AuthorizationHandlers;
using TeachingRecordSystem.SupportUi.Infrastructure.Security.AuthorizationHandlers.Legacy;
using TeachingRecordSystem.SupportUi.Infrastructure.Security.Requirements;

namespace TeachingRecordSystem.SupportUi.Infrastructure.Security;

public static class AlertsAuthorization
{
    public static AuthorizationBuilder AddAlertsPolicies(this AuthorizationBuilder builder)
    {
        builder
            .AddPolicy(
                AuthorizationPolicies.AlertsView,
                policy => policy
                    .RequireAuthenticatedUser()
                    .AddRequirements(new AlertsViewRequirement()))
            .AddPolicy(
                AuthorizationPolicies.AlertsEdit,
                policy => policy
                    .RequireAuthenticatedUser()
                    .AddRequirements(new AlertsEditRequirement()));

        builder.Services
            .AddSingleton<IAuthorizationHandler, AlertsViewAuthorizationHandler>()
            .AddSingleton<IAuthorizationHandler, AlertsEditAuthorizationHandler>()
            .AddSingleton<IAuthorizationHandler, AlertTypeAuthorizationHandler>()

            // AuthorizationHandlers for Legacy user roles, delete when existing users have been migrated to new user roles.
            .AddSingleton<IAuthorizationHandler, LegacyAlertsViewAuthorizationHandler>()
            .AddSingleton<IAuthorizationHandler, LegacyAlertsEditAuthorizationHandler>()
            .AddSingleton<IAuthorizationHandler, LegacyAlertTypeAuthorizationHandler>();

        return builder;
    }
}
