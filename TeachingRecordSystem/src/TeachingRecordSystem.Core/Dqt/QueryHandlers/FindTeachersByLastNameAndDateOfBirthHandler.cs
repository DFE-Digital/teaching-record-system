using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Core.Dqt.QueryHandlers;

public class FindTeachersByLastNameAndDateOfBirthHandler : ICrmQueryHandler<FindTeachersByLastNameAndDateOfBirthQuery, Contact[]>
{
    public async Task<Contact[]> Execute(FindTeachersByLastNameAndDateOfBirthQuery query, IOrganizationServiceAsync organizationService)
    {
        //Find all the permutations of names to match on
        var lastNameFilter = new FilterExpression(LogicalOperator.Or);
        lastNameFilter.AddCondition(Contact.Fields.LastName, ConditionOperator.Equal, query.LastName);
        lastNameFilter.AddCondition(Contact.Fields.dfeta_PreviousLastName, ConditionOperator.Equal, query.LastName);

        var queryExpression = new QueryExpression()
        {
            ColumnSet = query.ColumnSet,
            EntityName = Contact.EntityLogicalName,
            Criteria = new FilterExpression(LogicalOperator.And)
            {
                Conditions =
                    {
                        new ConditionExpression(Contact.Fields.StateCode, ConditionOperator.Equal, (int)ContactState.Active),
                        new ConditionExpression(Contact.Fields.dfeta_TRN, ConditionOperator.NotNull),
                        new ConditionExpression(Contact.Fields.BirthDate, ConditionOperator.Equal, query.DateOfBirth.ToDateTime()),
                    },
                Filters =
                    {
                        lastNameFilter
                    }
            },
            Orders =
                {
                    new OrderExpression() { AttributeName = Contact.Fields.LastName },
                    new OrderExpression() { AttributeName = Contact.Fields.FirstName },
                    new OrderExpression() { AttributeName = Contact.Fields.dfeta_TRN },
                }
        };

        var response = await organizationService.RetrieveMultipleAsync(queryExpression);

        return response.Entities.Select(e => e.ToEntity<Contact>()).ToArray();
    }
}
