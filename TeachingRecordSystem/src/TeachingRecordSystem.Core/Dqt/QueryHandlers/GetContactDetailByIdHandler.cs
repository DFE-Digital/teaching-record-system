using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Core.Dqt.QueryHandlers;

public class GetContactDetailByIdHandler : ICrmQueryHandler<GetContactDetailByIdQuery, ContactDetail?>
{
    public async Task<ContactDetail?> Execute(GetContactDetailByIdQuery query, IOrganizationServiceAsync organizationService)
    {
        var filter = new FilterExpression();
        filter.AddCondition(Contact.PrimaryIdAttribute, ConditionOperator.Equal, query.ContactId);

        var queryExpression = new QueryExpression(Contact.EntityLogicalName)
        {
            ColumnSet = query.ColumnSet,
            Criteria = filter
        };

        var previousNameLink = queryExpression.AddLink(
            dfeta_previousname.EntityLogicalName,
            Contact.PrimaryIdAttribute,
            dfeta_previousname.Fields.dfeta_PersonId,
            JoinOperator.LeftOuter);

        previousNameLink.Columns = new ColumnSet(
            dfeta_previousname.PrimaryIdAttribute,
            dfeta_previousname.Fields.dfeta_ChangedOn,
            dfeta_previousname.Fields.dfeta_name,
            dfeta_previousname.Fields.dfeta_Type);
        previousNameLink.EntityAlias = dfeta_previousname.EntityLogicalName;

        var previousNameFilter = new FilterExpression();
        previousNameFilter.AddCondition(dfeta_previousname.Fields.StateCode, ConditionOperator.Equal, (int)dfeta_documentState.Active);
        previousNameFilter.AddCondition(dfeta_previousname.Fields.dfeta_Type, ConditionOperator.NotEqual, (int)dfeta_NameType.Title);
        previousNameLink.LinkCriteria = previousNameFilter;

        var response = await organizationService.RetrieveMultipleAsync(queryExpression);
        if (response.Entities.Count == 0)
        {
            return null;
        }

        var contactAndPreviousNames = response.Entities.Select(e => e.ToEntity<Contact>())
            .Select(c => (Contact: c, PreviousName: c.Extract<dfeta_previousname>(dfeta_previousname.EntityLogicalName, dfeta_previousname.PrimaryIdAttribute)));

        var contactDetail = contactAndPreviousNames
            .GroupBy(c => c.Contact.Id)
            .Select(g => new ContactDetail(g.First().Contact, PreviousNames: g.Where(c => c.PreviousName != null).Select(c => c.PreviousName).ToArray()))
            .FirstOrDefault();

        return contactDetail;
    }
}
