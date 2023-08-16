using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace TeachingRecordSystem.Core.Dqt;

public partial class DataverseAdapter
{
    public async IAsyncEnumerable<InductionCompletee[]> GetInductionCompleteesForDateRange(DateTime startDate, DateTime endDate)
    {
        var filter = new FilterExpression(LogicalOperator.And);
        filter.AddCondition(dfeta_businesseventaudit.Fields.CreatedOn, ConditionOperator.GreaterEqual, startDate);
        filter.AddCondition(dfeta_businesseventaudit.Fields.CreatedOn, ConditionOperator.LessThan, endDate);
        filter.AddCondition(dfeta_businesseventaudit.Fields.dfeta_changedfield, ConditionOperator.Equal, "Induction Status");
        filter.AddCondition(dfeta_businesseventaudit.Fields.dfeta_NewValue, ConditionOperator.Equal, "Passed");
        filter.AddCondition(dfeta_businesseventaudit.Fields.dfeta_OldValue, ConditionOperator.NotEqual, "Passed");

        var query = new QueryExpression(dfeta_businesseventaudit.EntityLogicalName)
        {
            ColumnSet = new ColumnSet(
                dfeta_businesseventaudit.Fields.CreatedOn,
                dfeta_businesseventaudit.Fields.dfeta_changedfield,
                dfeta_businesseventaudit.Fields.dfeta_NewValue,
                dfeta_businesseventaudit.Fields.dfeta_OldValue,
                dfeta_businesseventaudit.Fields.dfeta_Person),
            Criteria = filter,
            Orders =
            {
                new OrderExpression(dfeta_businesseventaudit.Fields.CreatedOn, OrderType.Ascending)
            },
            PageInfo = new()
            {
                PageNumber = 1,
                Count = 500
            }
        };

        AddContactLink(query);

        EntityCollection response;

        do
        {
            response = await _service.RetrieveMultipleAsync(query);

            yield return response.Entities
                .Select(e => e.ToEntity<dfeta_businesseventaudit>())
                .Select(e => e.Extract<Contact>(Contact.EntityLogicalName, Contact.PrimaryIdAttribute))
                .GroupBy(c => c.ContactId)
                .Select(g => MapContactToInductionCompletee(g.First()))
                .ToArray();

            query.PageInfo.PageNumber++;
            query.PageInfo.PagingCookie = response.PagingCookie;
        }
        while (response.MoreRecords);

        static void AddContactLink(QueryExpression query)
        {
            var contactLink = query.AddLink(
            Contact.EntityLogicalName,
            dfeta_businesseventaudit.Fields.dfeta_Person,
            Contact.PrimaryIdAttribute,
            JoinOperator.Inner);

            contactLink.Columns = new ColumnSet(
                Contact.Fields.Id,
                Contact.Fields.dfeta_TRN,
                Contact.Fields.FirstName,
                Contact.Fields.LastName,
                Contact.Fields.dfeta_StatedFirstName,
                Contact.Fields.dfeta_StatedLastName,
                Contact.Fields.EMailAddress1,
                Contact.Fields.EMailAddress2);
            contactLink.EntityAlias = Contact.EntityLogicalName;

            var filter = new FilterExpression();
            filter.AddCondition(Contact.Fields.StateCode, ConditionOperator.Equal, (int)ContactState.Active);
            filter.AddCondition(Contact.Fields.dfeta_TRN, ConditionOperator.NotNull);
            var emailAddressFilter = new FilterExpression(LogicalOperator.Or);
            emailAddressFilter.AddCondition(Contact.Fields.EMailAddress1, ConditionOperator.NotNull);
            emailAddressFilter.AddCondition(Contact.Fields.EMailAddress2, ConditionOperator.NotNull);
            filter.AddFilter(emailAddressFilter);
            contactLink.LinkCriteria = filter;

            AddInductionLink(contactLink);
        }

        static void AddInductionLink(LinkEntity contactLink)
        {
            var inductionLink = contactLink.AddLink(
            dfeta_induction.EntityLogicalName,
            Contact.PrimaryIdAttribute,
            dfeta_induction.Fields.dfeta_PersonId,
            JoinOperator.Inner);

            inductionLink.Columns = new ColumnSet(
                dfeta_induction.Fields.dfeta_PersonId,
                dfeta_induction.Fields.dfeta_InductionStatus,
                dfeta_induction.Fields.StateCode);
            inductionLink.EntityAlias = dfeta_induction.EntityLogicalName;

            var filter = new FilterExpression();
            filter.AddCondition(dfeta_induction.Fields.StateCode, ConditionOperator.Equal, (int)dfeta_qtsregistrationState.Active);
            filter.AddCondition(dfeta_induction.Fields.dfeta_InductionStatus, ConditionOperator.Equal, (int)dfeta_InductionStatus.Pass);
            inductionLink.LinkCriteria = filter;
        }
    }

    private InductionCompletee MapContactToInductionCompletee(Contact contact)
    {
        return new InductionCompletee
        {
            TeacherId = contact.ContactId!.Value,
            Trn = contact.dfeta_TRN,
            FirstName = contact.ResolveFirstName(),
            LastName = contact.ResolveLastName(),
            EmailAddress = contact.EMailAddress1 ?? contact.EMailAddress2
        };
    }
}
