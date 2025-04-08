using Microsoft.AspNetCore.Authorization;
using TeachingRecordSystem.SupportUi.Infrastructure.Security.Requirements;

namespace TeachingRecordSystem.SupportUi.Infrastructure.Security.AuthorizationHandlers;

public class DbsAlertAuthorizationHandler : AuthorizationHandler<DbsAlertRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, DbsAlertRequirement requirement)
    {
        switch (requirement.AlertsPermission)
        {
            case Permissions.Alerts.Flag:
                context.Succeed(requirement);
                break;

            case Permissions.Alerts.Read:
                if (context.User.HasMinimumPermission(new(UserPermissionTypes.DbsAlerts, UserPermissionLevel.View)))
                {
                    context.Succeed(requirement);
                }
                break;

            case Permissions.Alerts.Write:
                if (context.User.HasMinimumPermission(new(UserPermissionTypes.DbsAlerts, UserPermissionLevel.Edit)))
                {
                    context.Succeed(requirement);
                }
                break;
        }

        return Task.CompletedTask;
    }
}
