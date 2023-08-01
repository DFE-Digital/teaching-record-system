#nullable disable
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;

namespace TeachingRecordSystem.Core.Dqt.Tests;

public partial class TestDataHelper
{
    public async Task<CreateQualificationResult> CreateQualification(
        Guid teacherId, dfeta_qualification_dfeta_Type qualificationType, bool? createdByApi = false)
    {
        var ittProviderUkprn = "10044534";

        var lookupRequestBuilder = _dataverseAdapter.CreateMultipleRequestBuilder();

        var getIttProviderTask = _globalCache.GetOrCreateAsync(
            CacheKeys.GetIttProviderOrganizationByUkprnKey(ittProviderUkprn),
            _ => _dataverseAdapter.GetOrganizationsByUkprn(ittProviderUkprn, columnNames: Array.Empty<string>(), lookupRequestBuilder)
                .ContinueWith(t => t.Result.SingleOrDefault()));

        var country = "XK";
        var getCountryCodeTask = _globalCache.GetOrCreateAsync(
            CacheKeys.GetCountryKey(country),
            _ => _dataverseAdapter.GetCountry(country, lookupRequestBuilder));

        var heSubjectCode = "100366";  // computer science
        var getHeSubjectTask = _globalCache.GetOrCreateAsync(
            CacheKeys.GetHeSubjectKey(heSubjectCode),
            _ => _dataverseAdapter.GetHeSubjectByCode(heSubjectCode, lookupRequestBuilder));

        var heSubject2Code = "B780";  // Paramedical Nursing
        var getHeSubject2Task = _globalCache.GetOrCreateAsync(
            CacheKeys.GetHeSubjectKey(heSubject2Code),
            _ => _dataverseAdapter.GetHeSubjectByCode(heSubjectCode, lookupRequestBuilder));

        var heSubject3Code = "101076";  // Laser Physics
        var getHeSubject3Task = _globalCache.GetOrCreateAsync(
            CacheKeys.GetHeSubjectKey(heSubject3Code),
            _ => _dataverseAdapter.GetHeSubjectByCode(heSubjectCode, lookupRequestBuilder));

        var qualificationCode = "400";  // First Degree
        var getQualificationTask = _globalCache.GetOrCreateAsync(
            CacheKeys.GetHeQualificationKey(qualificationCode),
            _ => _dataverseAdapter.GetHeQualificationByCode(qualificationCode, lookupRequestBuilder));

        await lookupRequestBuilder.Execute();

        var ittProvider = getIttProviderTask.Result;
        var countryId = getCountryCodeTask.Result.Id;
        var heSubjectId = getHeSubjectTask.Result.Id;
        var heSubject2Id = getHeSubject2Task.Result.Id;
        var heSubject3Id = getHeSubject3Task.Result.Id;
        var qualificationId = getQualificationTask.Result.Id;

        var txnRequestBuilder = _dataverseAdapter.CreateTransactionRequestBuilder();


        var createqual = txnRequestBuilder.AddRequest<CreateResponse>(new CreateRequest()
        {
            Target = new dfeta_qualification()
            {
                dfeta_PersonId = new EntityReference(Contact.EntityLogicalName, teacherId),
                dfeta_HE_CountryId = new EntityReference(dfeta_qualification.EntityLogicalName, countryId),
                dfeta_HE_EstablishmentId = new EntityReference(Account.EntityLogicalName, ittProvider.Id),
                dfeta_Type = qualificationType,
                dfeta_HE_ClassDivision = dfeta_classdivision.Pass,
                dfeta_CompletionorAwardDate = DateTime.Now.AddMonths(-1),
                dfeta_HE_HESubject1Id = new EntityReference(dfeta_hesubject.EntityLogicalName, heSubjectId),
                dfeta_HE_HESubject2Id = new EntityReference(dfeta_hesubject.EntityLogicalName, heSubject2Id),
                dfeta_HE_HESubject3Id = new EntityReference(dfeta_hesubject.EntityLogicalName, heSubject3Id),
                dfeta_HE_HEQualificationId = new EntityReference(dfeta_hequalification.EntityLogicalName, qualificationId),
                dfeta_createdbyapi = createdByApi
            }
        });
        await txnRequestBuilder.Execute();
        var qualid = createqual.GetResponse().id;

        return new CreateQualificationResult(teacherId, qualid, qualificationType);

    }
}

public record CreateQualificationResult(
Guid TeacherId,
Guid QualificationId,
dfeta_qualification_dfeta_Type QualificationType);
