using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Core.Dqt.QueryHandlers;

public class GetContactByTrnRequestIdHandler : ICrmQueryHandler<GetContactByTrnRequestIdQuery, Contact?>
{
    public async Task<Contact?> Execute(GetContactByTrnRequestIdQuery query, IOrganizationServiceAsync organizationService)
    {
        var queryByAttribute = new QueryByAttribute()
        {
            EntityName = Contact.EntityLogicalName,
            ColumnSet = query.ColumnSet
        };
        queryByAttribute.AddAttributeValue(Contact.Fields.dfeta_TrnRequestID, query.TrnRequestId);

        var response = await organizationService.RetrieveMultipleAsync(queryByAttribute);

        return response.Entities.SingleOrDefault()?.ToEntity<Contact>();
    }
}
