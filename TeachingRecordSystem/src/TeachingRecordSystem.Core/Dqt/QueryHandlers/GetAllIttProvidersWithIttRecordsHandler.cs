using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Core.Dqt.QueryHandlers;

public class GetAllIttProvidersWithCorrespondingIttRecordsHandler : ICrmQueryHandler<GetAllIttProvidersWithCorrespondingIttRecordsQuery, Account[]>
{
    public async Task<Account[]> ExecuteAsync(GetAllIttProvidersWithCorrespondingIttRecordsQuery _, IOrganizationServiceAsync organizationService)
    {
        var queryExpression = new QueryExpression(Account.EntityLogicalName)
        {
            ColumnSet = new(
                Account.Fields.Name,
                Account.Fields.dfeta_UKPRN,
                Account.Fields.AccountId),
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
            Distinct = true, // return each account record just once
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
        return response.Entities.Select(x => x.ToEntity<Account>()).ToArray();
    }
}
