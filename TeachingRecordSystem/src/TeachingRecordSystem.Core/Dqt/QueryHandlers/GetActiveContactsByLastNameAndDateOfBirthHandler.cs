using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Core.Dqt.QueryHandlers;

public class GetActiveContactsByLastNameAndDateOfBirthHandler : ICrmQueryHandler<GetActiveContactsByLastNameAndDateOfBirthQuery, Contact[]>
{
    public async Task<Contact[]> Execute(GetActiveContactsByLastNameAndDateOfBirthQuery query, IOrganizationServiceAsync organizationService)
    {
        var queryExpression = new QueryExpression(Contact.EntityLogicalName)
        {
            ColumnSet = query.ColumnSet,
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
                    new FilterExpression(LogicalOperator.Or)
                    {
                        Conditions =
                        {
                            new ConditionExpression(Contact.Fields.LastName, ConditionOperator.Equal, query.LastName),
                            new ConditionExpression(dfeta_previousname.EntityLogicalName, dfeta_previousname.Fields.dfeta_name, ConditionOperator.Equal, query.LastName)
                        }
                    }
                }
            },
            Orders =
            {
                new OrderExpression() { AttributeName = Contact.Fields.LastName },
                new OrderExpression() { AttributeName = Contact.Fields.FirstName },
                new OrderExpression() { AttributeName = Contact.Fields.dfeta_TRN },
            },
            LinkEntities =
            {
                new LinkEntity(
                    Contact.EntityLogicalName,
                    dfeta_previousname.EntityLogicalName,
                    Contact.PrimaryIdAttribute,
                    dfeta_previousname.Fields.dfeta_PersonId,
                    JoinOperator.LeftOuter)
                {
                    LinkCriteria = new FilterExpression()
                    {
                        Conditions =
                        {
                            new ConditionExpression(dfeta_previousname.Fields.dfeta_Type, ConditionOperator.Equal, (int)dfeta_NameType.LastName),
                            new ConditionExpression(dfeta_previousname.Fields.StateCode, ConditionOperator.Equal, (int)dfeta_previousnameState.Active)
                        }
                    }
                }
            }
        };

        var response = await organizationService.RetrieveMultipleAsync(queryExpression);

        return response.Entities
            .Select(e => e.ToEntity<Contact>())
            .GroupBy(c => c.Id)
            .Select(g => g.First())
            .ToArray();
    }
}
