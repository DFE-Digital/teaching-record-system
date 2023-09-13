using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Core.Dqt.QueryHandlers;

public class GetContactsByDateOfBirthHandler : ICrmQueryHandler<GetContactsByDateOfBirthQuery, Contact[]>
{
    public async Task<Contact[]> Execute(GetContactsByDateOfBirthQuery query, IOrganizationServiceAsync organizationService)
    {
        var queryByAttribute = new QueryByAttribute()
        {
            EntityName = Contact.EntityLogicalName,
            ColumnSet = query.ColumnSet,
            TopCount = query.MaxRecordCount
        };
        queryByAttribute.AddAttributeValue(Contact.Fields.StateCode, (int)ContactState.Active);
        queryByAttribute.AddAttributeValue(Contact.Fields.BirthDate, query.DateOfBirth.ToDateTime());

        var response = await organizationService.RetrieveMultipleAsync(queryByAttribute);
        return response.Entities.Select(e => e.ToEntity<Contact>()).ToArray();
    }
}
