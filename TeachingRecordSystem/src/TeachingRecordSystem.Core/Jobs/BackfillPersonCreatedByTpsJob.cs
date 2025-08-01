using System.ServiceModel;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Dqt;

namespace TeachingRecordSystem.Core.Jobs;

public class BackfillPersonCreatedByTpsJob(IOrganizationServiceAsync2 organizationService, TrsDbContext dbContext)
{
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        const int pageSize = 100;
        var query = new QueryExpression(Contact.EntityLogicalName)
        {
            ColumnSet = new ColumnSet(
                Contact.Fields.ContactId,
                Contact.PrimaryIdAttribute,
                Contact.Fields.dfeta_CapitaTRNChangedOn
            ),
            PageInfo = new PagingInfo()
            {
                Count = pageSize,
                PageNumber = 1
            },
        };
        query.Criteria.AddCondition(Contact.Fields.dfeta_TRN, ConditionOperator.NotNull);
        query.Criteria.AddCondition(Contact.Fields.dfeta_CapitaTRNChangedOn, ConditionOperator.Null);

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

            foreach (var item in result.Entities)
            {
                var person = dbContext.Persons.FirstOrDefault(x => x.PersonId == item.Id);
                if (person != null)
                {
                    person.CreatedByTps = true;
                }
            }
            await dbContext.SaveChangesAsync();

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
