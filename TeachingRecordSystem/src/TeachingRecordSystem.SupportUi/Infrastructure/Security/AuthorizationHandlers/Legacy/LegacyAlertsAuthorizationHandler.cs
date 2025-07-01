using Microsoft.AspNetCore.Authorization;
using TeachingRecordSystem.Core.Legacy;
using TeachingRecordSystem.SupportUi.Infrastructure.Security.Requirements;

namespace TeachingRecordSystem.SupportUi.Infrastructure.Security.AuthorizationHandlers.Legacy;

/// <summary>
/// AuthorizationHandler for Legacy user roles, delete when existing users have been migrated to new user roles.
/// </summary>
public class LegacyAlertsAuthorizationHandler : AuthorizationHandler<AlertsRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AlertsRequirement requirement)
    {
        // If the user has been migrated to the new user roles, they may still have the legacy roles, so we need to
        // disregard them for this user as they may be different to their new role.
        if (context.User.HasBeenMigrated())
        {
            return Task.CompletedTask;
        }

        // Check the user has either the AlertsReadWrite or DbsAlertsReadWrite role.
        // The AlertType page will deal with ensuring that only permitted alert types can be selected.
        if (context.User.IsInRole(LegacyUserRoles.AlertsReadWrite)
            || context.User.IsInRole(LegacyUserRoles.DbsAlertsReadWrite)
            || context.User.IsInRole(LegacyUserRoles.Administrator))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
