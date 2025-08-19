#nullable disable
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Query;

namespace TeachingRecordSystem.Core.Dqt;

public class DataverseAdapter(IOrganizationServiceAsync organizationServiceAsync) : IDataverseAdapter
{
    public async Task<dfeta_qualification[]> GetQualificationsForTeacherAsync(
        Guid teacherId,
        string[] columnNames,
        string[] heQualificationColumnNames = null,
        string[] heSubjectColumnNames = null)
    {
        var filter = new FilterExpression();
        filter.AddCondition(dfeta_qualification.Fields.dfeta_PersonId, ConditionOperator.Equal, teacherId);
        filter.AddCondition(dfeta_qualification.Fields.StateCode, ConditionOperator.Equal, (int)dfeta_qualificationState.Active);

        var query = new QueryExpression(dfeta_qualification.EntityLogicalName)
        {
            ColumnSet = new ColumnSet(columnNames),
            Criteria = filter,
            Orders =
            {
                new OrderExpression(dfeta_qualification.Fields.CreatedOn, OrderType.Ascending)
            }
        };

        if (heQualificationColumnNames?.Length > 0)
        {
            AddHeQualificationLink(query, heQualificationColumnNames);
        }

        if (heSubjectColumnNames?.Length > 0)
        {
            AddSubjectLinks(query, heSubjectColumnNames);
        }

        var result = await organizationServiceAsync.RetrieveMultipleAsync(query);

        return result.Entities.Select(entity => entity.ToEntity<dfeta_qualification>()).ToArray();

        static void AddHeQualificationLink(QueryExpression query, string[] columnNames)
        {
            var heSubjectLink = query.AddLink(
                dfeta_hequalification.EntityLogicalName,
                dfeta_qualification.Fields.dfeta_HE_HEQualificationId,
                dfeta_hequalification.Fields.Id,
                JoinOperator.LeftOuter);

            heSubjectLink.Columns = new ColumnSet(columnNames);
            heSubjectLink.EntityAlias = dfeta_hequalification.EntityLogicalName;
        }

        static void AddSubjectLinks(QueryExpression query, string[] columnNames)
        {
            var aliasPrefix = dfeta_hesubject.EntityLogicalName;

            AddSubjectLink(query, dfeta_qualification.Fields.dfeta_HE_HESubject1Id, aliasPrefix + 1, columnNames);
            AddSubjectLink(query, dfeta_qualification.Fields.dfeta_HE_HESubject2Id, aliasPrefix + 2, columnNames);
            AddSubjectLink(query, dfeta_qualification.Fields.dfeta_HE_HESubject3Id, aliasPrefix + 3, columnNames);
        }

        static void AddSubjectLink(QueryExpression query, string subjectIdField, string alias, string[] columnNames)
        {
            var subjectLink = query.AddLink(
                dfeta_hesubject.EntityLogicalName,
                subjectIdField,
                dfeta_hesubject.PrimaryIdAttribute,
                JoinOperator.LeftOuter);

            subjectLink.Columns = new ColumnSet(columnNames);
            subjectLink.EntityAlias = alias;
        }
    }
}
