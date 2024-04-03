using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Dqt.Queries;
using TeachingRecordSystem.Core.Jobs.Scheduling;
using TeachingRecordSystem.Core.Services.TrsDataSync;

namespace TeachingRecordSystem.SupportUi.Infrastructure.Filters;

/// <summary>
/// Checks that a Person exists with the ID specified by the personId route value or query parameter.
/// </summary>
/// <remarks>
/// <para>Returns a <see cref="StatusCodes.Status400BadRequest"/> response if the request is missing the personId route value.</para>
/// <para>
/// Returns a <see cref="StatusCodes.Status404NotFound"/> response if no person with the specified ID exists or if
/// <paramref name="requireQts"/> is <c>true</c> and the person does not have QTS.
/// </para>
/// <para>Assigns the <see cref="CurrentPersonFeature"/> on success.</para>
/// </remarks>
public class CheckPersonExistsFilter(
    TrsDbContext dbContext,
    ICrmQueryDispatcher crmQueryDispatcher,
    IBackgroundJobScheduler backgroundJobScheduler,
    bool requireQts = false) : IAsyncResourceFilter
{
    private readonly bool _requireQts = requireQts;

    public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
    {
        var personIdParam = context.RouteData.Values["personId"] as string ?? context.HttpContext.Request.Query["personId"];
        if (personIdParam is null || !Guid.TryParse(personIdParam, out Guid personId))
        {
            context.Result = new BadRequestResult();
            return;
        }

        var person = await crmQueryDispatcher.ExecuteQuery(
            new GetContactDetailByIdQuery(
                personId,
                new ColumnSet(
                    Contact.Fields.Id,
                    Contact.Fields.FirstName,
                    Contact.Fields.MiddleName,
                    Contact.Fields.LastName,
                    Contact.Fields.dfeta_StatedFirstName,
                    Contact.Fields.dfeta_StatedLastName,
                    Contact.Fields.dfeta_StatedMiddleName,
                    Contact.Fields.dfeta_QTSDate)));

        if (person is null)
        {
            context.Result = new NotFoundResult();
            return;
        }
        else if (_requireQts && person.Contact.dfeta_QTSDate is null)
        {
            context.Result = new BadRequestResult();
            return;
        }

        context.HttpContext.SetCurrentPersonFeature(person);

        // Ensure we've synced this person into the TRS DB at least once
        if (!await dbContext.Persons.AnyAsync(p => p.PersonId == personId))
        {
            await backgroundJobScheduler.Enqueue<TrsDataSyncHelper>(helper => helper.SyncPerson(personId, CancellationToken.None));
        }

        await next();
    }
}

public class CheckPersonExistsFilterFactory(bool requireQts = false) : IFilterFactory, IOrderedFilter
{
    public bool IsReusable => false;

    public int Order => -200;

    public IFilterMetadata CreateInstance(IServiceProvider serviceProvider) =>
        ActivatorUtilities.CreateInstance<CheckPersonExistsFilter>(serviceProvider, requireQts);
}
