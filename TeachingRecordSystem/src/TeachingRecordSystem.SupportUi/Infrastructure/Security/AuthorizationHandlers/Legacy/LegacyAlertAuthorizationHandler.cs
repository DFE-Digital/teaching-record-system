using Microsoft.AspNetCore.Authorization;
using TeachingRecordSystem.Core.Legacy;
using TeachingRecordSystem.SupportUi.Infrastructure.Security.Requirements;

namespace TeachingRecordSystem.SupportUi.Infrastructure.Security.AuthorizationHandlers.Legacy;

/// <summary>
/// AuthorizationHandler for Legacy user roles, delete when existing users have been migrated to new user roles.
/// </summary>
public class LegacyAlertAuthorizationHandler : AuthorizationHandler<AlertRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AlertRequirement requirement)
    {
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
