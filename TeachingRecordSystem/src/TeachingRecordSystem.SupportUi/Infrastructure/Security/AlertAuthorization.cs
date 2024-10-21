using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

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

    public static AuthorizationBuilder AddAlertPolicies(this AuthorizationBuilder builder) => builder
        .AddPolicy(
            AuthorizationPolicies.DbsAlertFlag,
            policy => policy
                .RequireAuthenticatedUser())
        .AddPolicy(
            AuthorizationPolicies.DbsAlertRead,
            policy => policy
                .RequireAuthenticatedUser()
                .RequireRole(UserRoles.DbsAlertsReadOnly, UserRoles.DbsAlertsReadWrite, UserRoles.Administrator))
        .AddPolicy(
            AuthorizationPolicies.DbsAlertWrite,
            policy => policy
                .RequireAuthenticatedUser()
                .RequireRole(UserRoles.DbsAlertsReadWrite, UserRoles.Administrator))
        .AddPolicy(
            AuthorizationPolicies.NonDbsAlertFlag,
            policy => policy
                .RequireAuthenticatedUser())
        .AddPolicy(
            AuthorizationPolicies.NonDbsAlertRead,
            policy => policy
                .RequireAuthenticatedUser())
        .AddPolicy(
            AuthorizationPolicies.NonDbsAlertWrite,
            policy => policy
                .RequireAuthenticatedUser()
                .RequireRole(UserRoles.AlertsReadWrite, UserRoles.Administrator));
}
