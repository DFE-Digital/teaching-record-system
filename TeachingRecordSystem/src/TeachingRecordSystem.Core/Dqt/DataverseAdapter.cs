#nullable disable
using System.Diagnostics;
using System.ServiceModel;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Core.Services.TrnGenerationApi;

namespace TeachingRecordSystem.Core.Dqt;

public partial class DataverseAdapter : IDataverseAdapter
{
    private readonly IOrganizationServiceAsync _service;
    private readonly IClock _clock;
    private readonly IMemoryCache _cache;
    private readonly ITrnGenerationApiClient _trnGenerationApiClient;

    public DataverseAdapter(
        IOrganizationServiceAsync organizationServiceAsync,
        IClock clock,
        IMemoryCache cache,
        ITrnGenerationApiClient trnGenerationApiClient)
    {
        _service = organizationServiceAsync;
        _clock = clock;
        _cache = cache;
        _trnGenerationApiClient = trnGenerationApiClient;
    }

    public Task<string> GenerateTrn() => _trnGenerationApiClient.GenerateTrn();

    public Task<dfeta_country> GetCountry(string value) => GetCountry(value, requestBuilder: null);

    public async Task<dfeta_country> GetCountry(string value, RequestBuilder requestBuilder)
    {
        requestBuilder ??= RequestBuilder.CreateSingle(_service);

        var query = new QueryByAttribute(dfeta_country.EntityLogicalName)
        {
            ColumnSet = new() { AllColumns = true }
        };

        query.AddAttributeValue(dfeta_country.Fields.dfeta_Value, value);
        query.AddAttributeValue(dfeta_country.Fields.StateCode, (int)dfeta_countryState.Active);

        var request = new RetrieveMultipleRequest()
        {
            Query = query
        };

        var result = await requestBuilder.AddRequest<RetrieveMultipleResponse>(request).GetResponseAsync();

        return result.EntityCollection.Entities.Select(entity => entity.ToEntity<dfeta_country>()).FirstOrDefault();
    }

    public Task<dfeta_earlyyearsstatus> GetEarlyYearsStatus(Guid earlyYearsStatusId) => GetEarlyYearsStatus(earlyYearsStatusId, requestBuilder: null);

    public async Task<dfeta_earlyyearsstatus> GetEarlyYearsStatus(Guid earlyYearsStatusId, RequestBuilder requestBuilder)
    {
        requestBuilder ??= RequestBuilder.CreateSingle(_service);

        var query = new QueryByAttribute(dfeta_earlyyearsstatus.EntityLogicalName)
        {
            ColumnSet = new() { AllColumns = true }
        };

        query.AddAttributeValue(dfeta_earlyyearsstatus.PrimaryIdAttribute, earlyYearsStatusId);
        query.AddAttributeValue(dfeta_earlyyearsstatus.Fields.StateCode, (int)dfeta_earlyyearsStatusState.Active);

        var request = new RetrieveMultipleRequest()
        {
            Query = query
        };

        var result = await requestBuilder.AddRequest<RetrieveMultipleResponse>(request).GetResponseAsync();

        return result.EntityCollection.Entities.Select(entity => entity.ToEntity<dfeta_earlyyearsstatus>()).FirstOrDefault();
    }

    public async Task<dfeta_earlyyearsstatus> GetEarlyYearsStatus(string value, RequestBuilder requestBuilder)
    {
        var all = await GetAllEarlyYearsStatuses(requestBuilder);
        return all.SingleOrDefault(x => x.dfeta_Value == value);
    }

    public async Task<List<dfeta_earlyyearsstatus>> GetAllEarlyYearsStatuses(RequestBuilder requestBuilder)
    {
        return await _cache.GetOrCreate(CacheKeys.GetAllEytsStatuses(), async _ =>
        {
            requestBuilder ??= RequestBuilder.CreateSingle(_service);

            var query = new QueryByAttribute(dfeta_earlyyearsstatus.EntityLogicalName)
            {
                ColumnSet = new() { AllColumns = true }
            };
            query.AddAttributeValue(dfeta_earlyyearsstatus.Fields.StateCode, (int)dfeta_earlyyearsStatusState.Active);

            var request = new RetrieveMultipleRequest()
            {
                Query = query
            };

            var result = await requestBuilder.AddRequest<RetrieveMultipleResponse>(request).GetResponseAsync();

            return result.EntityCollection.Entities.Select(entity => entity.ToEntity<dfeta_earlyyearsstatus>()).ToList();
        });
    }

    public Task<dfeta_hequalification> GetHeQualificationByCode(string value) => GetHeQualificationByCode(value, requestBuilder: null);

    public async Task<dfeta_hequalification> GetHeQualificationByCode(string value, RequestBuilder requestBuilder)
    {
        requestBuilder ??= RequestBuilder.CreateSingle(_service);

        var query = new QueryByAttribute(dfeta_hequalification.EntityLogicalName)
        {
            ColumnSet = new() { AllColumns = true }
        };

        query.AddAttributeValue(dfeta_hequalification.Fields.dfeta_Value, value);
        query.AddAttributeValue(dfeta_hequalification.Fields.StateCode, (int)dfeta_hequalificationState.Active);

        var request = new RetrieveMultipleRequest()
        {
            Query = query
        };

        var result = await requestBuilder.AddRequest<RetrieveMultipleResponse>(request).GetResponseAsync();

        return result.EntityCollection.Entities.Select(entity => entity.ToEntity<dfeta_hequalification>()).FirstOrDefault();
    }

    public Task<dfeta_hesubject> GetHeSubjectByCode(string value) => GetHeSubjectByCode(value, requestBuilder: null);

    public async Task<dfeta_hesubject> GetHeSubjectByCode(string value, RequestBuilder requestBuilder)
    {
        requestBuilder ??= RequestBuilder.CreateSingle(_service);

        var query = new QueryByAttribute(dfeta_hesubject.EntityLogicalName)
        {
            ColumnSet = new() { AllColumns = true }
        };

        query.AddAttributeValue(dfeta_hesubject.Fields.dfeta_Value, value);
        query.AddAttributeValue(dfeta_hesubject.Fields.StateCode, (int)dfeta_hesubjectState.Active);

        var request = new RetrieveMultipleRequest()
        {
            Query = query
        };

        var result = await requestBuilder.AddRequest<RetrieveMultipleResponse>(request).GetResponseAsync();

        return result.EntityCollection.Entities.Select(entity => entity.ToEntity<dfeta_hesubject>()).FirstOrDefault();
    }

