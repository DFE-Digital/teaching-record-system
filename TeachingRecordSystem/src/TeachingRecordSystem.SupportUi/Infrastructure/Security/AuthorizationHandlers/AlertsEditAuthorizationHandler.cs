using Microsoft.AspNetCore.Authorization;
using TeachingRecordSystem.SupportUi.Infrastructure.Security.Requirements;

namespace TeachingRecordSystem.SupportUi.Infrastructure.Security.AuthorizationHandlers;

public class AlertsEditAuthorizationHandler : AuthorizationHandler<AlertsEditRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AlertsEditRequirement requirement)
    {
        // Check the user has either the NonDbsAlerts or DbsAlerts permissions.
        // The AlertType page will deal with ensuring that only permitted alert types can be selected.
        if (context.User.HasMinimumPermission(new(UserPermissionTypes.NonDbsAlerts, UserPermissionLevel.Edit))
            || context.User.HasMinimumPermission(new(UserPermissionTypes.DbsAlerts, UserPermissionLevel.Edit)))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
