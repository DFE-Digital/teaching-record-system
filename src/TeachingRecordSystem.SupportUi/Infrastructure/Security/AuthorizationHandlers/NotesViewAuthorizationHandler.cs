using Microsoft.AspNetCore.Authorization;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.Infrastructure.Security.Requirements;

namespace TeachingRecordSystem.SupportUi.Infrastructure.Security.AuthorizationHandlers;

public class NotesViewAuthorizationHandler(TrsDbContext dbContext) : AuthorizationHandler<NotesViewRequirement, Guid>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, NotesViewRequirement requirement, Guid personId)
    {
        var hasDbsAlerts = await dbContext.Alerts
            .Where(a => a.PersonId == personId && a.AlertTypeId == AlertType.DbsAlertTypeId)
            .AnyAsync();

        // If the person has DBS alerts, we only want users with DBS view permissions to view notes
        if (!hasDbsAlerts || context.User.HasMinimumPermission(new(UserPermissionTypes.DbsAlerts, UserPermissionLevel.View)))
        {
            context.Succeed(requirement);
        }

        return;
    }
}
