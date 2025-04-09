using Microsoft.AspNetCore.Authorization;
using TeachingRecordSystem.SupportUi.Infrastructure.Security.Requirements;

namespace TeachingRecordSystem.SupportUi.Infrastructure.Security.AuthorizationHandlers;

public class NonDbsAlertAuthorizationHandler : AuthorizationHandler<NonDbsAlertRequirement>
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
                if (context.User.HasMinimumPermission(new(UserPermissionTypes.NonDbsAlerts, UserPermissionLevel.Edit)))
                {
                    context.Succeed(requirement);
                }
                break;
        }

        return Task.CompletedTask;
    }
}
