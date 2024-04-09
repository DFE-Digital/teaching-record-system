using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Core.Dqt.QueryHandlers;

public class GetActiveContactDetailByTrnHandler : ICrmQueryHandler<GetActiveContactDetailByTrnQuery, ContactDetail?>
{
    public async Task<ContactDetail?> Execute(GetActiveContactDetailByTrnQuery query, IOrganizationServiceAsync organizationService)
    {
        var contactFilter = new FilterExpression();
        contactFilter.AddCondition(Contact.Fields.dfeta_TRN, ConditionOperator.Equal, query.Trn);
        contactFilter.AddCondition(Contact.Fields.StateCode, ConditionOperator.Equal, (int)ContactState.Active);
        var contactQueryExpression = new QueryExpression(Contact.EntityLogicalName)
        {
            ColumnSet = query.ColumnSet,
            Criteria = contactFilter
        };

        var contactRequest = new RetrieveMultipleRequest()
        {
            Query = contactQueryExpression
        };

        var previousNameFilter = new FilterExpression();
        previousNameFilter.AddCondition(dfeta_previousname.Fields.StateCode, ConditionOperator.Equal, (int)dfeta_documentState.Active);
        previousNameFilter.AddCondition(dfeta_previousname.Fields.dfeta_Type, ConditionOperator.NotEqual, (int)dfeta_NameType.Title);
        var previousNameQueryExpression = new QueryExpression(dfeta_previousname.EntityLogicalName)
        {
            ColumnSet = new ColumnSet(
                dfeta_previousname.PrimaryIdAttribute,
                dfeta_previousname.Fields.dfeta_PersonId,
                dfeta_previousname.Fields.CreatedOn,
                dfeta_previousname.Fields.dfeta_ChangedOn,
                dfeta_previousname.Fields.dfeta_name,
                dfeta_previousname.Fields.dfeta_Type),
            Criteria = previousNameFilter
        };

        var contactLink = previousNameQueryExpression.AddLink(
            Contact.EntityLogicalName,
            dfeta_previousname.Fields.dfeta_PersonId,
            Contact.PrimaryIdAttribute,
            JoinOperator.Inner);

        contactLink.Columns = new ColumnSet(
            Contact.PrimaryIdAttribute,
            Contact.Fields.dfeta_TRN);

        contactLink.EntityAlias = Contact.EntityLogicalName;
        contactLink.LinkCriteria = contactFilter;

        var previousNameRequest = new RetrieveMultipleRequest()
        {
            Query = previousNameQueryExpression
        };

        var requestBuilder = RequestBuilder.CreateMultiple(organizationService);
        var contactResponse = requestBuilder.AddRequest<RetrieveMultipleResponse>(contactRequest);
        var previousNameResponse = requestBuilder.AddRequest<RetrieveMultipleResponse>(previousNameRequest);

        await requestBuilder.Execute();

        var contact = (await contactResponse.GetResponseAsync()).EntityCollection.Entities.FirstOrDefault()?.ToEntity<Contact>();
        var previousNames = (await previousNameResponse.GetResponseAsync()).EntityCollection.Entities.Select(e => e.ToEntity<dfeta_previousname>()).ToArray();

        if (contact is null)
        {
            return null;
        }

        return new ContactDetail(contact, previousNames);
    }
}
