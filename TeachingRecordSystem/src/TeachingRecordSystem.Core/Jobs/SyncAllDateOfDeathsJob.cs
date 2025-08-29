using System.ServiceModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Services.TrsDataSync;

namespace TeachingRecordSystem.Core.Jobs;

public class SyncAllDateOfDeathsJob(
    [FromKeyedServices(TrsDataSyncHelper.CrmClientName)] IOrganizationServiceAsync2 organizationService,
    IDbContextFactory<TrsDbContext> dbContextFactory)
{
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        const int pageSize = 1000;

        var columns = new ColumnSet(Contact.Fields.dfeta_DateofDeath);

        var query = new QueryExpression(Contact.EntityLogicalName)
        {
            ColumnSet = columns,
            Criteria =
            {
                Conditions =
                {
                    new ConditionExpression(Contact.Fields.dfeta_DateofDeath, ConditionOperator.NotNull)
                }
            },
            Orders =
            {
                new OrderExpression(Contact.PrimaryIdAttribute, OrderType.Ascending)
            },
            PageInfo = new PagingInfo()
            {
                Count = pageSize,
                PageNumber = 1
            }
        };

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            EntityCollection result;
            try
            {
                result = await organizationService.RetrieveMultipleAsync(query);
            }
            catch (FaultException<OrganizationServiceFault> fex) when (fex.IsCrmRateLimitException(out var retryAfter))
            {
                await Task.Delay(retryAfter, cancellationToken);
                continue;
            }

            var contacts = result.Entities
                .Select(e => e.ToEntity<Contact>())
                .ToArray();

            foreach (var contact in contacts)
            {
                await using var dbContext = await dbContextFactory.CreateDbContextAsync();
                var person = await dbContext.Persons.IgnoreQueryFilters().SingleAsync(p => p.PersonId == contact.Id);
                person.DateOfDeath = contact.dfeta_DateofDeath.ToDateOnlyWithDqtBstFix(isLocalTime: true);
                await dbContext.SaveChangesAsync();
            }

            if (result.MoreRecords)
            {
                query.PageInfo.PageNumber++;
                query.PageInfo.PagingCookie = result.PagingCookie;
            }
            else
            {
                break;
            }
        }
    }
}
