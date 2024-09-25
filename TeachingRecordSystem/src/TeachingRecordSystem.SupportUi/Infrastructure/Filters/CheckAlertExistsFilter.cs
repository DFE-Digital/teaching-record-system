using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Core.DataStore.Postgres;

namespace TeachingRecordSystem.SupportUi.Infrastructure.Filters;

/// <summary>
/// Checks that an Alert exists with the ID specified by the alertId route value.
/// </summary>
/// <remarks>
/// <para>Returns a <see cref="StatusCodes.Status400BadRequest"/> response if the request is missing the alertId route value.</para>
/// <para>Returns a <see cref="StatusCodes.Status404NotFound"/> response if no alert with the specified ID exists.</para>
/// <para>Assigns the <see cref="CurrentMandatoryQualificationFeature"/> and <see cref="CurrentPersonFeature"/> on success.</para>
/// </remarks>
public class CheckAlertExistsFilter(TrsDbContext dbContext, ICrmQueryDispatcher crmQueryDispatcher) :
    AssignCurrentPersonInfoFilterBase(crmQueryDispatcher), IAsyncResourceFilter
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
            .SingleOrDefaultAsync(a => a.AlertId == alertId);

        if (currentAlert is null)
        {
            context.Result = new NotFoundResult();
            return;
        }

        context.HttpContext.SetCurrentAlertFeature(new(currentAlert));

        await TryAssignCurrentPersonInfo(currentAlert.PersonId, context.HttpContext);

        await next();
    }
}
