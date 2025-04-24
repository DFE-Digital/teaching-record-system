using Microsoft.AspNetCore.Authorization;
using TeachingRecordSystem.SupportUi.Infrastructure.Security.Requirements;

namespace TeachingRecordSystem.SupportUi.Infrastructure.Security.AuthorizationHandlers;

public class AdminOnlyAuthorizationHandler : AuthorizationHandler<AdminOnlyRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AdminOnlyRequirement requirement)
    {
        if (context.User.IsInRole(UserRoles.Administrator))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
