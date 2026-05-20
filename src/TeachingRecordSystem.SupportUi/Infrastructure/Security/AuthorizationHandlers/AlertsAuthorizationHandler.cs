using Microsoft.AspNetCore.Authorization;
using TeachingRecordSystem.SupportUi.Infrastructure.Security.Requirements;

namespace TeachingRecordSystem.SupportUi.Infrastructure.Security.AuthorizationHandlers;

public class AlertsViewAuthorizationHandler : AuthorizationHandler<AlertsViewRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AlertsViewRequirement requirement)
    {
        // Check the user has either the NonDbsAlerts or DbsAlerts permissions.
        // The AlertType page will deal with ensuring that only permitted alert types can be selected.
        if (context.User.HasMinimumPermission(new(UserPermissionTypes.NonDbsAlerts, UserPermissionLevel.View))
            || context.User.HasMinimumPermission(new(UserPermissionTypes.DbsAlerts, UserPermissionLevel.View)))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
