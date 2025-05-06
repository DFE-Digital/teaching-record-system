using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;

namespace TeachingRecordSystem.Core.Dqt.QueryHandlers;

public class GetAllIttProvidersWithCorrespondingIttRecordsHandler : ICrmQueryHandler<MyDummyQuery, Account[]>
{
    public async Task<Account[]> ExecuteAsync(MyDummyQuery _, IOrganizationServiceAsync organizationService)
    {
        var filter = new FilterExpression(LogicalOperator.And);
        filter.AddCondition(Account.Fields.dfeta_TrainingProvider, ConditionOperator.Equal, true); // only get organisations that are a training provider

        var queryExpression = new QueryExpression(Account.EntityLogicalName)
        {
            ColumnSet = new(
                Account.Fields.Name,
                Account.Fields.dfeta_UKPRN),
            Criteria = filter,
            LinkEntities =
            {
                new LinkEntity // define the join
                {
                    LinkFromEntityName = Account.EntityLogicalName,
                    LinkFromAttributeName = Account.PrimaryIdAttribute,
                    LinkToEntityName = dfeta_initialteachertraining.EntityLogicalName,
                    LinkToAttributeName = dfeta_initialteachertraining.Fields.dfeta_EstablishmentId,
                    JoinOperator = JoinOperator.Inner
                }
            },
            Distinct = true, // return each record just once (like sql distinct)
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
