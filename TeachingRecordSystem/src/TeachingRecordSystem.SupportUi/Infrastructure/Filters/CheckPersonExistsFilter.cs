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
    IBackgroundJobScheduler backgroundJobScheduler) : IAsyncResourceFilter
{
    public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
    {
        var personIdParam = context.RouteData.Values["personId"] as string ?? context.HttpContext.Request.Query["personId"];
        if (personIdParam is null || !Guid.TryParse(personIdParam, out Guid personId))
        {
            context.Result = new BadRequestResult();
            return;
        }

        var person = await dbContext.Persons.SingleOrDefaultAsync(p => p.PersonId == personId);

        if (person is not null)
        {
            context.HttpContext.SetCurrentPersonFeature(person);
        }
        else
        {
            // If person isn't in the TRS DB it may be because we haven't synced it yet..

            var dqtContact = await crmQueryDispatcher.ExecuteQuery(
                new GetActiveContactDetailByIdQuery(
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

            if (dqtContact is not null)
            {
                context.HttpContext.SetCurrentPersonFeature(dqtContact);

                await backgroundJobScheduler.Enqueue<TrsDataSyncHelper>(helper => helper.SyncPerson(personId, /*ignoreInvalid: */ false, /*dryRun:*/ false, CancellationToken.None));
            }
            else
            {
                context.Result = new NotFoundResult();
                return;
            }
        }

        await next();
    }
}

public class CheckPersonExistsFilterFactory : IFilterFactory, IOrderedFilter
{
    public bool IsReusable => false;

    public int Order => -200;

    public IFilterMetadata CreateInstance(IServiceProvider serviceProvider) =>
        ActivatorUtilities.CreateInstance<CheckPersonExistsFilter>(serviceProvider);
}
