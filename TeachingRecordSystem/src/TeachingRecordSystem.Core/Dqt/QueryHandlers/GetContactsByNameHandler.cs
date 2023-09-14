using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Core.Dqt.QueryHandlers;

public class GetContactsByNameHandler : ICrmQueryHandler<GetContactsByNameQuery, Contact[]>
{
    public async Task<Contact[]> Execute(GetContactsByNameQuery query, IOrganizationServiceAsync organizationService)
    {
        var filter = new FilterExpression(LogicalOperator.Or);
        filter.AddCondition(Contact.Fields.FirstName, ConditionOperator.Like, $"{query.Name}%");
        filter.AddCondition(Contact.Fields.MiddleName, ConditionOperator.Like, $"{query.Name}%");
        filter.AddCondition(Contact.Fields.LastName, ConditionOperator.Like, $"{query.Name}%");
        filter.AddCondition(Contact.Fields.FullName, ConditionOperator.Like, $"{query.Name}%");

        var queryExpression = new QueryExpression(Contact.EntityLogicalName)
        {
            ColumnSet = query.ColumnSet,
            Criteria = filter,
            TopCount = query.MaxRecordCount,
        };

        switch (query.SortBy)
        {
            case ContactSearchSortByOption.LastNameAscending:
                queryExpression.AddOrder(Contact.Fields.LastName, OrderType.Ascending);
                break;
            case ContactSearchSortByOption.LastNameDescending:
                queryExpression.AddOrder(Contact.Fields.LastName, OrderType.Descending);
                break;
            case ContactSearchSortByOption.FirstNameAscending:
                queryExpression.AddOrder(Contact.Fields.FirstName, OrderType.Ascending);
                break;
            case ContactSearchSortByOption.FirstNameDescending:
                queryExpression.AddOrder(Contact.Fields.FirstName, OrderType.Descending);
                break;
            case ContactSearchSortByOption.DateOfBirthAscending:
                queryExpression.AddOrder(Contact.Fields.BirthDate, OrderType.Ascending);
                break;
            case ContactSearchSortByOption.DateOfBirthDescending:
                queryExpression.AddOrder(Contact.Fields.BirthDate, OrderType.Descending);
                break;
            default:
                break;
        }

        var response = await organizationService.RetrieveMultipleAsync(queryExpression);
        return response.Entities.Select(e => e.ToEntity<Contact>()).ToArray();
    }
}
