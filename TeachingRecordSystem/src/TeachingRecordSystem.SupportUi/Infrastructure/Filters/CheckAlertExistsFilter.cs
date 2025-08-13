using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.SupportUi.Infrastructure.Security;
using TeachingRecordSystem.SupportUi.Infrastructure.Security.Requirements;

namespace TeachingRecordSystem.SupportUi.Infrastructure.Filters;

/// <summary>
/// Checks that an Alert exists with the ID specified by the alertId route value and
/// checks that the current user has the required permissions to access it.
/// </summary>
/// <remarks>
/// <para>Returns a <see cref="StatusCodes.Status400BadRequest"/> response if the request is missing the alertId route value.</para>
/// <para>Returns a <see cref="StatusCodes.Status400BadRequest"/> response if the Alert exists but the associated Person has been deactivated.</para>
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

        var query = dbContext.Alerts
            .FromSql($"select * from alerts where alert_id = {alertId} for update")  // https://github.com/dotnet/efcore/issues/26042
            .Include(a => a.Person);

        // Query without query filters first - query filters will filter out deactivated Person
        // meaning the entire Alert is not found, but if Person is deactivated we
        // we need to return a BadRequest instead of a NotFound result
        var currentAlertWithPotentiallyDeactivatedPerson = await query
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync();

        if (currentAlertWithPotentiallyDeactivatedPerson is not null &&
            currentAlertWithPotentiallyDeactivatedPerson.Person!.Status == PersonStatus.Deactivated)
        {
            context.Result = new BadRequestResult();
            return;
        }

        // Query again with query filters to make sure deleted Alerts are ignored
        var currentAlert = await query
            .SingleOrDefaultAsync();

        if (currentAlert is null)
        {
            context.Result = new NotFoundResult();
            return;
        }

        var authorizationResult = await authorizationService.AuthorizeAsync(
            context.HttpContext.User,
            currentAlert.AlertTypeId,
            new AlertTypePermissionRequirement(requiredPermissionType));

        if (authorizationResult is not { Succeeded: true })
        {
            context.Result = new ForbidResult();
            return;
        }

        context.HttpContext.SetCurrentAlertFeature(new(currentAlert));
        context.HttpContext.SetCurrentPersonFeature(currentAlert.Person!);

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
