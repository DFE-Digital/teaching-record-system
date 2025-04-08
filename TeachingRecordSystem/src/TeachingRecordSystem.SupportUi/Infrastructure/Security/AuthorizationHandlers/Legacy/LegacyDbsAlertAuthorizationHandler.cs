using Microsoft.AspNetCore.Authorization;
using TeachingRecordSystem.Core.Legacy;
using TeachingRecordSystem.SupportUi.Infrastructure.Security.Requirements;

namespace TeachingRecordSystem.SupportUi.Infrastructure.Security.AuthorizationHandlers.Legacy;

/// <summary>
/// AuthorizationHandler for Legacy user roles, delete when existing users have been migrated to new user roles.
/// </summary>
public class LegacyDbsAlertAuthorizationHandler : AuthorizationHandler<DbsAlertRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, DbsAlertRequirement requirement)
    {
        switch (requirement.AlertsPermission)
        {
            case Permissions.Alerts.Flag:
                context.Succeed(requirement);
                break;

            case Permissions.Alerts.Read:
                if (context.User.IsInRole(LegacyUserRoles.DbsAlertsReadOnly)
                    || context.User.IsInRole(LegacyUserRoles.DbsAlertsReadWrite)
                    || context.User.IsInRole(LegacyUserRoles.Administrator))
                {
                    context.Succeed(requirement);
                }
                break;

            case Permissions.Alerts.Write:
                if (context.User.IsInRole(LegacyUserRoles.DbsAlertsReadWrite)
                    || context.User.IsInRole(LegacyUserRoles.Administrator))
                {
                    context.Succeed(requirement);
                }
                break;
        }

        return Task.CompletedTask;
    }
}
