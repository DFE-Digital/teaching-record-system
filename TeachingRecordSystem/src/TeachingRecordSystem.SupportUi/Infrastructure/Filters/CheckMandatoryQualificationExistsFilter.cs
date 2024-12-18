using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Core.DataStore.Postgres;

namespace TeachingRecordSystem.SupportUi.Infrastructure.Filters;

/// <summary>
/// Checks that a Mandatory Qualification exists with the ID specified by the qualificationId route value.
/// </summary>
/// <remarks>
/// <para>Returns a <see cref="StatusCodes.Status400BadRequest"/> response if the request is missing the qualicationId route value.</para>
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

        var currentMq = await dbContext.MandatoryQualifications
            .FromSql($"select * from qualifications where qualification_id = {qualificationId} for update")  // https://github.com/dotnet/efcore/issues/26042
            .Include(mq => mq.Provider)
            .Include(mq => mq.Person)
            .SingleOrDefaultAsync();

        if (currentMq is null)
        {
            context.Result = new NotFoundResult();
            return;
        }

        context.HttpContext.SetCurrentMandatoryQualificationFeature(new(currentMq));
        context.HttpContext.SetCurrentPersonFeature(currentMq.Person);

        await next();
    }
}
