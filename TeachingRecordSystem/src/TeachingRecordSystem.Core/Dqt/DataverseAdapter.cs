#nullable disable
using System.Diagnostics;
using System.ServiceModel;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Services.TrnGeneration;

namespace TeachingRecordSystem.Core.Dqt;

public partial class DataverseAdapter : IDataverseAdapter
{
    private readonly IOrganizationServiceAsync _service;
    private readonly IClock _clock;
    private readonly IMemoryCache _cache;
    private readonly ITrnGenerator _trnGenerationApiClient;
    private readonly TrsDbContext _dbContext;

    public DataverseAdapter(
        IOrganizationServiceAsync organizationServiceAsync,
        IClock clock,
        IMemoryCache cache,
        ITrnGenerator trnGenerationApiClient,
        TrsDbContext dbContext)
    {
        _service = organizationServiceAsync;
        _clock = clock;
        _cache = cache;
        _trnGenerationApiClient = trnGenerationApiClient;
        _dbContext = dbContext;
    }

    public Task<string> GenerateTrnAsync() => _trnGenerationApiClient.GenerateTrnAsync();

    public Task<dfeta_country> GetCountryAsync(string value) => GetCountryAsync(value, requestBuilder: null);

    public async Task<dfeta_country> GetCountryAsync(string value, RequestBuilder requestBuilder)
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

    public Task<dfeta_earlyyearsstatus> GetEarlyYearsStatusAsync(Guid earlyYearsStatusId) => GetEarlyYearsStatusAsync(earlyYearsStatusId, requestBuilder: null);

    public async Task<dfeta_earlyyearsstatus> GetEarlyYearsStatusAsync(Guid earlyYearsStatusId, RequestBuilder requestBuilder)
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

    public async Task<dfeta_earlyyearsstatus> GetEarlyYearsStatusAsync(string value, RequestBuilder requestBuilder)
    {
        var all = await GetAllEarlyYearsStatusesAsync(requestBuilder);
        return all.SingleOrDefault(x => x.dfeta_Value == value);
    }

    public async Task<List<dfeta_earlyyearsstatus>> GetAllEarlyYearsStatusesAsync(RequestBuilder requestBuilder)
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

    public Task<dfeta_hequalification> GetHeQualificationByCodeAsync(string value) => GetHeQualificationByCodeAsync(value, requestBuilder: null);

    public async Task<dfeta_hequalification> GetHeQualificationByCodeAsync(string value, RequestBuilder requestBuilder)
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

    public Task<dfeta_hesubject> GetHeSubjectByCodeAsync(string value) => GetHeSubjectByCodeAsync(value, requestBuilder: null);

    public async Task<dfeta_hesubject> GetHeSubjectByCodeAsync(string value, RequestBuilder requestBuilder)
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

    public Task<dfeta_initialteachertraining[]> GetInitialTeacherTrainingByTeacherAsync(
        Guid teacherId,
        string[] columnNames,
        string[] establishmentColumnNames = null,
        string[] subjectColumnNames = null,
        string[] qualificationColumnNames = null,
        bool activeOnly = true) =>
        GetInitialTeacherTrainingByTeacherAsync(teacherId, columnNames, establishmentColumnNames, subjectColumnNames, qualificationColumnNames, requestBuilder: null, activeOnly);

    public async Task<dfeta_initialteachertraining[]> GetInitialTeacherTrainingBySlugIdAsync(string slugId, string[] columnNames, RequestBuilder requestBuilder, bool activeOnly = true)
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

    public async Task<Contact[]> GetTeachersByInitialTeacherTrainingSlugIdAsync(string slugId, string[] columnNames, RequestBuilder requestBuilder, bool activeOnly = true)
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

