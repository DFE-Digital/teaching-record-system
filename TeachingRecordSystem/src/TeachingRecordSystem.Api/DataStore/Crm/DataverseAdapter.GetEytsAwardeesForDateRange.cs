﻿using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Api.DataStore.Crm.Models;

namespace TeachingRecordSystem.Api.DataStore.Crm;

public partial class DataverseAdapter
{
    public async Task<EytsAwardee[]> GetEytsAwardeesForDateRange(DateTime startDate, DateTime endDate)
    {
        var filter = new FilterExpression(LogicalOperator.And);
        filter.AddCondition(dfeta_businesseventaudit.Fields.CreatedOn, ConditionOperator.GreaterEqual, startDate);
        filter.AddCondition(dfeta_businesseventaudit.Fields.CreatedOn, ConditionOperator.LessThan, endDate);
        filter.AddCondition(dfeta_businesseventaudit.Fields.dfeta_changedfield, ConditionOperator.Equal, "EYTS Date");
        filter.AddCondition(dfeta_businesseventaudit.Fields.dfeta_NewValue, ConditionOperator.NotNull);
        filter.AddCondition(dfeta_businesseventaudit.Fields.dfeta_OldValue, ConditionOperator.Null);

        var query = new QueryExpression(dfeta_businesseventaudit.EntityLogicalName)
        {
            ColumnSet = new ColumnSet(
                dfeta_businesseventaudit.Fields.CreatedOn,
                dfeta_businesseventaudit.Fields.dfeta_changedfield,
                dfeta_businesseventaudit.Fields.dfeta_NewValue,
                dfeta_businesseventaudit.Fields.dfeta_OldValue,
                dfeta_businesseventaudit.Fields.dfeta_Person),
            Criteria = filter
        };

        AddContactLink(query);

        var result = await _service.RetrieveMultipleAsync(query);
        return result.Entities
            .Select(e => e.ToEntity<dfeta_businesseventaudit>())
            .Select(e => e.Extract<Contact>(Contact.EntityLogicalName, Contact.PrimaryIdAttribute))
            .GroupBy(c => c.ContactId)
            .Select(g => MapContactToEytsAwardee(g.First()))
            .ToArray();

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

            AddQtsRegistrationLink(contactLink);
        }

        static void AddQtsRegistrationLink(LinkEntity contactLink)
        {
            var qtsRegistrationLink = contactLink.AddLink(
            dfeta_qtsregistration.EntityLogicalName,
            Contact.PrimaryIdAttribute,
            dfeta_qtsregistration.Fields.dfeta_PersonId,
            JoinOperator.Inner);

            qtsRegistrationLink.Columns = new ColumnSet(
                dfeta_qtsregistration.Fields.dfeta_PersonId,
                dfeta_qtsregistration.Fields.dfeta_EYTSDate,
                dfeta_qtsregistration.Fields.StateCode);
            qtsRegistrationLink.EntityAlias = dfeta_qtsregistration.EntityLogicalName;

            var filter = new FilterExpression();
            filter.AddCondition(dfeta_qtsregistration.Fields.StateCode, ConditionOperator.Equal, (int)dfeta_qtsregistrationState.Active);
            qtsRegistrationLink.LinkCriteria = filter;
        }
    }

    private EytsAwardee MapContactToEytsAwardee(Contact contact)
    {
        return new EytsAwardee
        {
            TeacherId = contact.ContactId!.Value,
            Trn = contact.dfeta_TRN,
            FirstName = contact.ResolveFirstName(),
            LastName = contact.ResolveLastName(),
            EmailAddress = contact.EMailAddress1 ?? contact.EMailAddress2
        };
    }
}
