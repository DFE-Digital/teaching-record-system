using Microsoft.AspNetCore.Authorization;
using TeachingRecordSystem.Core.Legacy;
using TeachingRecordSystem.SupportUi.Infrastructure.Security.Requirements;

namespace TeachingRecordSystem.SupportUi.Infrastructure.Security.AuthorizationHandlers.Legacy;

/// <summary>
/// AuthorizationHandler for Legacy user roles, delete when existing users have been migrated to new user roles.
/// </summary>
public class LegacyNonDbsAlertsAuthorizationHandler : AuthorizationHandler<NonDbsAlertsRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, NonDbsAlertsRequirement requirement)
    {
        // If the user has been migrated to the new user roles, they may still have the legacy roles, so we need to
        // disregard them for this user as they may be different to their new role.
        if (context.User.HasBeenMigrated())
        {
            return Task.CompletedTask;
        }

        switch (requirement.AlertsPermission)
        {
            case Permissions.Alerts.Flag:
            case Permissions.Alerts.Read:
                context.Succeed(requirement);
                break;

            case Permissions.Alerts.Write:
                if (context.User.IsInRole(LegacyUserRoles.AlertsReadWrite)
                    || context.User.IsInRole(LegacyUserRoles.Administrator))
                {
                    context.Succeed(requirement);
                }
                break;
        }

        return Task.CompletedTask;
    }
}
