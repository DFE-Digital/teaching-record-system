using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.Infrastructure.Security.AuthorizationHandlers;
using TeachingRecordSystem.SupportUi.Infrastructure.Security.AuthorizationHandlers.Legacy;
using TeachingRecordSystem.SupportUi.Infrastructure.Security.Requirements;

namespace TeachingRecordSystem.SupportUi.Infrastructure.Security;

public static class AlertAuthorization
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
            (true, Permissions.Alerts.Flag) => AuthorizationPolicies.DbsAlertFlag,
            (true, Permissions.Alerts.Read) => AuthorizationPolicies.DbsAlertRead,
            (true, Permissions.Alerts.Write) => AuthorizationPolicies.DbsAlertWrite,
            (false, Permissions.Alerts.Flag) => AuthorizationPolicies.NonDbsAlertFlag,
            (false, Permissions.Alerts.Read) => AuthorizationPolicies.NonDbsAlertRead,
            (false, Permissions.Alerts.Write) => AuthorizationPolicies.NonDbsAlertWrite,
            _ => throw new Exception("Unknown permission.")
        };

        return authorizationService.AuthorizeAsync(user, policyName);
    }

    public static AuthorizationBuilder AddAlertPolicies(this AuthorizationBuilder builder)
    {
        builder
            .AddPolicy(
                AuthorizationPolicies.DbsAlertFlag,
                policy => policy
                    .RequireAuthenticatedUser()
                    .AddRequirements(new DbsAlertRequirement(Permissions.Alerts.Flag)))
            .AddPolicy(
                AuthorizationPolicies.DbsAlertRead,
                policy => policy
                    .RequireAuthenticatedUser()
                    .AddRequirements(new DbsAlertRequirement(Permissions.Alerts.Read)))
            .AddPolicy(
                AuthorizationPolicies.DbsAlertWrite,
                policy => policy
                    .RequireAuthenticatedUser()
                    .AddRequirements(new DbsAlertRequirement(Permissions.Alerts.Write)))
            .AddPolicy(
                AuthorizationPolicies.NonDbsAlertFlag,
                policy => policy
                    .RequireAuthenticatedUser()
                    .AddRequirements(new NonDbsAlertRequirement(Permissions.Alerts.Flag)))
            .AddPolicy(
                AuthorizationPolicies.NonDbsAlertRead,
                policy => policy
                    .RequireAuthenticatedUser()
                    .AddRequirements(new NonDbsAlertRequirement(Permissions.Alerts.Read)))
            .AddPolicy(
                AuthorizationPolicies.NonDbsAlertWrite,
                policy => policy
                    .RequireAuthenticatedUser()
                    .AddRequirements(new NonDbsAlertRequirement(Permissions.Alerts.Write)))
            .AddPolicy(
                AuthorizationPolicies.AlertWrite,
                policy => policy
                    .RequireAuthenticatedUser()
                    .AddRequirements(new AlertRequirement(Permissions.Alerts.Write)));

        builder.Services
            .AddSingleton<IAuthorizationHandler, DbsAlertAuthorizationHandler>()
            .AddSingleton<IAuthorizationHandler, NonDbsAlertAuthorizationHandler>()
            .AddSingleton<IAuthorizationHandler, AlertAuthorizationHandler>()

            // AuthorizationHandlers for Legacy user roles, delete when existing users have been migrated to new user roles.
            .AddSingleton<IAuthorizationHandler, LegacyDbsAlertAuthorizationHandler>()
            .AddSingleton<IAuthorizationHandler, LegacyNonDbsAlertAuthorizationHandler>()
            .AddSingleton<IAuthorizationHandler, LegacyAlertAuthorizationHandler>();

        return builder;
    }
}
