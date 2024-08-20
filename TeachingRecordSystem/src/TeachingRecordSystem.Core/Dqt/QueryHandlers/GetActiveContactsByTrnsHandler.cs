using AngleSharp.Common;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Core.Dqt.QueryHandlers;

public class GetActiveContactsByTrnsHandler :
    ICrmQueryHandler<GetActiveContactsByTrnsQuery, IDictionary<string, Contact?>>
{
    public async Task<IDictionary<string, Contact?>> Execute(
        GetActiveContactsByTrnsQuery query,
        IOrganizationServiceAsync organizationService)
    {
        var queryExpression = new QueryExpression(Contact.EntityLogicalName)
        {
            ColumnSet = query.ColumnSet,
            Criteria = new FilterExpression(LogicalOperator.And)
            {
                Conditions =
                {
                    new ConditionExpression(Contact.Fields.StateCode, ConditionOperator.Equal, (int)ContactState.Active),
                    new ConditionExpression(Contact.Fields.dfeta_TRN, ConditionOperator.In, query.Trns.ToArray())
                }
            }
        };

        var response = await organizationService.RetrieveMultipleAsync(queryExpression);
        var contacts = response.Entities.Select(e => e.ToEntity<Contact>()).ToDictionary(c => c.dfeta_TRN, c => c);

        return query.Trns.ToDictionary(trn => trn, trn => contacts.GetValueOrDefault(trn));
    }
}