    public Task<dfeta_initialteachertraining[]> GetInitialTeacherTrainingByTeacher(
        Guid teacherId,
        string[] columnNames,
        string[] establishmentColumnNames = null,
        string[] subjectColumnNames = null,
        string[] qualificationColumnNames = null,
        bool activeOnly = true) =>
        GetInitialTeacherTrainingByTeacher(teacherId, columnNames, establishmentColumnNames, subjectColumnNames, qualificationColumnNames, requestBuilder: null, activeOnly);

    public async Task<dfeta_initialteachertraining[]> GetInitialTeacherTrainingBySlugId(string slugId, string[] columnNames, RequestBuilder requestBuilder, bool activeOnly = true)
    {
        requestBuilder ??= RequestBuilder.CreateSingle(_service);

        var query = new QueryExpression(dfeta_initialteachertraining.EntityLogicalName)
        {
            ColumnSet = new(columnNames)
        };

        query.Criteria.AddCondition(dfeta_initialteachertraining.Fields.dfeta_SlugId, ConditionOperator.Equal, slugId);

        if (activeOnly == true)
        {
            query.Criteria.AddCondition(dfeta_initialteachertraining.Fields.StateCode, ConditionOperator.Equal, (int)dfeta_initialteachertrainingState.Active);
        }

        var request = new RetrieveMultipleRequest()
        {
            Query = query
        };

        var result = await requestBuilder.AddRequest<RetrieveMultipleResponse>(request).GetResponseAsync();

        return result.EntityCollection.Entities.Select(entity => entity.ToEntity<dfeta_initialteachertraining>()).ToArray();
    }

    public async Task<Contact[]> GetTeachersByInitialTeacherTrainingSlugId(string slugId, string[] columnNames, RequestBuilder requestBuilder, bool activeOnly = true)
    {
        requestBuilder ??= RequestBuilder.CreateSingle(_service);
        var query = new QueryExpression(Contact.EntityLogicalName)
        {
            ColumnSet = new ColumnSet(new string[] { Contact.Fields.dfeta_TRN })
        };

        if (activeOnly == true)
        {
            query.Criteria.AddCondition(Contact.Fields.StateCode, ConditionOperator.Equal, (int)ContactState.Active);
        }

        AddContactLink(query, Contact.EntityLogicalCollectionName, new string[] { dfeta_initialteachertraining.Fields.dfeta_SlugId }, slugId);

        var request = new RetrieveMultipleRequest()
        {
            Query = query
        };

        var result = await requestBuilder.AddRequest<RetrieveMultipleResponse>(request).GetResponseAsync();


        return result.EntityCollection.Entities.Select(entity => entity.ToEntity<Contact>()).ToArray();

        static void AddContactLink(QueryExpression query, string alias, string[] columnNames, string slugId)
        {
            var contactLink = query.AddLink(
                dfeta_initialteachertraining.EntityLogicalName,
                Contact.PrimaryIdAttribute,
                dfeta_initialteachertraining.Fields.dfeta_PersonId,
                JoinOperator.Inner);

            contactLink.Columns = new ColumnSet(columnNames);
            contactLink.EntityAlias = alias;
            contactLink.LinkCriteria.AddCondition(dfeta_initialteachertraining.Fields.dfeta_SlugId, ConditionOperator.Equal, slugId);
        }
    }

    public async Task<dfeta_initialteachertraining[]> GetInitialTeacherTrainingByTeacher(
        Guid teacherId,
        string[] columnNames,
        string[] establishmentColumnNames,
        string[] subjectColumnNames,
        string[] qualificationColumnNames,
        RequestBuilder requestBuilder,
        bool activeOnly = true)
    {
        requestBuilder ??= RequestBuilder.CreateSingle(_service);

        var query = new QueryExpression(dfeta_initialteachertraining.EntityLogicalName)
        {
            ColumnSet = new(columnNames)
        };

        query.Criteria.AddCondition(dfeta_initialteachertraining.Fields.dfeta_PersonId, ConditionOperator.Equal, teacherId);

        if (activeOnly == true)
        {
            query.Criteria.AddCondition(dfeta_initialteachertraining.Fields.StateCode, ConditionOperator.Equal, (int)dfeta_initialteachertrainingState.Active);
        }

        if (establishmentColumnNames?.Length > 0)
        {
            var establishmentLink = query.AddLink(
                Account.EntityLogicalName,
                dfeta_initialteachertraining.Fields.dfeta_EstablishmentId,
                Account.PrimaryIdAttribute,
                JoinOperator.LeftOuter);

            establishmentLink.Columns = new ColumnSet(establishmentColumnNames);

            establishmentLink.EntityAlias = "establishment";

            var filter = new FilterExpression();
            filter.AddCondition(Account.Fields.StateCode, ConditionOperator.Equal, (int)AccountState.Active);
            establishmentLink.LinkCriteria = filter;
        }

        if (subjectColumnNames?.Length > 0)
        {
            AddSubjectLink(query, dfeta_initialteachertraining.Fields.dfeta_Subject1Id, "subject1", subjectColumnNames);
            AddSubjectLink(query, dfeta_initialteachertraining.Fields.dfeta_Subject2Id, "subject2", subjectColumnNames);
            AddSubjectLink(query, dfeta_initialteachertraining.Fields.dfeta_Subject3Id, "subject3", subjectColumnNames);
        }

        if (qualificationColumnNames?.Length > 0)
        {
            var qualificationLink = query.AddLink(
                dfeta_ittqualification.EntityLogicalName,
                dfeta_initialteachertraining.Fields.dfeta_ITTQualificationId,
                dfeta_ittqualification.PrimaryIdAttribute,
                JoinOperator.LeftOuter);

            qualificationLink.Columns = new ColumnSet(qualificationColumnNames);

            qualificationLink.EntityAlias = "qualification";
        }

        var request = new RetrieveMultipleRequest()
        {
            Query = query
        };

        var result = await requestBuilder.AddRequest<RetrieveMultipleResponse>(request).GetResponseAsync();

        return result.EntityCollection.Entities.Select(entity => entity.ToEntity<dfeta_initialteachertraining>()).ToArray();

        static void AddSubjectLink(QueryExpression query, string subjectIdField, string alias, string[] columnNames)
        {
            var subjectLink = query.AddLink(
                dfeta_ittsubject.EntityLogicalName,
                subjectIdField,
                dfeta_ittsubject.PrimaryIdAttribute,
                JoinOperator.LeftOuter);

            subjectLink.Columns = new ColumnSet(columnNames);
            subjectLink.EntityAlias = alias;
        }
    }

