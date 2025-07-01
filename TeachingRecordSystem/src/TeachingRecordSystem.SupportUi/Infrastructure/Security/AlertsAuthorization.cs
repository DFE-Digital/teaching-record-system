using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.Infrastructure.Security.AuthorizationHandlers;
using TeachingRecordSystem.SupportUi.Infrastructure.Security.AuthorizationHandlers.Legacy;
using TeachingRecordSystem.SupportUi.Infrastructure.Security.Requirements;

namespace TeachingRecordSystem.SupportUi.Infrastructure.Security;

public static class AlertsAuthorization
{
    public static Task<AuthorizationResult> AuthorizeForAlertTypeAsync(
        this IAuthorizationService authorizationService,
        ClaimsPrincipal user,
        Guid alertTypeId,
        Permissions.Alerts permission)
    {
        var isDbsAlert = alertTypeId == AlertType.DbsAlertTypeId;

        var policyName = (isDbsAlert, permission) switch
        {
            (true, Permissions.Alerts.Flag) => AuthorizationPolicies.DbsAlertsFlag,
            (true, Permissions.Alerts.Read) => AuthorizationPolicies.DbsAlertsRead,
            (true, Permissions.Alerts.Write) => AuthorizationPolicies.DbsAlertsWrite,
            (false, Permissions.Alerts.Flag) => AuthorizationPolicies.NonDbsAlertsFlag,
            (false, Permissions.Alerts.Read) => AuthorizationPolicies.NonDbsAlertsRead,
            (false, Permissions.Alerts.Write) => AuthorizationPolicies.NonDbsAlertsWrite,
            _ => throw new Exception("Unknown permission.")
        };

        return authorizationService.AuthorizeAsync(user, policyName);
    }

    public static AuthorizationBuilder AddAlertsPolicies(this AuthorizationBuilder builder)
    {
        builder
            .AddPolicy(
                AuthorizationPolicies.DbsAlertsFlag,
                policy => policy
                    .RequireAuthenticatedUser()
                    .AddRequirements(new DbsAlertsRequirement(Permissions.Alerts.Flag)))
            .AddPolicy(
                AuthorizationPolicies.DbsAlertsRead,
                policy => policy
                    .RequireAuthenticatedUser()
                    .AddRequirements(new DbsAlertsRequirement(Permissions.Alerts.Read)))
            .AddPolicy(
                AuthorizationPolicies.DbsAlertsWrite,
                policy => policy
                    .RequireAuthenticatedUser()
                    .AddRequirements(new DbsAlertsRequirement(Permissions.Alerts.Write)))
            .AddPolicy(
                AuthorizationPolicies.NonDbsAlertsFlag,
                policy => policy
                    .RequireAuthenticatedUser()
                    .AddRequirements(new NonDbsAlertsRequirement(Permissions.Alerts.Flag)))
            .AddPolicy(
                AuthorizationPolicies.NonDbsAlertsRead,
                policy => policy
                    .RequireAuthenticatedUser()
                    .AddRequirements(new NonDbsAlertsRequirement(Permissions.Alerts.Read)))
            .AddPolicy(
                AuthorizationPolicies.NonDbsAlertsWrite,
                policy => policy
                    .RequireAuthenticatedUser()
                    .AddRequirements(new NonDbsAlertsRequirement(Permissions.Alerts.Write)))
            .AddPolicy(
                AuthorizationPolicies.AlertsRead,
                policy => policy
                    .RequireAuthenticatedUser()
                    .AddRequirements(new AlertsRequirement(Permissions.Alerts.Read)))
            .AddPolicy(
                AuthorizationPolicies.AlertsWrite,
                policy => policy
                    .RequireAuthenticatedUser()
                    .AddRequirements(new AlertsRequirement(Permissions.Alerts.Write)));

        builder.Services
            .AddSingleton<IAuthorizationHandler, DbsAlertsAuthorizationHandler>()
            .AddSingleton<IAuthorizationHandler, NonDbsAlertsAuthorizationHandler>()
            .AddSingleton<IAuthorizationHandler, AlertsAuthorizationHandler>()

            // AuthorizationHandlers for Legacy user roles, delete when existing users have been migrated to new user roles.
            .AddSingleton<IAuthorizationHandler, LegacyDbsAlertsAuthorizationHandler>()
            .AddSingleton<IAuthorizationHandler, LegacyNonDbsAlertsAuthorizationHandler>()
            .AddSingleton<IAuthorizationHandler, LegacyAlertsAuthorizationHandler>();

        return builder;
    }
}
