using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Core.DataStore.Postgres;

namespace TeachingRecordSystem.SupportUi.Infrastructure.Filters;

/// <summary>
/// Checks that a Professional Status qualification exists with the ID specified by the qualificationId route value.
/// </summary>
/// <remarks>
/// <para>Returns a <see cref="StatusCodes.Status400BadRequest"/> response if the request is missing the qualicationId route value.</para>
/// <para>Returns a <see cref="StatusCodes.Status404NotFound"/> response if no Professional status with the specified ID exists.</para>
/// <para>Assigns the <see cref="CurrentProfessionalStatusFeature"/> and <see cref="CurrentPersonFeature"/> on success.</para>
/// </remarks>
public class CheckProfessionalStatusExistsFilter(TrsDbContext dbContext) : IAsyncResourceFilter
{
    public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
    {
        if (context.RouteData.Values["qualificationId"] is not string qualificationIdParam ||
            !Guid.TryParse(qualificationIdParam, out Guid qualificationId))
        {
            context.Result = new BadRequestResult();
            return;
        }

        var currentProfessionalStatus = await dbContext.ProfessionalStatuses
            .FromSql($"select * from qualifications where qualification_id = {qualificationId} for update")  // https://github.com/dotnet/efcore/issues/26042
            .Include(ps => ps.Person)
            .SingleOrDefaultAsync();

        if (currentProfessionalStatus is null)
        {
            context.Result = new NotFoundResult();
            return;
        }

        context.HttpContext.SetCurrentProfessionalStatusFeature(new(currentProfessionalStatus));
        context.HttpContext.SetCurrentPersonFeature(currentProfessionalStatus.Person);
        await next();
    }
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class CheckProfessionalStatusExistsFilterFactory() : Attribute, IFilterFactory
{
    public bool IsReusable => false;

    public IFilterMetadata CreateInstance(IServiceProvider serviceProvider) =>
        ActivatorUtilities.CreateInstance<CheckProfessionalStatusExistsFilter>(serviceProvider);
}
