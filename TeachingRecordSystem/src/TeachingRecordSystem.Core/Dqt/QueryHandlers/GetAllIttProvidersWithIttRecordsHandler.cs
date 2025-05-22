using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Core.Dqt.QueryHandlers;

public class GetAllIttProvidersWithCorrespondingIttRecordsHandler : ICrmQueryHandler<GetAllIttProvidersWithCorrespondingIttRecordsPagedQuery, PagedProviderResults>
{
    public async Task<PagedProviderResults> ExecuteAsync(GetAllIttProvidersWithCorrespondingIttRecordsPagedQuery query, IOrganizationServiceAsync organizationService)
    {
        int pageSize = query.Pagesize;
        var queryExpression = new QueryExpression(Account.EntityLogicalName)
        {
            ColumnSet = new(
                Account.Fields.Name,
                Account.Fields.dfeta_UKPRN,
                Account.Fields.AccountId),
            PageInfo = new PagingInfo()
            {
                Count = pageSize,
                PageNumber = query.PageNumber,
                PagingCookie = query.PagingCookie
            },
            LinkEntities =
            {
                new LinkEntity // define the join
                {
                    LinkFromEntityName = Account.EntityLogicalName,
                    LinkFromAttributeName = Account.PrimaryIdAttribute,
                    LinkToEntityName = dfeta_initialteachertraining.EntityLogicalName,
                    LinkToAttributeName = dfeta_initialteachertraining.Fields.dfeta_EstablishmentId,
                    JoinOperator = JoinOperator.Inner,
                    LinkCriteria = new FilterExpression
                    {
                        Conditions =
                        {
                            new ConditionExpression(dfeta_initialteachertraining.Fields.StateCode, ConditionOperator.Equal, 0)
                        }
                    }
                }
            },
            Distinct = true,
            Orders =
            {
                new OrderExpression(Account.Fields.Name, OrderType.Ascending)
            }
        };

        var request = new RetrieveMultipleRequest
        {
            Query = queryExpression
        };

        var response = await organizationService.RetrieveMultipleAsync(queryExpression);
        return new PagedProviderResults(
            Providers: response.Entities.Select(x => x.ToEntity<Account>()).ToArray(),
            MoreRecords: response.MoreRecords,
            PagingCookie: response.PagingCookie);
    }
}