    public async Task<Account[]> GetIttProviders(bool activeOnly)
    {
        var filter = new FilterExpression(LogicalOperator.And);
        if (activeOnly)
        {
            filter.AddCondition(Account.Fields.StateCode, ConditionOperator.Equal, (int)AccountState.Active);
        }

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

    public Task<dfeta_ittqualification> GetIttQualificationByCode(string code) => GetIttQualificationByCode(code, requestBuilder: null);

    public async Task<dfeta_ittqualification> GetIttQualificationByCode(string code, RequestBuilder requestBuilder)
    {
        requestBuilder ??= RequestBuilder.CreateSingle(_service);

        var query = new QueryByAttribute(dfeta_ittqualification.EntityLogicalName)
        {
            ColumnSet = new() { AllColumns = true }
        };

        query.AddAttributeValue(dfeta_ittqualification.Fields.dfeta_Value, code);
        query.AddAttributeValue(dfeta_ittqualification.Fields.StateCode, (int)dfeta_ittqualificationState.Active);

        var request = new RetrieveMultipleRequest()
        {
            Query = query
        };

        var result = await requestBuilder.AddRequest<RetrieveMultipleResponse>(request).GetResponseAsync();

        return result.EntityCollection.Entities.Select(entity => entity.ToEntity<dfeta_ittqualification>()).FirstOrDefault();
    }

    public Task<dfeta_ittsubject> GetIttSubjectByCode(string code) => GetIttSubjectByCode(code, requestBuilder: null);

    public async Task<dfeta_ittsubject> GetIttSubjectByCode(string code, RequestBuilder requestBuilder)
    {
        requestBuilder ??= RequestBuilder.CreateSingle(_service);

        var query = new QueryByAttribute(dfeta_ittsubject.EntityLogicalName)
        {
            ColumnSet = new() { AllColumns = true }
        };

        query.AddAttributeValue(dfeta_ittsubject.Fields.dfeta_Value, code);
        query.AddAttributeValue(dfeta_ittsubject.Fields.StateCode, (int)dfeta_ittsubjectState.Active);

        var request = new RetrieveMultipleRequest()
        {
            Query = query
        };

        var result = await requestBuilder.AddRequest<RetrieveMultipleResponse>(request).GetResponseAsync();

        return result.EntityCollection.Entities.Select(entity => entity.ToEntity<dfeta_ittsubject>()).FirstOrDefault();
    }

    public async Task<Contact[]> FindTeachers(FindTeachersByTrnBirthDateAndNinoQuery request)
    {
        var filter = new FilterExpression(LogicalOperator.And);

        if (string.IsNullOrEmpty(request.NationalInsuranceNumber))
        {
            filter.AddCondition(Contact.Fields.dfeta_TRN, ConditionOperator.Equal, request.Trn);
        }
        else
        {
            var childFilter = new FilterExpression(LogicalOperator.Or);

            childFilter.AddCondition(Contact.Fields.dfeta_TRN, ConditionOperator.Equal, request.Trn);
            childFilter.AddCondition(Contact.Fields.dfeta_NINumber, ConditionOperator.Equal, request.NationalInsuranceNumber);

            filter.AddFilter(childFilter);
        }

        filter.AddCondition(Contact.Fields.BirthDate, ConditionOperator.Equal, request.BirthDate);

        var query = new QueryExpression(Contact.EntityLogicalName)
        {
            ColumnSet = new ColumnSet(
                Contact.Fields.FullName,
                Contact.Fields.StateCode,
                Contact.Fields.dfeta_TRN,
                Contact.Fields.dfeta_NINumber,
                Contact.Fields.BirthDate,
                Contact.Fields.dfeta_ActiveSanctions,
                Contact.Fields.dfeta_InductionStatus
            ),
            Criteria = filter
        };

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

            var filter = new FilterExpression();
            filter.AddCondition(dfeta_initialteachertraining.Fields.StateCode, ConditionOperator.Equal, (int)dfeta_initialteachertrainingState.Active);
            initialTeacherTrainingLink.LinkCriteria = filter;

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

            subjectLink.Columns = new ColumnSet(
                dfeta_ittsubject.PrimaryIdAttribute,
                dfeta_ittsubject.Fields.dfeta_Value);

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

            var filter = new FilterExpression();
            filter.AddCondition(dfeta_induction.Fields.StateCode, ConditionOperator.Equal, (int)dfeta_inductionState.Active);
            inductionLink.LinkCriteria = filter;
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
            filter.AddCondition(dfeta_qtsregistration.Fields.StateCode, ConditionOperator.Equal, (int)dfeta_qtsregistrationState.Active);
            qualifiedTeacherStatusLink.LinkCriteria = filter;
        }
    }

    public Task<Account[]> GetIttProviderOrganizationsByName(string name, string[] columnNames, bool activeOnly) =>
        GetIttProviderOrganizationsByName(name, activeOnly, columnNames, requestBuilder: null);

    public async Task<Account[]> GetIttProviderOrganizationsByName(string name, bool activeOnly, string[] columnNames, RequestBuilder requestBuilder)
    {
        requestBuilder ??= RequestBuilder.CreateSingle(_service);

        var query = new QueryByAttribute(Account.EntityLogicalName)
        {
            ColumnSet = new(columnNames)
        };

        query.AddAttributeValue(Account.Fields.dfeta_TrainingProvider, true);
        query.AddAttributeValue(Account.Fields.Name, name);

        if (activeOnly)
        {
            query.AddAttributeValue(Account.Fields.StateCode, (int)AccountState.Active);
        }

        var request = new RetrieveMultipleRequest()
        {
            Query = query
        };

        var result = await requestBuilder.AddRequest<RetrieveMultipleResponse>(request).GetResponseAsync();

        return result.EntityCollection.Entities.Select(entity => entity.ToEntity<Account>()).ToArray();
    }

