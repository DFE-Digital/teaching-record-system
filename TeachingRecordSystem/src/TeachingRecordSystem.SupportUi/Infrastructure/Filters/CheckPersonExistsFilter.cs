using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Dqt.Queries;
using TeachingRecordSystem.Core.Services.TrsDataSync;

namespace TeachingRecordSystem.SupportUi.Infrastructure.Filters;

/// <summary>
/// Checks that a Person exists with the ID specified by the personId route value or query parameter.
/// </summary>
/// <remarks>
/// <para>Returns a <see cref="StatusCodes.Status400BadRequest"/> response if the request is missing the personId route value.</para>
/// <para>
/// Returns a <see cref="StatusCodes.Status404NotFound"/> response if no person with the specified ID exists.
/// </para>
/// <para>Assigns the <see cref="CurrentPersonFeature"/> on success.</para>
/// </remarks>
public class CheckPersonExistsFilter(
    TrsDbContext dbContext,
    ICrmQueryDispatcher crmQueryDispatcher,
    TrsDataSyncHelper syncHelper) : IAsyncResourceFilter
{
    public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
    {
        var personIdParam = context.RouteData.Values["personId"] as string ?? context.HttpContext.Request.Query["personId"];
        if (personIdParam is null || !Guid.TryParse(personIdParam, out Guid personId))
        {
            context.Result = new BadRequestResult();
            return;
        }

        var person = await GetPersonAsync();

        if (person is not null)
        {
            context.HttpContext.SetCurrentPersonFeature(person);
        }
        else
        {
            // If person isn't in the TRS DB it may be because we haven't synced it yet..

            var dqtContact = await crmQueryDispatcher.ExecuteQueryAsync(
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
                var synced = await syncHelper.SyncPersonAsync(personId, /* syncAudit: */ true, /*ignoreInvalid: */ false, /*dryRun:*/ false, CancellationToken.None);
                if (!synced)
                {
                    throw new Exception($"Could not sync Person with contact ID: '{personId}'.");
                }

                person = await GetPersonAsync();
                Debug.Assert(person is not null);

                context.HttpContext.SetCurrentPersonFeature(person);
            }
            else
            {
                context.Result = new NotFoundResult();
                return;
            }
        }

        await next();

        Task<Person?> GetPersonAsync() => dbContext.Persons
            .FromSql($"select * from persons where person_id = {personId} for update")  // https://github.com/dotnet/efcore/issues/26042
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync();
    }
}

public class CheckPersonExistsFilterFactory : IFilterFactory, IOrderedFilter
{
    public bool IsReusable => false;

    public int Order => -200;

    public IFilterMetadata CreateInstance(IServiceProvider serviceProvider) =>
        ActivatorUtilities.CreateInstance<CheckPersonExistsFilter>(serviceProvider);
}
