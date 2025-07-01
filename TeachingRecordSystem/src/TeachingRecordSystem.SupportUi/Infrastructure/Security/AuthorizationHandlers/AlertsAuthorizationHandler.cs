using Microsoft.AspNetCore.Authorization;
using TeachingRecordSystem.SupportUi.Infrastructure.Security.Requirements;

namespace TeachingRecordSystem.SupportUi.Infrastructure.Security.AuthorizationHandlers;

public class AlertsAuthorizationHandler : AuthorizationHandler<AlertsRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AlertsRequirement requirement)
    {
        // Check the user has either the NonDbsAlerts or DbsAlerts permissions.
        // The AlertType page will deal with ensuring that only permitted alert types can be selected.
        switch (requirement.AlertsPermission)
        {
            case Permissions.Alerts.Flag:
                context.Succeed(requirement);
                break;

            case Permissions.Alerts.Read:
                if (context.User.HasMinimumPermission(new(UserPermissionTypes.NonDbsAlerts, UserPermissionLevel.View))
                    || context.User.HasMinimumPermission(new(UserPermissionTypes.DbsAlerts, UserPermissionLevel.View)))
                {
                    context.Succeed(requirement);
                }

                break;

            case Permissions.Alerts.Write:
                if (context.User.HasMinimumPermission(new(UserPermissionTypes.NonDbsAlerts, UserPermissionLevel.Edit))
                    || context.User.HasMinimumPermission(new(UserPermissionTypes.DbsAlerts, UserPermissionLevel.Edit)))
                {
                    context.Succeed(requirement);
                }
                break;
        }

        return Task.CompletedTask;
    }
}
