using System;
using System.Linq;
using System.Threading.Tasks;
using DqtApi.DataStore.Crm.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;

namespace DqtApi.Tests
{
    public partial class TestDataHelper
    {
        public async Task<CreatePersonResult> CreatePerson(
            bool earlyYears = false,
            bool assessmentOnly = false,
            bool withQualification = false,
            bool withActiveSanction = false)
        {
            if (earlyYears && assessmentOnly)
            {
                throw new ArgumentException($"Cannot set both {nameof(earlyYears)} and {nameof(assessmentOnly)}.");
            }

            var teacherId = Guid.NewGuid();
            var firstName = Faker.Name.First();
            var middleName = Faker.Name.Middle();
            var lastName = Faker.Name.Last();
            var birthDate = Faker.Identification.DateOfBirth();
            var nino = Faker.Identification.UkNationalInsuranceNumber();
            var email = Faker.Internet.Email();

            var programmeType = earlyYears ? dfeta_ITTProgrammeType.EYITTAssessmentOnly :
                assessmentOnly ? dfeta_ITTProgrammeType.AssessmentOnlyRoute :
                dfeta_ITTProgrammeType.RegisteredTeacherProgramme;

            var ittProviderUkprn = "10044534";

            var lookupRequestBuilder = _dataverseAdapter.CreateMultipleRequestBuilder();

            var getIttProviderTask = _globalCache.GetOrCreateAsync(
                CacheKeys.GetIttProviderOrganizationByUkprnKey(ittProviderUkprn),
                _ => _dataverseAdapter.GetOrganizationsByUkprn(ittProviderUkprn, columnNames: Array.Empty<string>(), lookupRequestBuilder)
                    .ContinueWith(t => t.Result.SingleOrDefault()));

            var earlyYearsStatus = "220"; // 220 == 'Early Years Trainee'

            var getEarlyYearsStatusTask = earlyYears ?
                _globalCache.GetOrCreateAsync(
                    CacheKeys.GetEarlyYearsStatusKey(earlyYearsStatus),
                    _ => _dataverseAdapter.GetEarlyYearsStatus(earlyYearsStatus, lookupRequestBuilder)) :
                Task.FromResult<dfeta_earlyyearsstatus>(null);

            var teacherStatus = programmeType == dfeta_ITTProgrammeType.AssessmentOnlyRoute ?
                "212" :  // 212 == 'AOR Candidate'
                "211";   // 211 == 'Trainee Teacher:DMS'

            var getTeacherStatusTask = !earlyYears ?
                _globalCache.GetOrCreateAsync(
                    CacheKeys.GetTeacherStatusKey(teacherStatus),
                    _ => _dataverseAdapter.GetTeacherStatus(teacherStatus, qtsDateRequired: false, lookupRequestBuilder)) :
                Task.FromResult<dfeta_teacherstatus>(null);

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
            var earlyYearsStatusId = getEarlyYearsStatusTask?.Result?.Id;
            var teacherStatusId = getTeacherStatusTask?.Result?.Id;
            var countryId = getCountryCodeTask.Result.Id;
            var heSubjectId = getHeSubjectTask.Result.Id;
            var heSubject2Id = getHeSubject2Task.Result.Id;
            var heSubject3Id = getHeSubject3Task.Result.Id;
            var qualificationId = getQualificationTask.Result.Id;

            var txnRequestBuilder = _dataverseAdapter.CreateTransactionRequestBuilder();

            txnRequestBuilder.AddRequests(
                new CreateRequest()
                {
                    Target = new Contact()
                    {
                        Id = teacherId,
                        FirstName = firstName,
                        MiddleName = middleName,
                        LastName = lastName,
                        BirthDate = birthDate,
                        dfeta_NINumber = nino,
                        EMailAddress1 = email,
                        Address1_Line1 = Faker.Address.StreetAddress(),
                        Address1_City = Faker.Address.City(),
                        Address1_Country = "United Kingdom",
                        Address1_PostalCode = Faker.Address.UkPostCode()
                    }
                },
                new UpdateRequest()
                {
                    Target = new Contact()
                    {
                        Id = teacherId,
                        dfeta_TRNAllocateRequest = DateTime.UtcNow
                    }
                });

            var createIttTask = txnRequestBuilder.AddRequest<CreateResponse>(new CreateRequest()
            {
                Target = new dfeta_initialteachertraining()
                {
                    dfeta_PersonId = new EntityReference(Contact.EntityLogicalName, teacherId),
                    dfeta_EstablishmentId = new EntityReference(Account.EntityLogicalName, ittProvider.Id),
                    dfeta_ProgrammeType = programmeType,
                    dfeta_Result = assessmentOnly ? dfeta_ITTResult.UnderAssessment : dfeta_ITTResult.InTraining
                }
            });

            var createQtsTask = txnRequestBuilder.AddRequest<CreateResponse>(new CreateRequest()
            {
                Target = new dfeta_qtsregistration()
                {
                    dfeta_PersonId = new EntityReference(Contact.EntityLogicalName, teacherId),
                    dfeta_EarlyYearsStatusId = earlyYearsStatusId.HasValue ? new EntityReference(dfeta_earlyyearsstatus.EntityLogicalName, earlyYearsStatusId.Value) : null,
                    dfeta_TeacherStatusId = teacherStatusId.HasValue ? new EntityReference(dfeta_teacherstatus.EntityLogicalName, teacherStatusId.Value) : null
                }
            });

            if (withQualification)
            {
                txnRequestBuilder.AddRequest(new CreateRequest()
                {
                    Target = new dfeta_qualification()
                    {
                        dfeta_PersonId = new EntityReference(Contact.EntityLogicalName, teacherId),
                        dfeta_HE_CountryId = new EntityReference(dfeta_qualification.EntityLogicalName, countryId),
                        dfeta_HE_EstablishmentId = new EntityReference(Account.EntityLogicalName, ittProvider.Id),
                        dfeta_Type = dfeta_qualification_dfeta_Type.HigherEducation,
                        dfeta_HE_ClassDivision = dfeta_classdivision.Pass,
                        dfeta_HE_CompletionDate = DateTime.Now.AddMonths(-1),
                        dfeta_HE_HESubject1Id = new EntityReference(dfeta_hesubject.EntityLogicalName, heSubjectId),
                        dfeta_HE_HESubject2Id = new EntityReference(dfeta_hesubject.EntityLogicalName, heSubject2Id),
                        dfeta_HE_HESubject3Id = new EntityReference(dfeta_hesubject.EntityLogicalName, heSubject3Id),
                        dfeta_HE_HEQualificationId = new EntityReference(dfeta_hequalification.EntityLogicalName, qualificationId)
                    }
                });
            }

            if (withActiveSanction)
            {
                txnRequestBuilder.AddRequest(new CreateRequest()
                {
                    Target = new dfeta_sanction()
                    {
                        dfeta_PersonId = new EntityReference(Contact.EntityLogicalName, teacherId),
                        dfeta_SanctionCodeId = new EntityReference(dfeta_sanction.EntityLogicalName, new Guid("95790cd0-c83b-e311-82ec-005056b1356a"))  // T1
                    }
                });
            }
            await txnRequestBuilder.Execute();

            var ittId = createIttTask.GetResponse().id;
            var qtsId = createQtsTask.GetResponse().id;

            return new CreatePersonResult(teacherId, firstName, lastName, DateOnly.FromDateTime(birthDate), nino, ittId, qtsId, ittProvider.Id, ittProviderUkprn);
        }

        public record CreatePersonResult(
            Guid TeacherId,
            string FirstName,
            string LastName,
            DateOnly BirthDate,
            string Nino,
            Guid InitialTeacherTrainingId,
            Guid QtsRegistrationId,
            Guid IttProviderId,
            string IttProviderUkprn);
    }
}