    public Task<Account[]> GetIttProviderOrganizationsByUkprn(string ukprn, string[] columnNames, bool activeOnly) =>
        GetIttProviderOrganizationsByUkprn(ukprn, activeOnly, columnNames, requestBuilder: null);

    public async Task<Account[]> GetIttProviderOrganizationsByUkprn(string ukprn, bool activeOnly, string[] columnNames, RequestBuilder requestBuilder)
    {
        requestBuilder ??= RequestBuilder.CreateSingle(_service);

        var query = new QueryByAttribute(Account.EntityLogicalName)
        {
            ColumnSet = new(columnNames)
        };

        query.AddAttributeValue(Account.Fields.dfeta_TrainingProvider, true);
        query.AddAttributeValue(Account.Fields.dfeta_UKPRN, ukprn);

        if (activeOnly)
        {
            query.AddAttributeValue(Account.Fields.StateCode, (int)AccountState.Active);
        }

        var request = new RetrieveMultipleRequest()
        {
            Query = query
        };

        var result = await requestBuilder.AddRequest<RetrieveMultipleResponse>(request).GetResponseAsync();

        return result.EntityCollection.Entities.Select(entity => entity.ToEntity<Account>()).ToArray();
    }

    public Task<Account[]> GetOrganizationsByUkprn(string ukprn, string[] columnNames) =>
        GetOrganizationsByUkprn(ukprn, columnNames, requestBuilder: null);

    public async Task<Account[]> GetOrganizationsByUkprn(string ukprn, string[] columnNames, RequestBuilder requestBuilder)
    {
        requestBuilder ??= RequestBuilder.CreateSingle(_service);

        var query = new QueryByAttribute(Account.EntityLogicalName)
        {
            ColumnSet = new(columnNames)
        };

        query.AddAttributeValue(Account.Fields.dfeta_UKPRN, ukprn);
        query.AddAttributeValue(Account.Fields.StateCode, (int)AccountState.Active);

        var request = new RetrieveMultipleRequest()
        {
            Query = query
        };

        var result = await requestBuilder.AddRequest<RetrieveMultipleResponse>(request).GetResponseAsync();

        return result.EntityCollection.Entities.Select(entity => entity.ToEntity<Account>()).ToArray();
    }

    public Task<dfeta_qtsregistration[]> GetQtsRegistrationsByTeacher(
        Guid teacherId,
        string[] columnNames) =>
        GetQtsRegistrationsByTeacher(teacherId, columnNames, requestBuilder: null);

    public async Task<dfeta_qtsregistration[]> GetQtsRegistrationsByTeacher(
        Guid teacherId,
        string[] columnNames,
        RequestBuilder requestBuilder)
    {
        requestBuilder ??= RequestBuilder.CreateSingle(_service);

        var query = new QueryByAttribute(dfeta_qtsregistration.EntityLogicalName)
        {
            ColumnSet = new(columnNames)
        };

        query.AddAttributeValue(dfeta_qtsregistration.Fields.dfeta_PersonId, teacherId);
        query.AddAttributeValue(dfeta_qtsregistration.Fields.StateCode, (int)dfeta_qtsregistrationState.Active);

        var request = new RetrieveMultipleRequest()
        {
            Query = query
        };

        var result = await requestBuilder.AddRequest<RetrieveMultipleResponse>(request).GetResponseAsync();

        return result.EntityCollection.Entities.Select(entity => entity.ToEntity<dfeta_qtsregistration>()).ToArray();
    }

