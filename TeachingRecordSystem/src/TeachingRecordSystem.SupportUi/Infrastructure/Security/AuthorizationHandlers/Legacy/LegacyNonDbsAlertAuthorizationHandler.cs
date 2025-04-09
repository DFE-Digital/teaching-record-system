using Microsoft.AspNetCore.Authorization;
using TeachingRecordSystem.Core.Legacy;
using TeachingRecordSystem.SupportUi.Infrastructure.Security.Requirements;

namespace TeachingRecordSystem.SupportUi.Infrastructure.Security.AuthorizationHandlers.Legacy;

/// <summary>
/// AuthorizationHandler for Legacy user roles, delete when existing users have been migrated to new user roles.
/// </summary>
public class LegacyNonDbsAlertAuthorizationHandler : AuthorizationHandler<NonDbsAlertRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, NonDbsAlertRequirement requirement)
    {
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
