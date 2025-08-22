using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Core.DataStore.Postgres;

namespace TeachingRecordSystem.SupportUi.Infrastructure.Filters;

/// <summary>
/// Checks that a Mandatory Qualification exists with the ID specified by the qualificationId route value.
/// </summary>
/// <remarks>
/// <para>Returns a <see cref="StatusCodes.Status400BadRequest"/> response if the request is missing the qualicationId route value.</para>
/// <para>Returns a <see cref="StatusCodes.Status400BadRequest"/> response if the Qualification exists but the associated Person has been deactivated.</para>
/// <para>Returns a <see cref="StatusCodes.Status404NotFound"/> response if no Mandatory Qualification with the specified ID exists.</para>
/// <para>Assigns the <see cref="CurrentMandatoryQualificationFeature"/> and <see cref="CurrentPersonFeature"/> on success.</para>
/// </remarks>
public class CheckMandatoryQualificationExistsFilter(TrsDbContext dbContext) : IAsyncResourceFilter
{
    public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
    {
        if (context.RouteData.Values["qualificationId"] is not string qualificationIdParam ||
            !Guid.TryParse(qualificationIdParam, out Guid qualificationId))
        {
            context.Result = new BadRequestResult();
            return;
        }

        await context.HttpContext.EnsureDbTransactionAsync();

        var query = dbContext.MandatoryQualifications
            .FromSql($"select * from qualifications where qualification_id = {qualificationId} for update")  // https://github.com/dotnet/efcore/issues/26042
            .Include(mq => mq.Person);

        // Query without query filters first - query filters will filter out deactivated Person
        // meaning the entire Qualification is not found, but if Person is deactivated we
        // we need to return a BadRequest instead of a NotFound result
        var currentMqWithPotentiallyDeactivatedPerson = await query
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync();

        if (currentMqWithPotentiallyDeactivatedPerson is not null &&
            currentMqWithPotentiallyDeactivatedPerson.Person!.Status == PersonStatus.Deactivated)
        {
            context.Result = new BadRequestResult();
            return;
        }

        // Query again with query filters to make sure deleted Qualifications are ignored
        var currentMq = await query
            .SingleOrDefaultAsync();

        if (currentMq is null)
        {
            context.Result = new NotFoundResult();
            return;
        }

        context.HttpContext.SetCurrentMandatoryQualificationFeature(new(currentMq));
        context.HttpContext.SetCurrentPersonFeature(currentMq.Person!);

        await next();
    }
}
