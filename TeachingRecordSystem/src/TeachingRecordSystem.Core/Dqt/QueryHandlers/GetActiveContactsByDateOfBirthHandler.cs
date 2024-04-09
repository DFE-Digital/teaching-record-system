using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Core.Dqt.QueryHandlers;

public class GetActiveContactsByDateOfBirthHandler : ICrmQueryHandler<GetActiveContactsByDateOfBirthQuery, Contact[]>
{
    public async Task<Contact[]> Execute(GetActiveContactsByDateOfBirthQuery query, IOrganizationServiceAsync organizationService)
    {
        var queryByAttribute = new QueryByAttribute()
        {
            EntityName = Contact.EntityLogicalName,
            ColumnSet = query.ColumnSet,
            TopCount = query.MaxRecordCount
        };

        queryByAttribute.AddAttributeValue(Contact.Fields.StateCode, (int)ContactState.Active);
        queryByAttribute.AddAttributeValue(Contact.Fields.BirthDate, query.DateOfBirth.ToDateTime());

        switch (query.SortBy)
        {
            case ContactSearchSortByOption.LastNameAscending:
                queryByAttribute.AddOrder(Contact.Fields.LastName, OrderType.Ascending);
                break;
            case ContactSearchSortByOption.LastNameDescending:
                queryByAttribute.AddOrder(Contact.Fields.LastName, OrderType.Descending);
                break;
            case ContactSearchSortByOption.FirstNameAscending:
                queryByAttribute.AddOrder(Contact.Fields.FirstName, OrderType.Ascending);
                break;
            case ContactSearchSortByOption.FirstNameDescending:
                queryByAttribute.AddOrder(Contact.Fields.FirstName, OrderType.Descending);
                break;
            case ContactSearchSortByOption.DateOfBirthAscending:
                queryByAttribute.AddOrder(Contact.Fields.BirthDate, OrderType.Ascending);
                break;
            case ContactSearchSortByOption.DateOfBirthDescending:
                queryByAttribute.AddOrder(Contact.Fields.BirthDate, OrderType.Descending);
                break;
            default:
                break;
        }

        var response = await organizationService.RetrieveMultipleAsync(queryByAttribute);
        return response.Entities.Select(e => e.ToEntity<Contact>()).ToArray();
    }
}
