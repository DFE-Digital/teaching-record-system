using System.Transactions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Core.DataStore.Postgres;

namespace TeachingRecordSystem.SupportUi.Infrastructure.Filters;

/// <summary>
/// Checks that a Professional Status qualification exists with the ID specified by the qualificationId route value.
/// </summary>
/// <remarks>
/// <para>Returns a <see cref="StatusCodes.Status400BadRequest"/> response if the request is missing the qualicationId route value.</para>
/// <para>Returns a <see cref="StatusCodes.Status400BadRequest"/> response if the Route exists but the associated Person has been deactivated.</para>
/// <para>Returns a <see cref="StatusCodes.Status404NotFound"/> response if no Route with the specified ID exists.</para>
/// <para>Assigns the <see cref="CurrentProfessionalStatusFeature"/> and <see cref="CurrentPersonFeature"/> on success.</para>
/// </remarks>
public class CheckRouteToProfessionalStatusExistsFilter(TrsDbContext dbContext) : IAsyncResourceFilter
{
    public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
    {
        if (context.RouteData.Values["qualificationId"] is not string qualificationIdParam ||
            !Guid.TryParse(qualificationIdParam, out Guid qualificationId))
        {
            context.Result = new BadRequestResult();
            return;
        }

        _ = Transaction.Current ?? throw new InvalidOperationException("A TransactionScope is required when enqueueing a background job.");

        var query = dbContext.RouteToProfessionalStatuses
            .FromSql($"select * from qualifications where qualification_id = {qualificationId} for update") // https://github.com/dotnet/efcore/issues/26042
            .Include(ps => ps.Person)
            .ThenInclude(p => p!.Qualifications);

        // Query without query filters first - query filters will filter out deactivated Person
        // meaning the entire Route is not found, but if Person is deactivated we
        // we need to return a BadRequest instead of a NotFound result
        var currentRouteWithPotentiallyDeactivatedPerson = await query
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync();

        if (currentRouteWithPotentiallyDeactivatedPerson is not null &&
            currentRouteWithPotentiallyDeactivatedPerson.Person!.Status == PersonStatus.Deactivated)
        {
            context.Result = new BadRequestResult();
            return;
        }

        // Query again with query filters to make sure deleted Routes are ignored
        var currentRoute = await query
            .SingleOrDefaultAsync();

        if (currentRoute is null)
        {
            context.Result = new NotFoundResult();
            return;
        }

        context.HttpContext.SetCurrentProfessionalStatusFeature(new(currentRoute));
        context.HttpContext.SetCurrentPersonFeature(currentRoute.Person!);
        await next();
    }
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class CheckRouteToProfessionalStatusExistsFilterFactory() : Attribute, IFilterFactory
{
    public bool IsReusable => false;

    public IFilterMetadata CreateInstance(IServiceProvider serviceProvider) =>
        ActivatorUtilities.CreateInstance<CheckRouteToProfessionalStatusExistsFilter>(serviceProvider);
}