    public async Task<dfeta_qualification[]> GetQualificationsForTeacher(
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

        var result = await _service.RetrieveMultipleAsync(query);

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

    public async Task<dfeta_qualification> GetQualificationById(
        Guid qualificationId,
        string[] columnNames,
        string[] contactColumnNames = null)
    {
        var filter = new FilterExpression();
        filter.AddCondition(dfeta_qualification.PrimaryIdAttribute, ConditionOperator.Equal, qualificationId);
        filter.AddCondition(dfeta_qualification.Fields.StateCode, ConditionOperator.Equal, (int)dfeta_qualificationState.Active);

        var query = new QueryExpression(dfeta_qualification.EntityLogicalName)
        {
            ColumnSet = new ColumnSet(columnNames),
            Criteria = filter
        };

        if (contactColumnNames?.Length > 0)
        {
            AddContactLink(query, contactColumnNames);
        }

        var result = await _service.RetrieveMultipleAsync(query);

        return result.Entities.Select(entity => entity.ToEntity<dfeta_qualification>()).FirstOrDefault();

        static void AddContactLink(QueryExpression query, string[] columnNames)
        {
            var contactLink = query.AddLink(
                Contact.EntityLogicalName,
                dfeta_qualification.Fields.dfeta_PersonId,
                Contact.PrimaryIdAttribute,
                JoinOperator.Inner);

            contactLink.Columns = new ColumnSet(columnNames);
            contactLink.EntityAlias = Contact.EntityLogicalName;
        }
    }

    public async Task<(dfeta_induction, dfeta_inductionperiod[])> GetInductionByTeacher(
        Guid teacherId,
        string[] columnNames,
        string[] inductionPeriodColumnNames = null,
        string[] appropriateBodyColumnNames = null,
        string[] contactColumnNames = null)
    {
        var filter = new FilterExpression();
        filter.AddCondition(dfeta_induction.Fields.dfeta_PersonId, ConditionOperator.Equal, teacherId);
        filter.AddCondition(dfeta_induction.Fields.StateCode, ConditionOperator.Equal, (int)dfeta_inductionState.Active);

        var query = new QueryExpression(dfeta_induction.EntityLogicalName)
        {
            ColumnSet = new ColumnSet(columnNames),
            Criteria = filter
        };

        if (inductionPeriodColumnNames?.Length > 0)
        {
            var inductionPeriodLink = query.AddLink(
                dfeta_inductionperiod.EntityLogicalName,
                dfeta_induction.PrimaryIdAttribute,
                dfeta_inductionperiod.Fields.dfeta_InductionId,
                JoinOperator.LeftOuter);

            inductionPeriodLink.Columns = new ColumnSet(inductionPeriodColumnNames);
            inductionPeriodLink.EntityAlias = dfeta_inductionperiod.EntityLogicalName;

            if (appropriateBodyColumnNames?.Length > 0)
            {
                var appropriateBodyLink = inductionPeriodLink.AddLink(
                Account.EntityLogicalName,
                dfeta_inductionperiod.Fields.dfeta_AppropriateBodyId,
                Account.PrimaryIdAttribute,
                JoinOperator.LeftOuter);

                appropriateBodyLink.Columns = new ColumnSet(appropriateBodyColumnNames);
                appropriateBodyLink.EntityAlias = $"{dfeta_inductionperiod.EntityLogicalName}.appropriatebody";
            }
        }

        if (contactColumnNames?.Length > 0)
        {
            var contactLink = query.AddLink(
                Contact.EntityLogicalName,
                dfeta_induction.Fields.dfeta_PersonId,
                Contact.PrimaryIdAttribute,
                JoinOperator.Inner);

            contactLink.Columns = new ColumnSet(contactColumnNames);
            contactLink.EntityAlias = Contact.EntityLogicalName;
        }

        var result = await _service.RetrieveMultipleAsync(query);

        var inductionAndPeriods = result.Entities.Select(entity => entity.ToEntity<dfeta_induction>())
            .Select(i => (Induction: i, InductionPeriod: i.Extract<dfeta_inductionperiod>(dfeta_inductionperiod.EntityLogicalName, dfeta_induction.PrimaryIdAttribute)));

        var returnValue = inductionAndPeriods
            .GroupBy(t => t.Induction.Id)
            .Select(g => (g.First().Induction, g.Where(i => i.InductionPeriod != null).Select(i => i.InductionPeriod).OrderBy(p => p.dfeta_StartDate).ToArray()))
            .OrderBy(i => i.Induction.CreatedOn ?? DateTime.MinValue)
            .FirstOrDefault();

        return returnValue;
    }

    public async Task<Contact> GetTeacher(Guid teacherId, string[] columnNames, bool resolveMerges = true)
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
            return await GetTeacher(masterReference.Id, columnNames, resolveMerges);
        }

