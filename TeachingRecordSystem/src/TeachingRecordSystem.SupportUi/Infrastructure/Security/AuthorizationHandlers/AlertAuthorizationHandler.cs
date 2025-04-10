using Microsoft.AspNetCore.Authorization;
using TeachingRecordSystem.SupportUi.Infrastructure.Security.Requirements;

namespace TeachingRecordSystem.SupportUi.Infrastructure.Security.AuthorizationHandlers;

public class AlertAuthorizationHandler : AuthorizationHandler<AlertRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AlertRequirement requirement)
    {
        // Check the user has either the NonDbsAlerts or DbsAlerts permissions.
        // The AlertType page will deal with ensuring that only permitted alert types can be selected.
        switch (requirement.AlertsPermission)
        {
            case Permissions.Alerts.Flag:
            case Permissions.Alerts.Read:
                context.Succeed(requirement);
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
