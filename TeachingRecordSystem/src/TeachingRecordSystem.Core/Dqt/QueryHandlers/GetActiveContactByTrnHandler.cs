using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Core.Dqt.QueryHandlers;

public class GetActiveContactByTrnHandler : ICrmQueryHandler<GetActiveContactByTrnQuery, Contact?>
{
    public async Task<Contact?> Execute(GetActiveContactByTrnQuery query, IOrganizationServiceAsync organizationService)
    {
        var queryByAttribute = new QueryByAttribute()
        {
            EntityName = Contact.EntityLogicalName,
            ColumnSet = query.ColumnSet
        };
        queryByAttribute.AddAttributeValue(Contact.Fields.StateCode, (int)ContactState.Active);
        queryByAttribute.AddAttributeValue(Contact.Fields.dfeta_TRN, query.Trn);

        var response = await organizationService.RetrieveMultipleAsync(queryByAttribute);

        return response.Entities.SingleOrDefault()?.ToEntity<Contact>();
    }
}
