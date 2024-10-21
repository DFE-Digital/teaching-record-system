using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.SupportUi.Infrastructure.Security;

namespace TeachingRecordSystem.SupportUi.Infrastructure.Filters;

/// <summary>
/// Checks that an Alert exists with the ID specified by the alertId route value and
/// checks that the current user has the required permissions to access it.
/// </summary>
/// <remarks>
/// <para>Returns a <see cref="StatusCodes.Status400BadRequest"/> response if the request is missing the alertId route value.</para>
/// <para>Returns a <see cref="StatusCodes.Status404NotFound"/> response if no alert with the specified ID exists.</para>
/// <para>Returns a <see cref="StatusCodes.Status403Forbidden"/> response if the user does not have the required permission to access the alert.</para>
/// <para>Assigns the <see cref="CurrentAlertFeature"/> and <see cref="CurrentPersonFeature"/> on success.</para>
/// </remarks>
public class CheckAlertExistsFilter(Permissions.Alerts requiredPermissionType, TrsDbContext dbContext, IAuthorizationService authorizationService) : IAsyncResourceFilter
{
    public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
    {
        if (context.RouteData.Values["alertId"] is not string alertIdParam ||
            !Guid.TryParse(alertIdParam, out Guid alertId))
        {
            context.Result = new BadRequestResult();
            return;
        }

        var currentAlert = await dbContext.Alerts
            .Include(a => a.AlertType)
            .ThenInclude(at => at.AlertCategory)
            .Include(a => a.Person)
            .SingleOrDefaultAsync(a => a.AlertId == alertId);

        if (currentAlert is null)
        {
            context.Result = new NotFoundResult();
            return;
        }

        var authorizationResult = await authorizationService.AuthorizeForAlertTypeAsync(
            context.HttpContext.User,
            currentAlert.AlertTypeId,
            requiredPermissionType);

        if (authorizationResult is not { Succeeded: true })
        {
            context.Result = new ForbidResult();
            return;
        }

        context.HttpContext.SetCurrentAlertFeature(new(currentAlert));
        context.HttpContext.SetCurrentPersonFeature(currentAlert.Person);

        await next();
    }
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class CheckAlertExistsFilterFactory(Permissions.Alerts requiredPermission) : Attribute, IFilterFactory
{
    public bool IsReusable => false;

    public IFilterMetadata CreateInstance(IServiceProvider serviceProvider) =>
        ActivatorUtilities.CreateInstance<CheckAlertExistsFilter>(serviceProvider, requiredPermission);
}
