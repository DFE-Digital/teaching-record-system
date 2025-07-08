using Microsoft.AspNetCore.Authorization;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.Infrastructure.Security.Requirements;

namespace TeachingRecordSystem.SupportUi.Infrastructure.Security.AuthorizationHandlers;

public class AlertTypeAuthorizationHandler : AuthorizationHandler<AlertTypePermissionRequirement, Guid>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AlertTypePermissionRequirement requirement, Guid alertTypeId)
    {
        var permissionType = alertTypeId == AlertType.DbsAlertTypeId
            ? UserPermissionTypes.DbsAlerts
            : UserPermissionTypes.NonDbsAlerts;

        switch (requirement.AlertsPermission)
        {
            case Permissions.Alerts.Flag:
                context.Succeed(requirement);
                break;

            case Permissions.Alerts.Read:
                if (context.User.HasMinimumPermission(new(permissionType, UserPermissionLevel.View)))
                {
                    context.Succeed(requirement);
                }
                break;

            case Permissions.Alerts.Write:
                if (context.User.HasMinimumPermission(new(permissionType, UserPermissionLevel.Edit)))
                {
                    context.Succeed(requirement);
                }
                break;
        }

        return Task.CompletedTask;
    }
}
