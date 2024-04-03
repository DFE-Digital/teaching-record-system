using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Core.Dqt.QueryHandlers;

public class GetQualificationsByContactIdHandler : ICrmQueryHandler<GetQualificationsByContactIdQuery, dfeta_qualification[]>
{
    public async Task<dfeta_qualification[]> Execute(GetQualificationsByContactIdQuery query, IOrganizationServiceAsync organizationService)
    {
        var filter = new FilterExpression();
        filter.AddCondition(dfeta_qualification.Fields.dfeta_PersonId, ConditionOperator.Equal, query.ContactId);
        filter.AddCondition(dfeta_qualification.Fields.StateCode, ConditionOperator.Equal, (int)dfeta_qualificationState.Active);

        var qualificationTypeFilter = new FilterExpression(LogicalOperator.Or);
        qualificationTypeFilter.AddCondition(dfeta_qualification.Fields.dfeta_Type, ConditionOperator.NotEqual, (int)dfeta_qualification_dfeta_Type.HigherEducation);

        var heFilter = new FilterExpression(LogicalOperator.And);
        heFilter.AddCondition(dfeta_qualification.Fields.dfeta_Type, ConditionOperator.Equal, (int)dfeta_qualification_dfeta_Type.HigherEducation);

        var heSubjectFilter = new FilterExpression(LogicalOperator.Or);
        heSubjectFilter.AddCondition(dfeta_qualification.Fields.dfeta_HE_HESubject1Id, ConditionOperator.NotNull);
        heSubjectFilter.AddCondition(dfeta_qualification.Fields.dfeta_HE_HESubject2Id, ConditionOperator.NotNull);
        heSubjectFilter.AddCondition(dfeta_qualification.Fields.dfeta_HE_HESubject3Id, ConditionOperator.NotNull);
        heFilter.AddFilter(heSubjectFilter);

        qualificationTypeFilter.AddFilter(heFilter);
        filter.AddFilter(qualificationTypeFilter);

        var queryExpression = new QueryExpression(dfeta_qualification.EntityLogicalName)
        {
            ColumnSet = query.ColumnSet,
            Criteria = filter,
            Orders =
            {
                new OrderExpression(dfeta_qualification.Fields.CreatedOn, OrderType.Ascending)
            }
        };

        if (query.IncludeHigherEducationDetails)
        {
            var heLink = queryExpression.AddLink(
                dfeta_hequalification.EntityLogicalName,
                dfeta_qualification.Fields.dfeta_HE_HEQualificationId,
                dfeta_hequalification.Fields.Id,
                JoinOperator.LeftOuter);

            heLink.Columns = new ColumnSet(
                dfeta_hequalification.PrimaryIdAttribute,
                dfeta_hequalification.Fields.dfeta_name);
            heLink.EntityAlias = dfeta_hequalification.EntityLogicalName;

            AddHeSubjectLink(queryExpression, dfeta_qualification.Fields.dfeta_HE_HESubject1Id, $"{dfeta_hesubject.EntityLogicalName}1");
            AddHeSubjectLink(queryExpression, dfeta_qualification.Fields.dfeta_HE_HESubject2Id, $"{dfeta_hesubject.EntityLogicalName}2");
            AddHeSubjectLink(queryExpression, dfeta_qualification.Fields.dfeta_HE_HESubject3Id, $"{dfeta_hesubject.EntityLogicalName}3");
        }

        var result = await organizationService.RetrieveMultipleAsync(queryExpression);

        return result.Entities.Select(entity => entity.ToEntity<dfeta_qualification>()).ToArray();

        void AddHeSubjectLink(QueryExpression query, string subjectIdField, string alias)
        {
            var heSubjectLink = query.AddLink(
                dfeta_hesubject.EntityLogicalName,
                subjectIdField,
                dfeta_hesubject.PrimaryIdAttribute,
                JoinOperator.LeftOuter);

            heSubjectLink.Columns = new ColumnSet(
                dfeta_hesubject.PrimaryIdAttribute,
                dfeta_hesubject.Fields.dfeta_name,
                dfeta_hesubject.Fields.dfeta_Value);
            heSubjectLink.EntityAlias = alias;
        }
    }
}
