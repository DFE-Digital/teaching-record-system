using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.Threading.Tasks;
using DqtApi.DataStore.Crm.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.PowerPlatform.Dataverse.Client.Utils;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace DqtApi.DataStore.Crm
{
    public partial class DataverseAdapter : IDataverseAdapter
    {
        private readonly IOrganizationServiceAsync _service;
        private readonly IClock _clock;
        private readonly IMemoryCache _cache;

        public DataverseAdapter(
            IOrganizationServiceAsync organizationServiceAsync,
            IClock clock,
            IMemoryCache cache)
        {
            _service = organizationServiceAsync;
            _clock = clock;
            _cache = cache;
        }

        public async Task<dfeta_country> GetCountry(string value)
        {
            var query = new QueryByAttribute(dfeta_country.EntityLogicalName)
            {
                ColumnSet = new() { AllColumns = true }
            };

            query.AddAttributeValue(dfeta_country.Fields.dfeta_Value, value);
            query.AddAttributeValue(dfeta_country.Fields.StateCode, (int)dfeta_countryState.Active);

            var result = await _service.RetrieveMultipleAsync(query);

            return result.Entities.Select(entity => entity.ToEntity<dfeta_country>()).FirstOrDefault();
        }

        public async Task<dfeta_earlyyearsstatus> GetEarlyYearsStatus(string value)
        {
            var query = new QueryByAttribute(dfeta_earlyyearsstatus.EntityLogicalName)
            {
                ColumnSet = new() { AllColumns = true }
            };

            query.AddAttributeValue(dfeta_earlyyearsstatus.Fields.dfeta_Value, value);
            query.AddAttributeValue(dfeta_earlyyearsstatus.Fields.StateCode, (int)dfeta_earlyyearsStatusState.Active);

            var result = await _service.RetrieveMultipleAsync(query);

            return result.Entities.Select(entity => entity.ToEntity<dfeta_earlyyearsstatus>()).FirstOrDefault();
        }

        public async Task<dfeta_hequalification> GetHeQualificationByName(string name)
        {
            var query = new QueryByAttribute(dfeta_hequalification.EntityLogicalName)
            {
                ColumnSet = new() { AllColumns = true }
            };

            query.AddAttributeValue(dfeta_hequalification.Fields.dfeta_name, name);
            query.AddAttributeValue(dfeta_hequalification.Fields.StateCode, (int)dfeta_hequalificationState.Active);

            var result = await _service.RetrieveMultipleAsync(query);

            return result.Entities.Select(entity => entity.ToEntity<dfeta_hequalification>()).FirstOrDefault();
        }

        public async Task<dfeta_hesubject> GetHeSubjectByName(string name)
        {
            var query = new QueryByAttribute(dfeta_hesubject.EntityLogicalName)
            {
                ColumnSet = new() { AllColumns = true }
            };

            query.AddAttributeValue(dfeta_hesubject.Fields.dfeta_name, name);
            query.AddAttributeValue(dfeta_hesubject.Fields.StateCode, (int)dfeta_hesubjectState.Active);

            var result = await _service.RetrieveMultipleAsync(query);

            return result.Entities.Select(entity => entity.ToEntity<dfeta_hesubject>()).FirstOrDefault();
        }

        public async Task<dfeta_initialteachertraining[]> GetInitialTeacherTrainingByTeacher(
            Guid teacherId,
            params string[] columnNames)
        {
            var query = new QueryByAttribute(dfeta_initialteachertraining.EntityLogicalName)
            {
                ColumnSet = new(columnNames)
            };

            query.AddAttributeValue(dfeta_initialteachertraining.Fields.dfeta_PersonId, teacherId);
            query.AddAttributeValue(dfeta_initialteachertraining.Fields.StateCode, (int)dfeta_initialteachertrainingState.Active);

            var result = await _service.RetrieveMultipleAsync(query);

            return result.Entities.Select(entity => entity.ToEntity<dfeta_initialteachertraining>()).ToArray();
        }

        public async Task<Account[]> GetIttProviders()
        {
            var filter = new FilterExpression(LogicalOperator.And);
            filter.AddCondition(Account.Fields.StateCode, ConditionOperator.Equal, (int)AccountState.Active);
            filter.AddCondition(Account.Fields.dfeta_TrainingProvider, ConditionOperator.Equal, true);
            filter.AddCondition(Account.Fields.dfeta_UKPRN, ConditionOperator.NotNull);

            var query = new QueryExpression(Account.EntityLogicalName)
            {
                ColumnSet = new(
                    Account.Fields.Name,
                    Account.Fields.dfeta_UKPRN),
                Criteria = filter,
                Orders =
                {
                    new OrderExpression(Account.Fields.Name, OrderType.Ascending)
                }
            };

            var result = await _service.RetrieveMultipleAsync(query);

            return result.Entities.Select(entity => entity.ToEntity<Account>()).ToArray();
        }

        public async Task<dfeta_ittsubject> GetIttSubjectByName(string name)
        {
            var query = new QueryByAttribute(dfeta_ittsubject.EntityLogicalName)
            {
                ColumnSet = new() { AllColumns = true }
            };

            query.AddAttributeValue(dfeta_ittsubject.Fields.dfeta_name, name);
            query.AddAttributeValue(dfeta_ittsubject.Fields.StateCode, (int)dfeta_ittsubjectState.Active);

            var result = await _service.RetrieveMultipleAsync(query);

            return result.Entities.Select(entity => entity.ToEntity<dfeta_ittsubject>()).FirstOrDefault();
        }

        public async Task<Contact[]> GetMatchingTeachers(GetTeacherRequest request)
        {
            var query = request.GenerateQuery();

            AddInductionLink(query);
            AddInitialTeacherTrainingLink(query);
            AddQualifiedTeacherStatusLink(query);

            var result = await _service.RetrieveMultipleAsync(query);

            return result.Entities.Select(entity => entity.ToEntity<Contact>()).ToArray();

            static void AddInitialTeacherTrainingLink(QueryExpression query)
            {
                var initialTeacherTrainingLink = query.AddLink(
                    dfeta_initialteachertraining.EntityLogicalName,
                    Contact.PrimaryIdAttribute,
                    dfeta_initialteachertraining.Fields.dfeta_PersonId,
                    JoinOperator.LeftOuter);

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

            static void AddSubjectLinks(LinkEntity initialTeacherTrainingLink)
            {
                var aliasPrefix = nameof(dfeta_initialteachertraining) + "." + nameof(dfeta_ittsubject);

                AddSubjectLink(initialTeacherTrainingLink, dfeta_initialteachertraining.Fields.dfeta_Subject1Id, aliasPrefix + 1);
                AddSubjectLink(initialTeacherTrainingLink, dfeta_initialteachertraining.Fields.dfeta_Subject2Id, aliasPrefix + 2);
                AddSubjectLink(initialTeacherTrainingLink, dfeta_initialteachertraining.Fields.dfeta_Subject3Id, aliasPrefix + 3);
            }

            static void AddSubjectLink(LinkEntity initialTeacherTrainingLink, string subjectIdField, string alias)
            {
                var subjectLink = initialTeacherTrainingLink.AddLink(
                    dfeta_ittsubject.EntityLogicalName,
                    subjectIdField,
                    dfeta_ittsubject.PrimaryIdAttribute,
                    JoinOperator.LeftOuter);

                subjectLink.Columns = new ColumnSet(dfeta_ittsubject.Fields.dfeta_Value);

                subjectLink.EntityAlias = alias;
            }

            static void AddInductionLink(QueryExpression query)
            {
                var inductionLink = query.AddLink(
                    dfeta_induction.EntityLogicalName,
                    Contact.PrimaryIdAttribute,
                    dfeta_induction.Fields.dfeta_PersonId,
                    JoinOperator.LeftOuter);

                inductionLink.Columns = new ColumnSet(
                    dfeta_induction.PrimaryIdAttribute,
                    dfeta_induction.Fields.dfeta_InductionStatus,
                    dfeta_induction.Fields.StateCode,
                    dfeta_induction.Fields.dfeta_StartDate,
                    dfeta_induction.Fields.dfeta_CompletionDate
                );

                inductionLink.EntityAlias = nameof(dfeta_induction);
            }

            static void AddQualifiedTeacherStatusLink(QueryExpression query)
            {
                var qualifiedTeacherStatusLink = query.AddLink(
                    dfeta_qtsregistration.EntityLogicalName,
                    Contact.PrimaryIdAttribute,
                    dfeta_qtsregistration.Fields.dfeta_PersonId,
                    JoinOperator.LeftOuter);

                qualifiedTeacherStatusLink.Columns = new ColumnSet(
                    dfeta_qtsregistration.PrimaryIdAttribute,
                    dfeta_qtsregistration.Fields.dfeta_name,
                    dfeta_qtsregistration.Fields.StateCode,
                    dfeta_qtsregistration.Fields.dfeta_QTSDate
                );

                qualifiedTeacherStatusLink.EntityAlias = nameof(dfeta_qtsregistration);

                var filter = new FilterExpression();
                filter.AddCondition(dfeta_initialteachertraining.Fields.StateCode, ConditionOperator.Equal, (int)dfeta_initialteachertrainingState.Active);
                qualifiedTeacherStatusLink.LinkCriteria = filter;
            }
        }

        public async Task<Account> GetOrganizationByUkprn(string ukprn, params string[] columnNames)
        {
            var query = new QueryByAttribute(Account.EntityLogicalName)
            {
                ColumnSet = new(columnNames)
            };

            query.AddAttributeValue(Account.Fields.dfeta_UKPRN, ukprn);
            query.AddAttributeValue(Account.Fields.StateCode, (int)AccountState.Active);

            var result = await _service.RetrieveMultipleAsync(query);

            return result.Entities.Select(entity => entity.ToEntity<Account>()).SingleOrDefault();
        }

        public async Task<dfeta_qtsregistration[]> GetQtsRegistrationsByTeacher(
            Guid teacherId,
            params string[] columnNames)
        {
            var query = new QueryByAttribute(dfeta_qtsregistration.EntityLogicalName)
            {
                ColumnSet = new(columnNames)
            };

            query.AddAttributeValue(dfeta_qtsregistration.Fields.dfeta_PersonId, teacherId);
            query.AddAttributeValue(dfeta_qtsregistration.Fields.StateCode, (int)dfeta_qtsregistrationState.Active);

            var result = await _service.RetrieveMultipleAsync(query);

            return result.Entities.Select(entity => entity.ToEntity<dfeta_qtsregistration>()).ToArray();
        }

        public async Task<dfeta_qualification[]> GetQualificationsForTeacher(Guid teacherId, params string[] columnNames)
        {
            var query = new QueryByAttribute(dfeta_qualification.EntityLogicalName)
            {
                ColumnSet = new ColumnSet(columnNames)
            };

            query.AddAttributeValue(dfeta_qualification.Fields.dfeta_PersonId, teacherId);

            var result = await _service.RetrieveMultipleAsync(query);

            return result.Entities.Select(entity => entity.ToEntity<dfeta_qualification>()).ToArray();
        }

        public async Task<Contact> GetTeacher(Guid teacherId, bool resolveMerges = true, params string[] columnNames)
        {
            var columnSet = new ColumnSet(
                columnNames
                    .Append(Contact.Fields.Merged)
                    .Append(Contact.Fields.MasterId)
                    .Distinct()
                    .ToArray());

            Contact teacher;

            try
            {
                teacher = (await _service.RetrieveAsync(Contact.EntityLogicalName, teacherId, columnSet)).ToEntity<Contact>();
            }
            catch (FaultException<OrganizationServiceFault> fault) when (fault.Message.Contains($"{teacherId} Does Not Exist"))
            {
                return null;
            }

            while (resolveMerges && teacher.Merged == true)
            {
                var masterReference = teacher.MasterId;
                return await GetTeacher(masterReference.Id, resolveMerges);
            }

            return teacher;
        }

        public async Task<Contact[]> GetTeachersByTrn(string trn, bool activeOnly = true, params string[] columnNames)
        {
            var filter = new FilterExpression(LogicalOperator.And);
            filter.AddCondition(Contact.Fields.dfeta_TRN, ConditionOperator.Equal, trn);

            if (activeOnly)
            {
                filter.AddCondition(Contact.Fields.StateCode, ConditionOperator.Equal, (int)ContactState.Active);
            }

            var query = new QueryExpression(Contact.EntityLogicalName)
            {
                ColumnSet = new(columnNames),
                Criteria = filter
            };

            var result = await _service.RetrieveMultipleAsync(query);

            return result.Entities.Select(e => e.ToEntity<Contact>()).ToArray();
        }

        public async Task<CrmTask[]> GetCrmTasksForTeacher(Guid teacherId, params string[] columnNames)
        {
            var query = new QueryByAttribute(CrmTask.EntityLogicalName)
            {
                ColumnSet = new ColumnSet(columnNames)
            };
            query.AddAttributeValue(CrmTask.Fields.RegardingObjectId, teacherId);

            var result = await _service.RetrieveMultipleAsync(query);

            return result.Entities.Select(entity => entity.ToEntity<CrmTask>()).ToArray();
        }

        public async Task<dfeta_teacherstatus> GetTeacherStatus(
            string value,
            bool qtsDateRequired)
        {
            // TECH DEBT Some junk reference data in the build environment means we have teacher statuses duplicated.
            // In some cases the duplicate records vary by 'dfeta_qtsdaterequired' - we need to ensure we get the correct
            // one as a workflow will prevent us allocating a qtsregistration for a status where dfeta_qtsdaterequired is true
            // without a QTS Date.

            var query = new QueryByAttribute(dfeta_teacherstatus.EntityLogicalName)
            {
                ColumnSet = new() { AllColumns = true }
            };

            query.AddAttributeValue(dfeta_teacherstatus.Fields.dfeta_Value, value);
            query.AddAttributeValue(dfeta_teacherstatus.Fields.StateCode, (int)dfeta_teacherStatusState.Active);
            query.AddAttributeValue(dfeta_teacherstatus.Fields.dfeta_QTSDateRequired, qtsDateRequired);

            var result = await _service.RetrieveMultipleAsync(query);

            return result.Entities.Select(entity => entity.ToEntity<dfeta_teacherstatus>()).FirstOrDefault();
        }

        public async Task<bool> UnlockTeacherRecord(Guid teacherId)
        {
            var update = new Entity(Contact.EntityLogicalName, teacherId);
            update[Contact.Fields.dfeta_loginfailedcounter] = 0;

            try
            {
                await _service.UpdateAsync(update);
                return true;
            }
            catch (DataverseOperationException ex)
                when (ex.InnerException is Microsoft.Rest.HttpOperationException httpException && httpException.Response.StatusCode == HttpStatusCode.NotFound)
            {
                return false;
            }
        }

        public async Task<Account> GetOrganizationByProviderName(string providerName, params string[] columnNames)
        {
            var query = new QueryByAttribute(Account.EntityLogicalName)
            {
                ColumnSet = new ColumnSet(columnNames)
            };

            query.AddAttributeValue(Account.Fields.Name, providerName);
            query.AddAttributeValue(Account.Fields.StateCode, (int)AccountState.Active);

            var result = await _service.RetrieveMultipleAsync(query);

            return result.Entities.Select(entity => entity.ToEntity<Account>()).SingleOrDefault();
        }

        public async Task<Contact[]> FindTeachers(FindTeachersQuery filter)
        {
            var fields = new[]
            {
                (FieldName: Contact.Fields.FirstName, Value: filter.FirstName),
                (FieldName: Contact.Fields.MiddleName, Value: filter.MiddleName),
                (FieldName: Contact.Fields.LastName, Value: filter.LastName),
                (FieldName: Contact.Fields.BirthDate, Value: (object)filter.DateOfBirth),
                (FieldName: Contact.Fields.dfeta_NINumber, Value: filter.NationalInsuranceNumber),
                (FieldName: Contact.Fields.EMailAddress1, Value: filter.EmailAddress),
                (FieldName: Contact.Fields.FirstName, Value: filter.PreviousFirstName),
                (FieldName: Contact.Fields.LastName, Value: filter.PreviousLastName),
            }.ToList();

            // If fields are null in the input then don't try to match them (typically MiddleName)
            fields.RemoveAll(f => f.Value == null);

            var combinations = fields.GetCombinations(length: 2).ToArray();

            if (combinations.Length == 0)
            {
                return null;
            }
            var combinationsFilter = new FilterExpression(LogicalOperator.Or);

            foreach (var combination in combinations)
            {
                var innerFilter = new FilterExpression(LogicalOperator.And);

                foreach (var (fieldName, value) in combination)
                {
                    innerFilter.AddCondition(fieldName, ConditionOperator.Equal, value);
                }

                combinationsFilter.AddFilter(innerFilter);
            }

            var query = new QueryExpression(Contact.EntityLogicalName)
            {
                ColumnSet = new ColumnSet(true),
                Criteria = combinationsFilter,
                Orders =
                {
                    new OrderExpression(Contact.Fields.LastName, OrderType.Ascending)
                }
            };

            // filter by IttProviderOrganisationId if provided
            if (filter.IttProviderOrganizationId.HasValue)
            {
                var le1 = new LinkEntity(Contact.EntityLogicalName, dfeta_initialteachertraining.EntityLogicalName, Contact.PrimaryIdAttribute, dfeta_initialteachertraining.Fields.dfeta_PersonId, JoinOperator.LeftOuter);
                le1.Columns = new ColumnSet(dfeta_initialteachertraining.Fields.dfeta_EstablishmentId);
                le1.LinkCriteria.AddCondition(dfeta_initialteachertraining.Fields.dfeta_EstablishmentId, ConditionOperator.Equal, filter.IttProviderOrganizationId);
                query.LinkEntities.Add(le1);
            }

            var result = await _service.RetrieveMultipleAsync(query);

            return result.Entities.Select(entity => entity.ToEntity<Contact>()).ToArray();
        }
    }
}