    public async Task<dfeta_initialteachertraining[]> GetInitialTeacherTrainingByTeacherAsync(
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

    public async Task<Account[]> GetIttProvidersAsync(bool activeOnly)
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

    public Task<dfeta_ittqualification> GetIttQualificationByCodeAsync(string code) => GetIttQualificationByCodeAsync(code, requestBuilder: null);

    public async Task<dfeta_ittqualification> GetIttQualificationByCodeAsync(string code, RequestBuilder requestBuilder)
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

    public Task<dfeta_ittsubject> GetIttSubjectByCodeAsync(string code) => GetIttSubjectByCodeAsync(code, requestBuilder: null);

    public async Task<dfeta_ittsubject> GetIttSubjectByCodeAsync(string code, RequestBuilder requestBuilder)
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

    public Task<Account[]> GetIttProviderOrganizationsByNameAsync(string name, string[] columnNames, bool activeOnly) =>
        GetIttProviderOrganizationsByNameAsync(name, activeOnly, columnNames, requestBuilder: null);

    public async Task<Account[]> GetIttProviderOrganizationsByNameAsync(string name, bool activeOnly, string[] columnNames, RequestBuilder requestBuilder)
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

    public Task<Account[]> GetIttProviderOrganizationsByUkprnAsync(string ukprn, string[] columnNames, bool activeOnly) =>
        GetIttProviderOrganizationsByUkprnAsync(ukprn, activeOnly, columnNames, requestBuilder: null);

    public async Task<Account[]> GetIttProviderOrganizationsByUkprnAsync(string ukprn, bool activeOnly, string[] columnNames, RequestBuilder requestBuilder)
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

    public Task<Account[]> GetOrganizationsByUkprnAsync(string ukprn, string[] columnNames) =>
        GetOrganizationsByUkprnAsync(ukprn, columnNames, requestBuilder: null);

    public async Task<Account[]> GetOrganizationsByUkprnAsync(string ukprn, string[] columnNames, RequestBuilder requestBuilder)
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

    public Task<dfeta_qtsregistration[]> GetQtsRegistrationsByTeacherAsync(
        Guid teacherId,
        string[] columnNames) =>
        GetQtsRegistrationsByTeacherAsync(teacherId, columnNames, requestBuilder: null);

    public async Task<dfeta_qtsregistration[]> GetQtsRegistrationsByTeacherAsync(
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

    public async Task<dfeta_qualification> GetQualificationByIdAsync(
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

    public async Task<Contact> GetTeacherAsync(Guid teacherId, string[] columnNames, bool resolveMerges = true)
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
            return await GetTeacherAsync(masterReference.Id, columnNames, resolveMerges);
        }

        return teacher;
    }

    public async Task<Contact> GetTeacherByTrnAsync(string trn, string[] columnNames, bool activeOnly = true)
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

    public async Task<Contact[]> GetTeachersByTrnAndDoBAsync(string trn, DateOnly birthDate, string[] columnNames, bool activeOnly = true)
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

    public async Task<Contact[]> GetTeachersBySlugIdAndTrnAsync(string slugId, string trn, string[] columnNames, bool activeOnly = true)
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

    public Task<Models.Task[]> GetCrmTasksForTeacherAsync(Guid teacherId, string[] columnNames) =>
        GetCrmTasksForTeacherAsync(teacherId, columnNames, requestBuilder: null);

    public async Task<Models.Task[]> GetCrmTasksForTeacherAsync(Guid teacherId, string[] columnNames, RequestBuilder requestBuilder)
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

    public async Task<Contact[]> GetTeachersByHusIdAsync(string husId, string[] columnNames)
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

    public async Task<Contact[]> GetTeachersBySlugIdAsync(string slugId, string[] columnNames)
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

    public async Task<dfeta_teacherstatus> GetTeacherStatusAsync(string value, RequestBuilder requestBuilder)
    {
        var result = await GetAllTeacherStatusesAsync(requestBuilder);

        return result.SingleOrDefault(x => x.dfeta_Value == value);
    }

    public Task<List<dfeta_teacherstatus>> GetAllTeacherStatusesAsync() => GetAllTeacherStatusesAsync(requestBuilder: null);

    public async Task<List<dfeta_teacherstatus>> GetAllTeacherStatusesAsync(
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

    public async Task<bool> UnlockTeacherRecordAsync(Guid teacherId)
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

    public Task<Account[]> GetOrganizationsByNameAsync(string name, string[] columnNames, bool activeOnly) =>
        GetOrganizationsByNameAsync(name, activeOnly, columnNames, requestBuilder: null);

    public async Task<Account[]> GetOrganizationsByNameAsync(string name, bool activeOnly, string[] columnNames, RequestBuilder requestBuilder)
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

    public async Task<Contact[]> FindTeachersAsync(FindTeachersQuery findTeachersQuery)
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
                Contact.Fields.dfeta_NINumber
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

    public async Task<Contact[]> FindTeachersStrictAsync(FindTeachersQuery findTeachersQuery)
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
                Contact.Fields.dfeta_NINumber
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

    public async Task<Contact> GetTeacherByTsPersonIdAsync(string tsPersonId, string[] columnNames)
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

    public Task SetTsPersonIdAsync(Guid teacherId, string tsPersonId)
    {
        return _service.UpdateAsync(new Contact()
        {
            Id = teacherId,
            dfeta_TSPersonID = tsPersonId
        });
    }

    public Task<Subject> GetSubjectByTitleAsync(string title) => GetSubjectByTitleAsync(title, requestBuilder: null);

    public async Task<Subject> GetSubjectByTitleAsync(string title, RequestBuilder requestBuilder)
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

    public async Task<Incident[]> GetIncidentsByContactIdAsync(Guid contactId, IncidentState? state, string[] columnNames)
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

    public async Task ClearTeacherIdentityInfoAsync(Guid identityUserId, DateTime updateTimeUtc)
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
}