        return teacher;
    }

    public async Task<Contact> GetTeacherByTrn(string trn, string[] columnNames, bool activeOnly = true)
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

        return result.Entities.Select(e => e.ToEntity<Contact>()).SingleOrDefault();
    }

    public async Task<Contact[]> GetTeachersByTrnAndDoB(string trn, DateOnly birthDate, string[] columnNames, bool activeOnly = true)
    {
        var filter = new FilterExpression(LogicalOperator.And);
        filter.AddCondition(Contact.Fields.dfeta_TRN, ConditionOperator.Equal, trn);
        filter.AddCondition(Contact.Fields.BirthDate, ConditionOperator.Equal, birthDate.ToDateTime());
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

    public async Task<Contact[]> GetTeachersBySlugIdAndTrn(string slugId, string trn, string[] columnNames, bool activeOnly = true)
    {
        var filter = new FilterExpression(LogicalOperator.And);
        filter.AddCondition(Contact.Fields.dfeta_SlugId, ConditionOperator.Equal, slugId);
        if (activeOnly)
        {
            filter.AddCondition(Contact.Fields.StateCode, ConditionOperator.Equal, (int)ContactState.Active);
            filter.AddCondition(Contact.Fields.dfeta_TRN, ConditionOperator.Equal, trn);
        }

        var query = new QueryExpression(Contact.EntityLogicalName)
        {
            ColumnSet = new(columnNames),
            Criteria = filter
        };

        var result = await _service.RetrieveMultipleAsync(query);

        return result.Entities.Select(e => e.ToEntity<Contact>()).ToArray();
    }

    public Task<Models.Task[]> GetCrmTasksForTeacher(Guid teacherId, string[] columnNames) =>
        GetCrmTasksForTeacher(teacherId, columnNames, requestBuilder: null);

    public async Task<Models.Task[]> GetCrmTasksForTeacher(Guid teacherId, string[] columnNames, RequestBuilder requestBuilder)
    {
        requestBuilder ??= RequestBuilder.CreateSingle(_service);

        var query = new QueryByAttribute(Models.Task.EntityLogicalName)
        {
            ColumnSet = new ColumnSet(columnNames)
        };
        query.AddAttributeValue(Models.Task.Fields.RegardingObjectId, teacherId);

        var request = new RetrieveMultipleRequest()
        {
            Query = query
        };

        var result = await requestBuilder.AddRequest<RetrieveMultipleResponse>(request).GetResponseAsync();

        return result.EntityCollection.Entities.Select(entity => entity.ToEntity<Models.Task>()).ToArray();
    }

    public async Task<Contact[]> GetTeachersByHusId(string husId, string[] columnNames)
    {
        var filter = new FilterExpression(LogicalOperator.And);
        filter.AddCondition(Contact.Fields.dfeta_HUSID, ConditionOperator.Equal, husId);
        filter.AddCondition(Contact.Fields.StateCode, ConditionOperator.Equal, (int)ContactState.Active);

        var query = new QueryExpression(Contact.EntityLogicalName)
        {
            ColumnSet = new(columnNames),
            Criteria = filter
        };

        var result = await _service.RetrieveMultipleAsync(query);

        return result.Entities.Select(e => e.ToEntity<Contact>()).ToArray();
    }

    public async Task<Contact[]> GetTeachersBySlugId(string slugId, string[] columnNames)
    {
        var filter = new FilterExpression(LogicalOperator.And);
        filter.AddCondition(Contact.Fields.dfeta_SlugId, ConditionOperator.Equal, slugId);
        filter.AddCondition(Contact.Fields.StateCode, ConditionOperator.Equal, (int)ContactState.Active);

        var query = new QueryExpression(Contact.EntityLogicalName)
        {
            ColumnSet = new(columnNames),
            Criteria = filter
        };

        var result = await _service.RetrieveMultipleAsync(query);

        return result.Entities.Select(e => e.ToEntity<Contact>()).ToArray();
    }

    public async Task<dfeta_teacherstatus> GetTeacherStatus(string value, RequestBuilder requestBuilder)
    {
        var result = await GetAllTeacherStatuses(requestBuilder);

        return result.SingleOrDefault(x => x.dfeta_Value == value);
    }

    public Task<List<dfeta_teacherstatus>> GetAllTeacherStatuses() => GetAllTeacherStatuses(requestBuilder: null);

    public async Task<List<dfeta_teacherstatus>> GetAllTeacherStatuses(
        RequestBuilder requestBuilder)
    {
        return await _cache.GetOrCreate(CacheKeys.GetAllTeacherStatuses(), async _ =>
        {
            requestBuilder ??= RequestBuilder.CreateSingle(_service);

            var query = new QueryByAttribute(dfeta_teacherstatus.EntityLogicalName)
            {
                ColumnSet = new() { AllColumns = true }
            };

            query.AddAttributeValue(dfeta_teacherstatus.Fields.StateCode, (int)dfeta_teacherStatusState.Active);

            var request = new RetrieveMultipleRequest()
            {
                Query = query
            };

            var result = await requestBuilder.AddRequest<RetrieveMultipleResponse>(request).GetResponseAsync();

            return result.EntityCollection.Entities.Select(entity => entity.ToEntity<dfeta_teacherstatus>()).ToList();
        });
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
        catch (FaultException<OrganizationServiceFault> fault)
            when (fault.Message.Contains(" does not exist", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }
    }

    public Task<Account[]> GetOrganizationsByName(string name, string[] columnNames, bool activeOnly) =>
        GetOrganizationsByName(name, activeOnly, columnNames, requestBuilder: null);

    public async Task<Account[]> GetOrganizationsByName(string name, bool activeOnly, string[] columnNames, RequestBuilder requestBuilder)
    {
        requestBuilder ??= RequestBuilder.CreateSingle(_service);

        var query = new QueryByAttribute(Account.EntityLogicalName)
        {
            ColumnSet = new ColumnSet(columnNames)
        };

        query.AddAttributeValue(Account.Fields.Name, name);

        if (activeOnly)
        {
            query.AddAttributeValue(Account.Fields.StateCode, (int)AccountState.Active);
        }

        var request = new RetrieveMultipleRequest()
        {
            Query = query
        };

        var result = await requestBuilder.AddRequest<RetrieveMultipleResponse>(request).GetResponseAsync();

        return result.EntityCollection.Entities.Select(entity => entity.ToEntity<Account>()).ToArray();
    }

    public async Task<Contact[]> FindTeachers(FindTeachersQuery findTeachersQuery)
    {
        var conditions = new List<object>();  //  Either ConditionExpresssion or FilterExpression

        void AddEqualCondition(string fieldName, object value)
        {
            if (value is null || (value is string str && string.IsNullOrEmpty(str)))
            {
                return;
            }

            conditions.Add(new ConditionExpression(fieldName, ConditionOperator.Equal, value));
        }

        AddEqualCondition(Contact.Fields.BirthDate, findTeachersQuery.DateOfBirth?.ToDateTime());
        AddEqualCondition(Contact.Fields.dfeta_NINumber, findTeachersQuery.NationalInsuranceNumber);
        AddEqualCondition(Contact.Fields.EMailAddress1, findTeachersQuery.EmailAddress);
        AddEqualCondition(Contact.Fields.dfeta_TRN, findTeachersQuery.Trn);

        {
            // Find all the permutations of names to match on
            var firstNames = new[] { findTeachersQuery.FirstName, findTeachersQuery.PreviousFirstName };
            var lastNames = new[] { findTeachersQuery.LastName, findTeachersQuery.PreviousLastName };

            var firstNamesFilter = new FilterExpression(LogicalOperator.Or);
            foreach (var firstName in firstNames)
            {
                if (!string.IsNullOrEmpty(firstName))
                {
                    firstNamesFilter.AddCondition(Contact.Fields.FirstName, ConditionOperator.Equal, firstName);
                }
            }

            var lastNamesFilter = new FilterExpression(LogicalOperator.Or);
            foreach (var lastName in lastNames)
            {
                if (!string.IsNullOrEmpty(lastName))
                {
                    lastNamesFilter.AddCondition(Contact.Fields.LastName, ConditionOperator.Equal, lastName);
                }
            }

            var nameFilter = new FilterExpression(LogicalOperator.And);

            if (firstNamesFilter.Conditions.Count > 0)
            {
                nameFilter.AddFilter(firstNamesFilter);
            }

            if (lastNamesFilter.Conditions.Count > 0)
            {
                nameFilter.AddFilter(lastNamesFilter);
            }

            if (nameFilter.Filters.Count > 0)
            {
                conditions.Add(nameFilter);
            }
        }

        LinkEntity ittProviderLink = null;
        var ittProviderOrganizationIdsArray = findTeachersQuery.IttProviderOrganizationIds?.ToArray() ?? Array.Empty<Guid>();

        if (ittProviderOrganizationIdsArray.Length > 0)
        {
            ittProviderLink = new LinkEntity(
                Contact.EntityLogicalName,
                dfeta_initialteachertraining.EntityLogicalName,
                Contact.PrimaryIdAttribute,
                dfeta_initialteachertraining.Fields.dfeta_PersonId,
                JoinOperator.LeftOuter)
            {
                Columns = new ColumnSet(dfeta_initialteachertraining.Fields.dfeta_EstablishmentId)
            };

            conditions.Add(
                new ConditionExpression(
                    dfeta_initialteachertraining.EntityLogicalName,
                    dfeta_initialteachertraining.Fields.dfeta_EstablishmentId,
                    ConditionOperator.In,
                    ittProviderOrganizationIdsArray.Cast<object>().ToArray()));  // https://community.dynamics.com/crm/b/crmbusiness/posts/crm-2011-odd-error-with-query-expression-and-conditionoperator-in
        }

        // If we still don't have at least 3 identifiers to match on then we're done
        var identifierCount = conditions.Count;
        if (identifierCount < 3)
        {
            return Array.Empty<Contact>();
        }

        // Get all permutations of 3 matching conditions
        var filter = new FilterExpression(LogicalOperator.Or);
        foreach (var permutationConditions in conditions.GetCombinations(length: 3))
        {
            var innerFilter = new FilterExpression(LogicalOperator.And);

            foreach (var condition in permutationConditions)
            {
                if (condition is FilterExpression filterCondition)
                {
                    innerFilter.AddFilter(filterCondition);
                }
                else
                {
                    Debug.Assert(condition is ConditionExpression);
                    innerFilter.AddCondition((ConditionExpression)condition);
                }
            }

            filter.AddFilter(innerFilter);
        }

        var query = new QueryExpression(Contact.EntityLogicalName)
        {
            ColumnSet = new ColumnSet(
                Contact.Fields.dfeta_TRN,
                Contact.Fields.EMailAddress1,
                Contact.Fields.FirstName,
                Contact.Fields.MiddleName,
                Contact.Fields.LastName,
                Contact.Fields.BirthDate,
                Contact.Fields.dfeta_NINumber,
                Contact.Fields.dfeta_ActiveSanctions
            ),
            Criteria = new FilterExpression(LogicalOperator.And)
            {
                Conditions =
                {
                    new ConditionExpression(Contact.Fields.StateCode, ConditionOperator.Equal, (int)ContactState.Active),
                    new ConditionExpression(Contact.Fields.dfeta_TRN, ConditionOperator.NotNull)
                },
                Filters =
                {
                    filter
                }
            },
            Orders =
            {
                new OrderExpression(Contact.Fields.LastName, OrderType.Ascending),
                new OrderExpression(Contact.Fields.FirstName, OrderType.Ascending)
            }
        };

        if (ittProviderLink != null)
        {
            query.LinkEntities.Add(ittProviderLink);
        }

        var result = await _service.RetrieveMultipleAsync(query);

        var contacts = result.Entities.Select(entity => entity.ToEntity<Contact>());

        // De-dup records (we might get multiple results for the same person because of the ITT provider join)
        return contacts.GroupBy(c => c.Id).Select(c => c.First()).ToArray();
    }

    public async Task<Contact[]> FindTeachersStrict(FindTeachersQuery findTeachersQuery)
    {
        // Match on DOB, NINO & TRN *OR*
        // DOB, TRN & Name & contact.NINO is null

        if (string.IsNullOrEmpty(findTeachersQuery.Trn) || findTeachersQuery.DateOfBirth is null)
        {
            return Array.Empty<Contact>();
        }

        var filter = new FilterExpression(LogicalOperator.Or);

        if (!string.IsNullOrEmpty(findTeachersQuery.NationalInsuranceNumber))
        {
            var dobNinoTrnCondition = new FilterExpression(LogicalOperator.And);
            dobNinoTrnCondition.AddCondition(Contact.Fields.BirthDate, ConditionOperator.Equal, findTeachersQuery.DateOfBirth!.Value.ToDateTime());
            dobNinoTrnCondition.AddCondition(Contact.Fields.dfeta_NINumber, ConditionOperator.Equal, findTeachersQuery.NationalInsuranceNumber);
            dobNinoTrnCondition.AddCondition(Contact.Fields.dfeta_TRN, ConditionOperator.Equal, findTeachersQuery.Trn);

            filter.AddFilter(dobNinoTrnCondition);
        }

        var nameFilter = GetNameFilter();
        if (nameFilter is not null)
        {
            var dobTrnNameAndNoNinoCondition = new FilterExpression(LogicalOperator.And);
            dobTrnNameAndNoNinoCondition.AddCondition(Contact.Fields.BirthDate, ConditionOperator.Equal, findTeachersQuery.DateOfBirth!.Value.ToDateTime());
            dobTrnNameAndNoNinoCondition.AddCondition(Contact.Fields.dfeta_TRN, ConditionOperator.Equal, findTeachersQuery.Trn);
            dobTrnNameAndNoNinoCondition.AddFilter(nameFilter);

            var missingNinoCondition = new FilterExpression(LogicalOperator.Or);
            missingNinoCondition.AddCondition(Contact.Fields.dfeta_NINumber, ConditionOperator.Equal, "");
            missingNinoCondition.AddCondition(Contact.Fields.dfeta_NINumber, ConditionOperator.Null);

            dobTrnNameAndNoNinoCondition.AddFilter(missingNinoCondition);

            filter.AddFilter(dobTrnNameAndNoNinoCondition);
        }

        if (filter.Filters.Count == 0)
        {
            return Array.Empty<Contact>();
        }

        var query = new QueryExpression(Contact.EntityLogicalName)
        {
            ColumnSet = new ColumnSet(
                Contact.Fields.dfeta_TRN,
                Contact.Fields.EMailAddress1,
                Contact.Fields.FirstName,
                Contact.Fields.MiddleName,
                Contact.Fields.LastName,
                Contact.Fields.BirthDate,
                Contact.Fields.dfeta_NINumber,
                Contact.Fields.dfeta_ActiveSanctions
            ),
            Criteria = new FilterExpression(LogicalOperator.And)
            {
                Conditions =
                {
                    new ConditionExpression(Contact.Fields.StateCode, ConditionOperator.Equal, (int)ContactState.Active),
                    new ConditionExpression(Contact.Fields.dfeta_TRN, ConditionOperator.NotNull)
                },
                Filters =
                {
                    filter
                }
            },
            Orders =
            {
                new OrderExpression(Contact.Fields.LastName, OrderType.Ascending),
                new OrderExpression(Contact.Fields.FirstName, OrderType.Ascending)
            }
        };

        var result = await _service.RetrieveMultipleAsync(query);

        var contacts = result.Entities.Select(entity => entity.ToEntity<Contact>());

        return contacts.ToArray();

        FilterExpression GetNameFilter()
        {
            // Find all the permutations of names to match on
            var firstNames = new[] { findTeachersQuery.FirstName, findTeachersQuery.PreviousFirstName };
            var lastNames = new[] { findTeachersQuery.LastName, findTeachersQuery.PreviousLastName };

            var firstNamesFilter = new FilterExpression(LogicalOperator.Or);
            foreach (var firstName in firstNames)
            {
                if (!string.IsNullOrEmpty(firstName))
                {
                    firstNamesFilter.AddCondition(Contact.Fields.FirstName, ConditionOperator.Equal, firstName);
                }
            }

            var lastNamesFilter = new FilterExpression(LogicalOperator.Or);
            foreach (var lastName in lastNames)
            {
                if (!string.IsNullOrEmpty(lastName))
                {
                    lastNamesFilter.AddCondition(Contact.Fields.LastName, ConditionOperator.Equal, lastName);
                }
            }

            var nameFilter = new FilterExpression(LogicalOperator.And);

            if (firstNamesFilter.Conditions.Count > 0)
            {
                nameFilter.AddFilter(firstNamesFilter);
            }

            if (lastNamesFilter.Conditions.Count > 0)
            {
                nameFilter.AddFilter(lastNamesFilter);
            }

            return nameFilter.Filters.Count > 0 ? nameFilter : null;
        }
    }

    public async Task<Contact> GetTeacherByTsPersonId(string tsPersonId, string[] columnNames)
    {
        var filter = new FilterExpression(LogicalOperator.And);
        filter.AddCondition(Contact.Fields.dfeta_TSPersonID, ConditionOperator.Equal, tsPersonId);
        filter.AddCondition(Contact.Fields.StateCode, ConditionOperator.Equal, (int)ContactState.Active);

        var query = new QueryExpression(Contact.EntityLogicalName)
        {
            ColumnSet = new(columnNames),
            Criteria = filter
        };

        var result = await _service.RetrieveMultipleAsync(query);

        return result.Entities.Select(e => e.ToEntity<Contact>()).SingleOrDefault();
    }

    public Task SetTsPersonId(Guid teacherId, string tsPersonId)
    {
        return _service.UpdateAsync(new Contact()
        {
            Id = teacherId,
            dfeta_TSPersonID = tsPersonId
        });
    }

    public Task UpdateTeacherIdentityInfo(UpdateTeacherIdentityInfoCommand command)
    {
        return _service.UpdateAsync(new Contact()
        {
            Id = command.TeacherId,
            dfeta_TSPersonID = command.IdentityUserId.ToString(),
            EMailAddress1 = command.EmailAddress,
            MobilePhone = command.MobilePhone,
            dfeta_LastIdentityUpdate = command.UpdateTimeUtc
        });
    }

    public Task<Subject> GetSubjectByTitle(string title) => GetSubjectByTitle(title, requestBuilder: null);

    public async Task<Subject> GetSubjectByTitle(string title, RequestBuilder requestBuilder)
    {
        return await _cache.GetOrCreateAsync(CacheKeys.GetSubjectTitleKey(title), async _ =>
        {
            requestBuilder ??= RequestBuilder.CreateSingle(_service);

            var filter = new FilterExpression(LogicalOperator.And);
            filter.AddCondition(Subject.Fields.Title, ConditionOperator.Equal, title);

            var query = new QueryExpression(Subject.EntityLogicalName)
            {
                ColumnSet = new(allColumns: true),
                Criteria = filter
            };

            var request = new RetrieveMultipleRequest()
            {
                Query = query
            };

            var result = await requestBuilder.AddRequest<RetrieveMultipleResponse>(request).GetResponseAsync();
            return result.EntityCollection.Entities.Select(e => e.ToEntity<Subject>()).SingleOrDefault();
        });
    }

    public async Task<Incident[]> GetIncidentsByContactId(Guid contactId, IncidentState? state, string[] columnNames)
    {
        var filter = new FilterExpression(LogicalOperator.And);
        filter.AddCondition(Incident.Fields.CustomerId, ConditionOperator.Equal, contactId);

        if (state is not null)
        {
            filter.AddCondition(Incident.Fields.StateCode, ConditionOperator.Equal, (int)state);
        }

        var query = new QueryExpression(Incident.EntityLogicalName)
        {
            ColumnSet = new(columnNames),
            Criteria = filter
        };

        var result = await _service.RetrieveMultipleAsync(query);

        return result.Entities.Select(e => e.ToEntity<Incident>()).ToArray();
    }

    public async Task ClearTeacherIdentityInfo(Guid identityUserId, DateTime updateTimeUtc)
    {
        var query = new QueryByAttribute(Contact.EntityLogicalName)
        {
            ColumnSet = new()
        };

        query.AddAttributeValue(Contact.Fields.dfeta_TSPersonID, identityUserId.ToString());

        var request = new RetrieveMultipleRequest()
        {
            Query = query
        };

        var result = (RetrieveMultipleResponse)await _service.ExecuteAsync(request);

        if (result.EntityCollection.Entities.Count == 1)
        {
            await _service.UpdateAsync(new Contact()
            {
                Id = result.EntityCollection.Entities[0].Id,
                dfeta_TSPersonID = null,
                dfeta_LastIdentityUpdate = updateTimeUtc
            });
        }
    }

    public RequestBuilder CreateMultipleRequestBuilder() => RequestBuilder.CreateMultiple(_service);

    public RequestBuilder CreateSingleRequestBuilder() => RequestBuilder.CreateSingle(_service);

    public RequestBuilder CreateTransactionRequestBuilder() => RequestBuilder.CreateTransaction(_service);

    public async Task<bool> DoesTeacherHavePendingPIIChanges(Guid teacherId)
    {
        bool pendingNameChange = default, pendingDateOfBirthChange = default;
        var nameChangeSubject = await GetSubjectByTitle("Change of Name");
        var dateOfBirthChangeSubject = await GetSubjectByTitle("Change of Date of Birth");
        var incidents = await GetIncidentsByContactId(
            teacherId,
            IncidentState.Active,
            columnNames: new[] { Incident.Fields.SubjectId, Incident.Fields.StateCode });

        pendingNameChange = incidents.Any(i => i.SubjectId.Id == nameChangeSubject.Id);
        pendingDateOfBirthChange = incidents.Any(i => i.SubjectId.Id == dateOfBirthChangeSubject.Id);

        return (pendingNameChange || pendingDateOfBirthChange);
    }
}
