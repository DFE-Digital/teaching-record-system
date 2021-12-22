using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DqtApi.Models;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Query;

namespace DqtApi.DAL
{
    public class DataverseAdaptor : IDataverseAdaptor
    {
        private readonly IOrganizationServiceAsync _service;

        public DataverseAdaptor(IOrganizationServiceAsync organizationServiceAsync)
        {
            _service = organizationServiceAsync;
        }

        public async Task<IEnumerable<Contact>> GetMatchingTeachersAsync(GetTeacherRequest request)
        {
            var query = request.GenerateQuery();

            AddInductionLink(query);
            AddInitialTeacherTrainingLink(query);
            AddQualifiedTeacherStatusLink(query);

            var result = await _service.RetrieveMultipleAsync(query);

            return result.Entities.Select(entity => entity.ToEntity<Contact>());
        }

        public async Task<IEnumerable<dfeta_qualification>> GetQualificationsAsync(Guid teacherId)
        {
            var query = new QueryByAttribute(dfeta_qualification.EntityLogicalName)
            {
                ColumnSet = new ColumnSet(
                    dfeta_qualification.Fields.dfeta_CompletionorAwardDate,
                    dfeta_qualification.Fields.dfeta_Type
                )
            };

            query.AddAttributeValue(dfeta_qualification.Fields.dfeta_PersonId, teacherId);

            var result = await _service.RetrieveMultipleAsync(query);

            return result.Entities.Select(entity => entity.ToEntity<dfeta_qualification>());
        }

        public async Task<Contact> GetTeacherAsync(Guid teacherId)
        {
            var filter = new FilterExpression();
            filter.AddCondition(Contact.PrimaryIdAttribute, ConditionOperator.Equal, teacherId);

            var query = new QueryExpression(Contact.EntityLogicalName)
            {
                ColumnSet = new ColumnSet { AllColumns = true },
                Criteria = filter
            };

            var result = await _service.RetrieveMultipleAsync(query);

            return result.Entities.FirstOrDefault()?.ToEntity<Contact>();
        }

        private static void AddQualifiedTeacherStatusLink(QueryExpression query)
        {
            var qualifiedTeacherStatusLink = query.AddLink(dfeta_qtsregistration.EntityLogicalName, Contact.PrimaryIdAttribute,
                            dfeta_qtsregistration.Fields.dfeta_PersonId, JoinOperator.LeftOuter);

            qualifiedTeacherStatusLink.Columns = new ColumnSet(
                dfeta_qtsregistration.PrimaryIdAttribute,
                dfeta_qtsregistration.Fields.dfeta_name,
                dfeta_qtsregistration.Fields.StateCode,
                dfeta_qtsregistration.Fields.dfeta_QTSDate
            );

            qualifiedTeacherStatusLink.EntityAlias = nameof(dfeta_qtsregistration);
        }

        private static void AddInitialTeacherTrainingLink(QueryExpression query)
        {
            var initialTeacherTrainingLink = query.AddLink(dfeta_initialteachertraining.EntityLogicalName, Contact.PrimaryIdAttribute,
                            dfeta_initialteachertraining.Fields.dfeta_PersonId, JoinOperator.LeftOuter);

            initialTeacherTrainingLink.Columns = new ColumnSet(
                dfeta_initialteachertraining.PrimaryIdAttribute,
                dfeta_initialteachertraining.Fields.StateCode,
                dfeta_initialteachertraining.Fields.dfeta_ProgrammeType,
                dfeta_initialteachertraining.Fields.dfeta_ITTQualificationId,
                dfeta_initialteachertraining.Fields.dfeta_Result,
                dfeta_initialteachertraining.Fields.dfeta_Subject1Id,
                dfeta_initialteachertraining.Fields.dfeta_Subject2Id,
                dfeta_initialteachertraining.Fields.dfeta_Subject3Id,
                dfeta_initialteachertraining.Fields.dfeta_ProgrammeStartDate,
                dfeta_initialteachertraining.Fields.dfeta_ProgrammeEndDate
            );

            initialTeacherTrainingLink.EntityAlias = nameof(dfeta_initialteachertraining);

            AddSubjectLinks(initialTeacherTrainingLink);
        }

        private static void AddSubjectLinks(LinkEntity initialTeacherTrainingLink)
        {
            var aliasPrefix = nameof(dfeta_initialteachertraining) + "." + nameof(dfeta_ittsubject);

            AddSubjectLink(initialTeacherTrainingLink, dfeta_initialteachertraining.Fields.dfeta_Subject1Id, aliasPrefix + 1);
            AddSubjectLink(initialTeacherTrainingLink, dfeta_initialteachertraining.Fields.dfeta_Subject2Id, aliasPrefix + 2);
            AddSubjectLink(initialTeacherTrainingLink, dfeta_initialteachertraining.Fields.dfeta_Subject3Id, aliasPrefix + 3);            
        }

        private static void AddSubjectLink(LinkEntity initialTeacherTrainingLink, string subjectIdField, string alias)
        {
            var subjectLink = initialTeacherTrainingLink.AddLink(dfeta_ittsubject.EntityLogicalName, subjectIdField,
                    dfeta_ittsubject.PrimaryIdAttribute, JoinOperator.LeftOuter);

            subjectLink.Columns = new ColumnSet(dfeta_ittsubject.Fields.dfeta_Value);

            subjectLink.EntityAlias = alias;
        }

        private static void AddInductionLink(QueryExpression query)
        {
            var inductionLink = query.AddLink(dfeta_induction.EntityLogicalName, Contact.PrimaryIdAttribute,
                            dfeta_induction.Fields.dfeta_PersonId, JoinOperator.LeftOuter);

            inductionLink.Columns = new ColumnSet(
                dfeta_induction.PrimaryIdAttribute,
                dfeta_induction.Fields.dfeta_InductionStatus,
                dfeta_induction.Fields.StateCode,
                dfeta_induction.Fields.dfeta_StartDate,
                dfeta_induction.Fields.dfeta_CompletionDate
            );

            inductionLink.EntityAlias = nameof(dfeta_induction);
        }
    }
}
