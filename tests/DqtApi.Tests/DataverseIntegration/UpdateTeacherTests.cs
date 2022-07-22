using System;
using System.Linq;
using System.Threading.Tasks;
using DqtApi.DataStore.Crm;
using DqtApi.DataStore.Crm.Models;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Xunit;

namespace DqtApi.Tests.DataverseIntegration
{
    public class UpdateTeacherTests : IAsyncLifetime
    {
        private readonly CrmClientFixture.TestDataScope _dataScope;
        private readonly DataverseAdapter _dataverseAdapter;
        private readonly IOrganizationServiceAsync _organizationService;
        private readonly TestDataHelper _testDataHelper;

        public UpdateTeacherTests(CrmClientFixture crmClientFixture)
        {
            _dataScope = crmClientFixture.CreateTestDataScope();
            _dataverseAdapter = _dataScope.CreateDataverseAdapter();
            _organizationService = _dataScope.OrganizationService;
            _testDataHelper = _dataScope.CreateTestDataHelper();
        }

        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync() => await _dataScope.DisposeAsync();

        [Fact]
        public async Task Given_update_without_providing_qualification_ukprn_returns_success()
        {
            // Arrange
            var (teacherId, ittProviderUkprn) = await CreatePerson(earlyYears: false, hasActiveSanctions: false);

            // Act
            var (result, transactionRequest) = await _dataverseAdapter.UpdateTeacherImpl(new UpdateTeacherCommand()
            {
                TeacherId = teacherId,
                InitialTeacherTraining = new UpdateTeacherCommandInitialTeacherTraining()
                {
                    ProviderUkprn = ittProviderUkprn,
                    ProgrammeStartDate = new DateOnly(2011, 11, 01),
                    ProgrammeEndDate = new DateOnly(2012, 11, 01),
                    ProgrammeType = dfeta_ITTProgrammeType.RegisteredTeacherProgramme,
                    Subject1 = "100366",  // computer science
                    Subject2 = "100403",  // mathematics
                    Subject3 = "100302",  // history
                    AgeRangeFrom = dfeta_AgeRange._11,
                    AgeRangeTo = dfeta_AgeRange._12
                },
                Qualification = new UpdateTeacherCommandQualification()
                {
                    CountryCode = "XK",
                    Subject = "100366",  // computer science
                    Class = dfeta_classdivision.Firstclasshonours,
                    Date = new DateOnly(2022, 01, 28),
                }
            });

            // Assert
            Assert.True(result.Succeeded);
            transactionRequest.AssertSingleUpsertRequest<dfeta_qualification>();
        }

        [Fact]
        public async Task Given_existing_itt_update_programmetype_from_qts_to_another_qts_programmetype_succeeds()
        {
            // Arrange
            var (teacherId, ittProviderUkprn) = await CreatePerson(earlyYears: false, hasActiveSanctions: false);

            var updateHeSubjectId = await _dataverseAdapter.GetHeSubjectByCode("100366");  // computer science
            var updatedHeCountryId = await _dataverseAdapter.GetCountry("XK");
            var updateIttSubject1Id = await _dataverseAdapter.GetIttSubjectByCode("100403");  // mathematics
            var updateIttSubject2Id = await _dataverseAdapter.GetIttSubjectByCode("100366");  // computer science

            // Act
            var (result, transactionRequest) = await _dataverseAdapter.UpdateTeacherImpl(new UpdateTeacherCommand()
            {
                TeacherId = teacherId,
                InitialTeacherTraining = new UpdateTeacherCommandInitialTeacherTraining()
                {
                    ProviderUkprn = ittProviderUkprn,
                    ProgrammeStartDate = new DateOnly(2011, 11, 01),
                    ProgrammeEndDate = new DateOnly(2012, 11, 01),
                    ProgrammeType = dfeta_ITTProgrammeType.RegisteredTeacherProgramme,
                    Subject1 = "100366",  // computer science
                    Subject2 = "100403",  // mathematics
                    Subject3 = "100302",  // history
                    AgeRangeFrom = dfeta_AgeRange._11,
                    AgeRangeTo = dfeta_AgeRange._12
                },
                Qualification = new UpdateTeacherCommandQualification()
                {
                    CountryCode = "XK",
                    Subject = "100366",  // computer science
                    Class = dfeta_classdivision.Firstclasshonours,
                    Date = new DateOnly(2022, 01, 28),
                    ProviderUkprn = ittProviderUkprn,
                }
            });

            var itt = await _dataverseAdapter.GetInitialTeacherTrainingByTeacher(
                teacherId,
                columnNames: new[]
                {
                    dfeta_initialteachertraining.Fields.dfeta_ProgrammeType,
                    dfeta_initialteachertraining.Fields.dfeta_Result,
                    dfeta_initialteachertraining.Fields.dfeta_EstablishmentId,
                    dfeta_initialteachertraining.Fields.StateCode,
                    dfeta_initialteachertraining.Fields.dfeta_ProgrammeEndDate,
                    dfeta_initialteachertraining.Fields.dfeta_ProgrammeStartDate,
                    dfeta_initialteachertraining.Fields.dfeta_AgeRangeFrom,
                    dfeta_initialteachertraining.Fields.dfeta_AgeRangeTo,
                    dfeta_initialteachertraining.Fields.dfeta_Result,
                    dfeta_initialteachertraining.Fields.dfeta_Subject1Id,
                    dfeta_initialteachertraining.Fields.dfeta_Subject2Id,
                });

            // Assert
            Assert.True(result.Succeeded);

            Assert.Collection(
                itt,
                item1 => Assert.Equal(dfeta_ITTProgrammeType.RegisteredTeacherProgramme, item1.dfeta_ProgrammeType));
        }

        [Theory]
        [InlineData(dfeta_ITTProgrammeType.EYITTAssessmentOnly)]
        [InlineData(dfeta_ITTProgrammeType.EYITTGraduateEmploymentBased)]
        [InlineData(dfeta_ITTProgrammeType.EYITTGraduateEntry)]
        [InlineData(dfeta_ITTProgrammeType.EYITTSchoolDirect_EarlyYears)]
        [InlineData(dfeta_ITTProgrammeType.EYITTUndergraduate)]
        public async Task Given_existing_itt_update_programmetype_from_eyts_to_another_eyts_programmetype_succeeds(dfeta_ITTProgrammeType programmeType)
        {
            // Arrange
            var (teacherId, ittProviderUkprn) = await CreatePerson(earlyYears: true, hasActiveSanctions: false);

            var updateHeSubjectId = await _dataverseAdapter.GetHeSubjectByCode("100366");  // computer science
            var updatedHeCountryId = await _dataverseAdapter.GetCountry("XK");
            var updateIttSubject1Id = await _dataverseAdapter.GetIttSubjectByCode("100403");  // mathematics
            var updateIttSubject2Id = await _dataverseAdapter.GetIttSubjectByCode("100366");  // computer science

            // Act
            var (result, transactionRequest) = await _dataverseAdapter.UpdateTeacherImpl(new UpdateTeacherCommand()
            {
                TeacherId = teacherId,
                InitialTeacherTraining = new UpdateTeacherCommandInitialTeacherTraining()
                {
                    ProviderUkprn = ittProviderUkprn,
                    ProgrammeStartDate = new DateOnly(2011, 11, 01),
                    ProgrammeEndDate = new DateOnly(2012, 11, 01),
                    ProgrammeType = programmeType,
                    Subject1 = "100366",  // computer science
                    Subject2 = "100403",  // mathematics
                    Subject3 = "100302",  // history
                    AgeRangeFrom = dfeta_AgeRange._11,
                    AgeRangeTo = dfeta_AgeRange._12
                },
                Qualification = new UpdateTeacherCommandQualification()
                {
                    CountryCode = "XK",
                    Subject = "100366",  // computer science
                    Class = dfeta_classdivision.Firstclasshonours,
                    Date = new DateOnly(2022, 01, 28),
                    ProviderUkprn = ittProviderUkprn,
                }
            });

            var itt = await _dataverseAdapter.GetInitialTeacherTrainingByTeacher(
                teacherId,
                columnNames: new[]
                {
                    dfeta_initialteachertraining.Fields.dfeta_ProgrammeType,
                    dfeta_initialteachertraining.Fields.dfeta_Result,
                    dfeta_initialteachertraining.Fields.dfeta_EstablishmentId,
                    dfeta_initialteachertraining.Fields.StateCode,
                    dfeta_initialteachertraining.Fields.dfeta_ProgrammeEndDate,
                    dfeta_initialteachertraining.Fields.dfeta_ProgrammeStartDate,
                    dfeta_initialteachertraining.Fields.dfeta_AgeRangeFrom,
                    dfeta_initialteachertraining.Fields.dfeta_AgeRangeTo,
                    dfeta_initialteachertraining.Fields.dfeta_Result,
                    dfeta_initialteachertraining.Fields.dfeta_Subject1Id,
                    dfeta_initialteachertraining.Fields.dfeta_Subject2Id,
                });

            // Assert
            Assert.True(result.Succeeded);

            Assert.Collection(
                itt,
                item1 => Assert.Equal(programmeType, item1.dfeta_ProgrammeType));
        }

        [Fact]
        public async Task Given_updating_existing_teacher_does_not_change_itt_result()
        {
            // Arrange
            var (teacherId, ittProviderUkprn) = await CreatePerson(earlyYears: true, hasActiveSanctions: false);

            // Act
            var (result, transactionRequest) = await _dataverseAdapter.UpdateTeacherImpl(new UpdateTeacherCommand()
            {
                TeacherId = teacherId,
                InitialTeacherTraining = new UpdateTeacherCommandInitialTeacherTraining()
                {
                    ProviderUkprn = ittProviderUkprn,
                    ProgrammeStartDate = new DateOnly(2011, 11, 01),
                    ProgrammeEndDate = new DateOnly(2012, 11, 01),
                    ProgrammeType = dfeta_ITTProgrammeType.EYITTAssessmentOnly,
                    Subject1 = "100403",  // mathematics
                    Subject2 = "100366",  // computer science
                    AgeRangeFrom = dfeta_AgeRange._11,
                    AgeRangeTo = dfeta_AgeRange._12
                },
                Qualification = new UpdateTeacherCommandQualification()
                {
                    CountryCode = "XK",
                    Subject = "100366",  // computer science
                    Class = dfeta_classdivision.Firstclasshonours,
                    Date = new DateOnly(2022, 01, 28),
                    ProviderUkprn = ittProviderUkprn,
                }
            });

            var itt = await _dataverseAdapter.GetInitialTeacherTrainingByTeacher(
                teacherId,
                columnNames: new[]
                {
                    dfeta_initialteachertraining.Fields.dfeta_ProgrammeType,
                    dfeta_initialteachertraining.Fields.dfeta_Result,
                    dfeta_initialteachertraining.Fields.dfeta_EstablishmentId,
                    dfeta_initialteachertraining.Fields.StateCode,
                    dfeta_initialteachertraining.Fields.dfeta_ProgrammeEndDate,
                    dfeta_initialteachertraining.Fields.dfeta_ProgrammeStartDate,
                    dfeta_initialteachertraining.Fields.dfeta_AgeRangeFrom,
                    dfeta_initialteachertraining.Fields.dfeta_AgeRangeTo,
                    dfeta_initialteachertraining.Fields.dfeta_Result,
                    dfeta_initialteachertraining.Fields.dfeta_Subject1Id,
                    dfeta_initialteachertraining.Fields.dfeta_Subject2Id,
                    dfeta_initialteachertraining.Fields.dfeta_Subject3Id
                });

            // Assert
            Assert.True(result.Succeeded);
            Assert.Collection(itt,
                item1 => Assert.Equal(dfeta_ITTResult.InTraining, item1.dfeta_Result)
            );
        }

        [Fact]
        public async Task Given_updating_existing_contact_update_without_subject3_returns_success()
        {
            // Arrange
            var (teacherId, ittProviderUkprn) = await CreatePerson(earlyYears: true, hasActiveSanctions: false);

            var updateHeSubjectId = await _dataverseAdapter.GetHeSubjectByCode("100366");  // computer science
            var updatedHeCountryId = await _dataverseAdapter.GetCountry("XK");
            var updateIttSubject1Id = await _dataverseAdapter.GetIttSubjectByCode("100403");  // mathematics
            var updateIttSubject2Id = await _dataverseAdapter.GetIttSubjectByCode("100366");  // computer science

            // Act
            var (result, transactionRequest) = await _dataverseAdapter.UpdateTeacherImpl(new UpdateTeacherCommand()
            {
                TeacherId = teacherId,
                InitialTeacherTraining = new UpdateTeacherCommandInitialTeacherTraining()
                {
                    ProviderUkprn = ittProviderUkprn,
                    ProgrammeStartDate = new DateOnly(2011, 11, 01),
                    ProgrammeEndDate = new DateOnly(2012, 11, 01),
                    ProgrammeType = dfeta_ITTProgrammeType.EYITTAssessmentOnly,
                    Subject1 = "100403",  // mathematics
                    Subject2 = "100366",  // computer science
                    AgeRangeFrom = dfeta_AgeRange._11,
                    AgeRangeTo = dfeta_AgeRange._12
                },
                Qualification = new UpdateTeacherCommandQualification()
                {
                    CountryCode = "XK",
                    Subject = "100366",  // computer science
                    Class = dfeta_classdivision.Firstclasshonours,
                    Date = new DateOnly(2022, 01, 28),
                    ProviderUkprn = ittProviderUkprn,
                }
            });

            var oldProvider = (await _dataverseAdapter.GetOrganizationsByUkprn(ittProviderUkprn)).Single().Id;

            var qualifications = await _dataverseAdapter.GetQualificationsForTeacher(
                teacherId,
                columnNames: new[]
                {
                    dfeta_qualification.Fields.dfeta_CompletionorAwardDate,
                    dfeta_qualification.Fields.dfeta_Type,
                    dfeta_qualification.Fields.dfeta_HE_EstablishmentId,
                    dfeta_qualification.Fields.dfeta_PersonId,
                    dfeta_qualification.Fields.dfeta_HE_ClassDivision,
                    dfeta_qualification.Fields.dfeta_HE_CompletionDate,
                    dfeta_qualification.Fields.dfeta_HE_HESubject1Id,
                    dfeta_qualification.Fields.dfeta_HE_CountryId,
                });

            var itt = await _dataverseAdapter.GetInitialTeacherTrainingByTeacher(
                teacherId,
                columnNames: new[]
                {
                    dfeta_initialteachertraining.Fields.dfeta_ProgrammeType,
                    dfeta_initialteachertraining.Fields.dfeta_Result,
                    dfeta_initialteachertraining.Fields.dfeta_EstablishmentId,
                    dfeta_initialteachertraining.Fields.StateCode,
                    dfeta_initialteachertraining.Fields.dfeta_ProgrammeEndDate,
                    dfeta_initialteachertraining.Fields.dfeta_ProgrammeStartDate,
                    dfeta_initialteachertraining.Fields.dfeta_AgeRangeFrom,
                    dfeta_initialteachertraining.Fields.dfeta_AgeRangeTo,
                    dfeta_initialteachertraining.Fields.dfeta_Result,
                    dfeta_initialteachertraining.Fields.dfeta_Subject1Id,
                    dfeta_initialteachertraining.Fields.dfeta_Subject2Id,
                    dfeta_initialteachertraining.Fields.dfeta_Subject3Id
                });

            // Assert
            Assert.True(result.Succeeded);

            Assert.Collection(
                itt,
                item1 =>
                {
                    Assert.Equal(oldProvider, item1.dfeta_EstablishmentId.Id);
                    Assert.Equal(dfeta_ITTProgrammeType.EYITTAssessmentOnly, item1.dfeta_ProgrammeType);
                    Assert.Equal(dfeta_AgeRange._11, item1.dfeta_AgeRangeFrom);
                    Assert.Equal(dfeta_AgeRange._12, item1.dfeta_AgeRangeTo);
                    Assert.Equal(dfeta_ITTResult.InTraining, item1.dfeta_Result);
                    Assert.Equal(updateIttSubject1Id.Id, item1.dfeta_Subject1Id.Id);
                    Assert.Equal(updateIttSubject2Id.Id, item1.dfeta_Subject2Id.Id);
                    Assert.Null(item1.dfeta_Subject3Id);
                });
        }

        [Fact]
        public async Task Given_existing_contact_update_itt_and_qualification_returns_success()
        {
            // Arrange
            var (teacherId, ittProviderUkprn) = await CreatePerson(earlyYears: true, hasActiveSanctions: false);

            var updateHeSubject = await _dataverseAdapter.GetHeSubjectByCode("100366");  // computer science
            var updatedHeCountry = await _dataverseAdapter.GetCountry("XK");
            var updateIttSubject1 = await _dataverseAdapter.GetIttSubjectByCode("100403");  // mathematics
            var updateIttSubject2 = await _dataverseAdapter.GetIttSubjectByCode("100366");  // computer science
            var updateIttSubject3 = await _dataverseAdapter.GetIttSubjectByCode("100302");  // history
            var updateIttQualification = await _dataverseAdapter.GetIttQualificationByCode("001");  // BEd
            var updateHeQualification = await _dataverseAdapter.GetHeQualificationByCode("401");  // Higher Degree

            // Act
            var (result, transactionRequest) = await _dataverseAdapter.UpdateTeacherImpl(new UpdateTeacherCommand()
            {
                TeacherId = teacherId,
                InitialTeacherTraining = new UpdateTeacherCommandInitialTeacherTraining()
                {
                    ProviderUkprn = ittProviderUkprn,
                    ProgrammeStartDate = new DateOnly(2011, 11, 01),
                    ProgrammeEndDate = new DateOnly(2012, 11, 01),
                    ProgrammeType = dfeta_ITTProgrammeType.EYITTAssessmentOnly,
                    Subject1 = "100403",  // mathematics
                    Subject2 = "100366",  // computer science
                    Subject3 = "100302",  // history
                    AgeRangeFrom = dfeta_AgeRange._11,
                    AgeRangeTo = dfeta_AgeRange._12,
                    IttQualificationValue = updateIttQualification.dfeta_Value
                },
                Qualification = new UpdateTeacherCommandQualification()
                {
                    CountryCode = "XK",
                    Subject = "100366",  // computer science
                    Class = dfeta_classdivision.Firstclasshonours,
                    Date = new DateOnly(2022, 01, 28),
                    ProviderUkprn = ittProviderUkprn,
                    HeQualificationValue = updateHeQualification.dfeta_Value
                }
            });

            var oldProvider = (await _dataverseAdapter.GetOrganizationsByUkprn(ittProviderUkprn)).Single().Id;

            var qualifications = await _dataverseAdapter.GetQualificationsForTeacher(
                teacherId,
                columnNames: new[]
                {
                    dfeta_qualification.Fields.dfeta_CompletionorAwardDate,
                    dfeta_qualification.Fields.dfeta_Type,
                    dfeta_qualification.Fields.dfeta_HE_EstablishmentId,
                    dfeta_qualification.Fields.dfeta_PersonId,
                    dfeta_qualification.Fields.dfeta_HE_ClassDivision,
                    dfeta_qualification.Fields.dfeta_HE_CompletionDate,
                    dfeta_qualification.Fields.dfeta_HE_HESubject1Id,
                    dfeta_qualification.Fields.dfeta_HE_CountryId,
                    dfeta_qualification.Fields.dfeta_HE_HEQualificationId
                });

            var itt = await _dataverseAdapter.GetInitialTeacherTrainingByTeacher(
                teacherId,
                columnNames: new[]
                {
                    dfeta_initialteachertraining.Fields.dfeta_ProgrammeType,
                    dfeta_initialteachertraining.Fields.dfeta_Result,
                    dfeta_initialteachertraining.Fields.dfeta_EstablishmentId,
                    dfeta_initialteachertraining.Fields.StateCode,
                    dfeta_initialteachertraining.Fields.dfeta_ProgrammeEndDate,
                    dfeta_initialteachertraining.Fields.dfeta_ProgrammeStartDate,
                    dfeta_initialteachertraining.Fields.dfeta_AgeRangeFrom,
                    dfeta_initialteachertraining.Fields.dfeta_AgeRangeTo,
                    dfeta_initialteachertraining.Fields.dfeta_Result,
                    dfeta_initialteachertraining.Fields.dfeta_Subject1Id,
                    dfeta_initialteachertraining.Fields.dfeta_Subject2Id,
                    dfeta_initialteachertraining.Fields.dfeta_Subject3Id,
                    dfeta_initialteachertraining.Fields.dfeta_ITTQualificationId
                });

            // Assert
            Assert.True(result.Succeeded);

            Assert.Collection(
                itt,
                item1 =>
                {
                    Assert.Equal(oldProvider, item1.dfeta_EstablishmentId.Id);
                    Assert.Equal(dfeta_ITTProgrammeType.EYITTAssessmentOnly, item1.dfeta_ProgrammeType);
                    Assert.Equal(dfeta_AgeRange._11, item1.dfeta_AgeRangeFrom);
                    Assert.Equal(dfeta_AgeRange._12, item1.dfeta_AgeRangeTo);
                    Assert.Equal(dfeta_ITTResult.InTraining, item1.dfeta_Result);
                    Assert.Equal(updateIttSubject1.Id, item1.dfeta_Subject1Id.Id);
                    Assert.Equal(updateIttSubject2.Id, item1.dfeta_Subject2Id.Id);
                    Assert.Equal(updateIttSubject3.Id, item1.dfeta_Subject3Id.Id);
                    Assert.Equal(updateIttQualification.Id, item1.dfeta_ITTQualificationId.Id);
                }
            );

            Assert.Collection(
                qualifications,
                item1 =>
                {
                    Assert.Equal(updatedHeCountry.Id, item1.dfeta_HE_CountryId.Id);
                    Assert.Equal(updateHeSubject.Id, item1.dfeta_HE_HESubject1Id.Id);
                    Assert.Equal(oldProvider, item1.dfeta_HE_EstablishmentId.Id);
                    Assert.Equal(dfeta_classdivision.Firstclasshonours, item1.dfeta_HE_ClassDivision);
                    Assert.Equal(new DateTime(2022, 01, 28), item1.dfeta_CompletionorAwardDate);
                    Assert.Equal(updateHeQualification.Id, item1.dfeta_HE_HEQualificationId.Id);
                });
        }

        [Fact]
        public async Task Given_update_itt_and_qualification_with_noactive_sanctions_does_not_create_crm_task_and_returns_success()
        {
            // Arrange
            var (teacherId, ittProviderUkprn) = await CreatePerson(earlyYears: true, hasActiveSanctions: false);

            // Act
            var (result, transactionRequest) = await _dataverseAdapter.UpdateTeacherImpl(new UpdateTeacherCommand()
            {
                TeacherId = teacherId,
                InitialTeacherTraining = new UpdateTeacherCommandInitialTeacherTraining()
                {
                    ProviderUkprn = ittProviderUkprn,
                    ProgrammeStartDate = new DateOnly(2011, 11, 01),
                    ProgrammeEndDate = new DateOnly(2012, 11, 01),
                    ProgrammeType = dfeta_ITTProgrammeType.EYITTAssessmentOnly,
                    Subject1 = "100366",  // computer science
                    Subject2 = "100403",  // mathematics
                    Subject3 = "100302",  // history
                    AgeRangeFrom = dfeta_AgeRange._11,
                    AgeRangeTo = dfeta_AgeRange._12
                },
                Qualification = new UpdateTeacherCommandQualification()
                {
                    CountryCode = "XK",
                    Subject = "100366",  // computer science
                    Class = dfeta_classdivision.Firstclasshonours,
                    Date = new DateOnly(2022, 01, 28),
                    ProviderUkprn = ittProviderUkprn,
                }
            });

            // Assert
            Assert.True(result.Succeeded);
            transactionRequest.AssertDoesNotContainCreateRequest<CrmTask>();
        }

        [Theory]
        [InlineData(dfeta_ITTProgrammeType.EYITTAssessmentOnly)]
        [InlineData(dfeta_ITTProgrammeType.EYITTGraduateEmploymentBased)]
        [InlineData(dfeta_ITTProgrammeType.EYITTGraduateEntry)]
        [InlineData(dfeta_ITTProgrammeType.EYITTSchoolDirect_EarlyYears)]
        [InlineData(dfeta_ITTProgrammeType.EYITTUndergraduate)]
        public async Task Given_qts_itt_cannot_change_qts_programmetype_to_eyts_programmetype(dfeta_ITTProgrammeType programmeType)
        {
            // Arrange
            var (teacherId, ittProviderUkprn) = await CreatePerson(earlyYears: false, hasActiveSanctions: false);

            // Act
            var (result, _) = await _dataverseAdapter.UpdateTeacherImpl(new UpdateTeacherCommand()
            {
                TeacherId = teacherId,
                InitialTeacherTraining = new UpdateTeacherCommandInitialTeacherTraining()
                {
                    ProviderUkprn = ittProviderUkprn,
                    ProgrammeStartDate = new DateOnly(2011, 11, 01),
                    ProgrammeEndDate = new DateOnly(2012, 11, 01),
                    ProgrammeType = programmeType,
                    Subject1 = "100366",  // computer science
                    Subject2 = "100403",  // mathematics
                    Subject3 = "100302",  // history
                    AgeRangeFrom = dfeta_AgeRange._11,
                    AgeRangeTo = dfeta_AgeRange._12
                },
                Qualification = new UpdateTeacherCommandQualification()
                {
                    CountryCode = "XK",
                    Subject = "100366",  // computer science
                    Class = dfeta_classdivision.Firstclasshonours,
                    Date = new DateOnly(2022, 01, 28),
                    ProviderUkprn = ittProviderUkprn,
                }
            });

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal(UpdateTeacherFailedReasons.CannotChangeProgrammeType, result.FailedReasons);
        }

        [Theory]
        [InlineData(dfeta_ITTProgrammeType.AssessmentOnlyRoute)]
        [InlineData(dfeta_ITTProgrammeType.Apprenticeship)]
        [InlineData(dfeta_ITTProgrammeType.Core)]
        [InlineData(dfeta_ITTProgrammeType.CoreFlexible)]
        [InlineData(dfeta_ITTProgrammeType.FutureTeachingScholars)]
        [InlineData(dfeta_ITTProgrammeType.GraduateTeacherProgramme)]
        [InlineData(dfeta_ITTProgrammeType.HEI)]
        [InlineData(dfeta_ITTProgrammeType.LicensedTeacherProgramme)]
        [InlineData(dfeta_ITTProgrammeType.OverseasTrainedTeacherProgramme)]
        [InlineData(dfeta_ITTProgrammeType.RegisteredTeacherProgramme)]
        [InlineData(dfeta_ITTProgrammeType.SchoolDirecttrainingprogramme)]
        [InlineData(dfeta_ITTProgrammeType.SchoolDirecttrainingprogramme_Salaried)]
        [InlineData(dfeta_ITTProgrammeType.SchoolDirecttrainingprogramme_Selffunded)]
        [InlineData(dfeta_ITTProgrammeType.TeachFirstProgramme)]
        [InlineData(dfeta_ITTProgrammeType.TeachFirstProgramme_CC)]
        [InlineData(dfeta_ITTProgrammeType.UndergraduateOptIn)]
        public async Task Given_earlyyears_itt_cannot_change_eyts_programmetype_to_qts(dfeta_ITTProgrammeType programmeType)
        {
            // Arrange
            var (teacherId, ittProviderUkprn) = await CreatePerson(earlyYears: true, hasActiveSanctions: false);

            // Act
            var (result, _) = await _dataverseAdapter.UpdateTeacherImpl(new UpdateTeacherCommand()
            {
                TeacherId = teacherId,
                InitialTeacherTraining = new UpdateTeacherCommandInitialTeacherTraining()
                {
                    ProviderUkprn = ittProviderUkprn,
                    ProgrammeStartDate = new DateOnly(2011, 11, 01),
                    ProgrammeEndDate = new DateOnly(2012, 11, 01),
                    ProgrammeType = programmeType,
                    Subject1 = "100366",  // computer science
                    Subject2 = "100403",  // mathematics
                    Subject3 = "100302",  // history
                    AgeRangeFrom = dfeta_AgeRange._11,
                    AgeRangeTo = dfeta_AgeRange._12
                },
                Qualification = new UpdateTeacherCommandQualification()
                {
                    CountryCode = "XK",
                    Subject = "100366",  // computer science
                    Class = dfeta_classdivision.Firstclasshonours,
                    Date = new DateOnly(2022, 01, 28),
                    ProviderUkprn = ittProviderUkprn,
                }
            });

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal(UpdateTeacherFailedReasons.CannotChangeProgrammeType, result.FailedReasons);
        }

        [Fact]
        public async Task Given_two_or_more_qualifications_does_not_create_new_qualification_and_creates_warning_crm_task()
        {
            // Arrange
            var (teacherId, ittProviderUkprn) = await CreatePerson(earlyYears: true, hasActiveSanctions: false);

            var countryId = await _dataverseAdapter.GetCountry("XK");
            var qualificationSubject1Id = await _dataverseAdapter.GetHeSubjectByCode("100366");  // computer science
            var providerId = (await _dataverseAdapter.GetOrganizationsByUkprn(ittProviderUkprn)).Single();
            var qualification = await _dataverseAdapter.GetHeQualificationByCode("400");  // First Degree

            var txnResponse = (ExecuteTransactionResponse)await _organizationService.ExecuteAsync(new ExecuteTransactionRequest()
            {
                Requests = new()
                {
                    new CreateRequest()
                    {
                        Target = new dfeta_qualification()
                        {
                            dfeta_PersonId = new EntityReference(Contact.EntityLogicalName, teacherId),
                            dfeta_HE_CountryId = new EntityReference(dfeta_qualification.EntityLogicalName, countryId.Id),
                            dfeta_HE_EstablishmentId = new EntityReference(Account.EntityLogicalName, providerId.Id),
                            dfeta_Type = dfeta_qualification_dfeta_Type.HigherEducation,
                            dfeta_HE_ClassDivision = dfeta_classdivision.Pass,
                            dfeta_HE_CompletionDate = DateTime.Now.AddMonths(-1),
                            dfeta_HE_HESubject1Id = new EntityReference(dfeta_hesubject.EntityLogicalName, qualificationSubject1Id.Id),
                            dfeta_HE_HEQualificationId = new EntityReference(dfeta_hequalification.EntityLogicalName, qualification.Id)
                        }
                    }
                },
                ReturnResponses = true
            });

            await _dataverseAdapter.GetQualificationsForTeacher(
                teacherId,
                columnNames: new[]
                {
                    dfeta_qualification.Fields.dfeta_CompletionorAwardDate,
                    dfeta_qualification.Fields.dfeta_Type,
                    dfeta_qualification.Fields.dfeta_HE_EstablishmentId,
                    dfeta_qualification.Fields.dfeta_PersonId,
                    dfeta_qualification.Fields.dfeta_HE_ClassDivision,
                    dfeta_qualification.Fields.dfeta_HE_CompletionDate,
                    dfeta_qualification.Fields.dfeta_HE_HESubject1Id,
                    dfeta_qualification.Fields.dfeta_HE_CountryId,
                });

            // Act
            var (result, transactionRequest) = await _dataverseAdapter.UpdateTeacherImpl(new UpdateTeacherCommand()
            {
                TeacherId = teacherId,
                InitialTeacherTraining = new UpdateTeacherCommandInitialTeacherTraining()
                {
                    ProviderUkprn = ittProviderUkprn,
                    ProgrammeStartDate = new DateOnly(2011, 11, 01),
                    ProgrammeEndDate = new DateOnly(2012, 11, 01),
                    ProgrammeType = dfeta_ITTProgrammeType.EYITTAssessmentOnly,
                    Subject1 = "100366",  // computer science
                    Subject2 = "100403",  // mathematics
                    Subject3 = "100302",  // history
                    AgeRangeFrom = dfeta_AgeRange._11,
                    AgeRangeTo = dfeta_AgeRange._12
                },
                Qualification = new UpdateTeacherCommandQualification()
                {
                    CountryCode = "XK",
                    Subject = "100366",  // computer science
                    Class = dfeta_classdivision.Fourthclasshonours,
                    Date = new DateOnly(2022, 01, 15),
                    ProviderUkprn = ittProviderUkprn
                }
            });

            var qualifications = await _dataverseAdapter.GetQualificationsForTeacher(
                teacherId,
                columnNames: new[]
                {
                    dfeta_qualification.Fields.dfeta_CompletionorAwardDate,
                    dfeta_qualification.Fields.dfeta_Type,
                    dfeta_qualification.Fields.dfeta_HE_EstablishmentId,
                    dfeta_qualification.Fields.dfeta_PersonId,
                    dfeta_qualification.Fields.dfeta_HE_ClassDivision,
                    dfeta_qualification.Fields.dfeta_HE_CompletionDate,
                    dfeta_qualification.Fields.dfeta_HE_HESubject1Id,
                    dfeta_qualification.Fields.dfeta_HE_CountryId,
                });

            // Assert
            Assert.True(result.Succeeded);
            transactionRequest.AssertDoesNotContainUpsertRequest<dfeta_qualification>();
            var crmTask = transactionRequest.AssertSingleCreateRequest<CrmTask>();
            Assert.Equal($"Incoming Subject: 100366,Incoming Date: {new DateOnly(2022, 01, 15)},Incoming Class {dfeta_classdivision.Fourthclasshonours},Incoming ProviderUkprn {ittProviderUkprn},Incoming CountryCode: XK", crmTask.Description);
            Assert.Equal("Notification for QTS unit - Register: matched record holds multiple qualifications", crmTask.Category);
            Assert.Equal("Register: multiple qualifications", crmTask.Subject);

            Assert.Collection(
                qualifications,
                item1 =>
                {
                    Assert.Equal(providerId.Id, item1.dfeta_HE_EstablishmentId.Id);
                },
                item2 =>
                {
                    Assert.Equal(providerId.Id, item2.dfeta_HE_EstablishmentId.Id);
                });
        }

        [Theory]
        [InlineData(dfeta_ITTProgrammeType.AssessmentOnlyRoute, dfeta_ITTResult.UnderAssessment)]
        [InlineData(dfeta_ITTProgrammeType.Apprenticeship, dfeta_ITTResult.InTraining)]
        [InlineData(dfeta_ITTProgrammeType.Core, dfeta_ITTResult.InTraining)]
        [InlineData(dfeta_ITTProgrammeType.CoreFlexible, dfeta_ITTResult.InTraining)]
        [InlineData(dfeta_ITTProgrammeType.EYITTAssessmentOnly, dfeta_ITTResult.InTraining)]
        [InlineData(dfeta_ITTProgrammeType.EYITTGraduateEmploymentBased, dfeta_ITTResult.InTraining)]
        [InlineData(dfeta_ITTProgrammeType.EYITTGraduateEntry, dfeta_ITTResult.InTraining)]
        [InlineData(dfeta_ITTProgrammeType.EYITTSchoolDirect_EarlyYears, dfeta_ITTResult.InTraining)]
        [InlineData(dfeta_ITTProgrammeType.EYITTUndergraduate, dfeta_ITTResult.InTraining)]
        [InlineData(dfeta_ITTProgrammeType.FutureTeachingScholars, dfeta_ITTResult.InTraining)]
        [InlineData(dfeta_ITTProgrammeType.GraduateTeacherProgramme, dfeta_ITTResult.InTraining)]
        [InlineData(dfeta_ITTProgrammeType.HEI, dfeta_ITTResult.InTraining)]
        [InlineData(dfeta_ITTProgrammeType.LicensedTeacherProgramme, dfeta_ITTResult.InTraining)]
        [InlineData(dfeta_ITTProgrammeType.OverseasTrainedTeacherProgramme, dfeta_ITTResult.InTraining)]
        [InlineData(dfeta_ITTProgrammeType.RegisteredTeacherProgramme, dfeta_ITTResult.InTraining)]
        [InlineData(dfeta_ITTProgrammeType.SchoolDirecttrainingprogramme, dfeta_ITTResult.InTraining)]
        [InlineData(dfeta_ITTProgrammeType.SchoolDirecttrainingprogramme_Salaried, dfeta_ITTResult.InTraining)]
        [InlineData(dfeta_ITTProgrammeType.SchoolDirecttrainingprogramme_Selffunded, dfeta_ITTResult.InTraining)]
        [InlineData(dfeta_ITTProgrammeType.TeachFirstProgramme, dfeta_ITTResult.InTraining)]
        [InlineData(dfeta_ITTProgrammeType.TeachFirstProgramme_CC, dfeta_ITTResult.InTraining)]
        [InlineData(dfeta_ITTProgrammeType.UndergraduateOptIn, dfeta_ITTResult.InTraining)]
        public async Task Given_there_are_no_records_matched_create_new_itt_with_correct_result_and_warning_crm_task(dfeta_ITTProgrammeType programmeType, dfeta_ITTResult ittResult)
        {
            // Arrange
            var (teacherId, ittProviderUkprn) = await CreatePerson(earlyYears: true, hasActiveSanctions: false);

            // mark existing itt record as inactive
            var existingItt = await _dataverseAdapter.GetInitialTeacherTrainingByTeacher(
                teacherId,
                columnNames: new[]
                {
                    dfeta_initialteachertraining.Fields.dfeta_ProgrammeType,
                    dfeta_initialteachertraining.Fields.dfeta_Result,
                    dfeta_initialteachertraining.Fields.dfeta_EstablishmentId,
                    dfeta_initialteachertraining.Fields.StateCode,
                    dfeta_initialteachertraining.Fields.dfeta_ProgrammeEndDate,
                    dfeta_initialteachertraining.Fields.dfeta_ProgrammeStartDate,
                    dfeta_initialteachertraining.Fields.dfeta_AgeRangeFrom,
                    dfeta_initialteachertraining.Fields.dfeta_AgeRangeTo,
                    dfeta_initialteachertraining.Fields.dfeta_Result,
                    dfeta_initialteachertraining.Fields.dfeta_Subject1Id,
                    dfeta_initialteachertraining.Fields.dfeta_Subject2Id,
                    dfeta_initialteachertraining.Fields.dfeta_Subject3Id,
                });

            var entity = existingItt.Single();

            await _organizationService.ExecuteAsync(new SetStateRequest()
            {
                EntityMoniker = new EntityReference(dfeta_initialteachertraining.EntityLogicalName, entity.Id),
                State = new OptionSetValue(1),  // Inactive
                Status = new OptionSetValue(2)
            });

            // Act
            var (result, transactionRequest) = await _dataverseAdapter.UpdateTeacherImpl(new UpdateTeacherCommand()
            {
                TeacherId = teacherId,
                InitialTeacherTraining = new UpdateTeacherCommandInitialTeacherTraining()
                {
                    ProviderUkprn = ittProviderUkprn,
                    ProgrammeStartDate = new DateOnly(2011, 11, 01),
                    ProgrammeEndDate = new DateOnly(2012, 11, 01),
                    ProgrammeType = programmeType,
                    Subject1 = "100366",  // computer science
                    Subject2 = "100403",  // mathematics
                    Subject3 = "100302",  // history
                    AgeRangeFrom = dfeta_AgeRange._11,
                    AgeRangeTo = dfeta_AgeRange._12,
                    IttQualificationAim = dfeta_ITTQualificationAim.Professionalstatusbyassessmentonly
                },
                Qualification = new UpdateTeacherCommandQualification()
                {
                    CountryCode = "XK",
                    Subject = "100366",  // computer science
                    Class = dfeta_classdivision.Fourthclasshonours,
                    Date = new DateOnly(2022, 01, 15),
                    ProviderUkprn = ittProviderUkprn,
                }
            });

            var crmTask = transactionRequest.AssertSingleCreateRequest<CrmTask>();
            var itt = transactionRequest.AssertSingleCreateRequest<dfeta_initialteachertraining>();

            // Assert
            Assert.True(result.Succeeded);
            Assert.Equal(ittResult, itt.dfeta_Result);
            Assert.Equal($"Register: missing ITT UKPRN", crmTask.Subject);
            Assert.Equal($"Notification for QTS unit - Register: matched record holds no ITT UKPRN", crmTask.Category);
        }

        [Fact]
        public async Task Given_two_or_more_itt_earlyyears_records_match_create_new_itt_and_warning_crm_task()
        {
            // Arrange
            var (teacherId, ittProviderUkprn) = await CreatePerson(earlyYears: true, hasActiveSanctions: false);

            var providerId = (await _dataverseAdapter.GetOrganizationsByUkprn(ittProviderUkprn)).Single();

            // Create second Itt record
            await _organizationService.ExecuteAsync(new CreateRequest()
            {
                Target = new dfeta_initialteachertraining()
                {
                    dfeta_PersonId = new EntityReference(Contact.EntityLogicalName, teacherId),
                    dfeta_EstablishmentId = new EntityReference(Account.EntityLogicalName, providerId.Id),
                    dfeta_ProgrammeType = dfeta_ITTProgrammeType.EYITTAssessmentOnly,
                    dfeta_Result = dfeta_ITTResult.InTraining,
                    dfeta_AgeRangeFrom = dfeta_AgeRange._00,
                    dfeta_AgeRangeTo = dfeta_AgeRange._18,
                    dfeta_ittqualificationaim = dfeta_ITTQualificationAim.Academicawardonly,
                }
            });

            // Act
            var (result, transactionRequest) = await _dataverseAdapter.UpdateTeacherImpl(new UpdateTeacherCommand()
            {
                TeacherId = teacherId,
                InitialTeacherTraining = new UpdateTeacherCommandInitialTeacherTraining()
                {
                    ProviderUkprn = ittProviderUkprn,
                    ProgrammeStartDate = new DateOnly(2011, 11, 01),
                    ProgrammeEndDate = new DateOnly(2012, 11, 01),
                    ProgrammeType = dfeta_ITTProgrammeType.EYITTAssessmentOnly,
                    Subject1 = "100366",  // computer science
                    Subject2 = "100403",  // mathematics
                    Subject3 = "100302",  // history
                    AgeRangeFrom = dfeta_AgeRange._11,
                    AgeRangeTo = dfeta_AgeRange._12,
                    IttQualificationAim = dfeta_ITTQualificationAim.Professionalstatusbyassessmentonly
                },
                Qualification = new UpdateTeacherCommandQualification()
                {
                    CountryCode = "XK",
                    Subject = "100366",  // computer science
                    Class = dfeta_classdivision.Fourthclasshonours,
                    Date = new DateOnly(2022, 01, 15),
                    ProviderUkprn = ittProviderUkprn,
                }
            });

            var crmTask = transactionRequest.AssertSingleCreateRequest<CrmTask>();

            // Assert
            Assert.True(result.Succeeded);
            Assert.Equal("Notification for QTS unit - Register: matched record holds multiple ITT UKPRNs", crmTask.Category);
            Assert.Equal("Register: multiple ITT UKPRNs", crmTask.Subject);
        }

        [Fact]
        public void Given_crm_task_is_created_when_no_itt_for_ukprn_description_is_correct()
        {
            // Arrange
            var teacherId = Guid.NewGuid();
            var trn = "1234567";
            var updateCommand = new UpdateTeacherCommand() { TRN = trn, TeacherId = teacherId };
            var helper = new DataverseAdapter.UpdateTeacherHelper(_dataverseAdapter, updateCommand);

            // Act
            var crmTask = helper.CreateNoMatchIttReviewTask();

            // Assert
            Assert.Equal($"No ITT UKPRN match for TRN {trn}", crmTask.Description);
        }

        [Fact]
        public void Given_crm_task_is_created_when_multiple_itt_for_ukprn_description_is_correct()
        {
            // Arrange
            var teacherId = Guid.NewGuid();
            var trn = "1234567";
            var updateCommand = new UpdateTeacherCommand() { TRN = trn, TeacherId = teacherId };
            var helper = new DataverseAdapter.UpdateTeacherHelper(_dataverseAdapter, updateCommand);

            // Act
            var crmTask = helper.CreateMultipleMatchIttReviewTask();

            // Assert
            Assert.Equal($"Multiple ITT UKPRNs found for TRN {trn}", crmTask.Description);
        }

        [Fact]
        public void Given_crm_task_is_created_when_qualification_is_created_description_is_correct()
        {
            // Arrange
            var teacherId = Guid.NewGuid();
            var trn = "1234567";
            var subject = "12345";
            var classdivision = dfeta_classdivision.Distinction;
            var countrycode = "XK";
            var date = new DateOnly(1990, 01, 05);
            var ProviderUkprn = "12345";
            var qualification = new UpdateTeacherCommandQualification() { Subject = subject, Class = classdivision, CountryCode = countrycode, Date = date, ProviderUkprn = ProviderUkprn };
            var updateCommand = new UpdateTeacherCommand() { Qualification = qualification, TRN = trn, TeacherId = teacherId };
            var helper = new DataverseAdapter.UpdateTeacherHelper(_dataverseAdapter, updateCommand);

            // Act
            var crmTask = helper.CreateReviewTaskEntityForMultipleQualifications();

            // Assert
            Assert.Equal($"Incoming Subject: {subject},Incoming Date: {date},Incoming Class {classdivision},Incoming ProviderUkprn {ProviderUkprn},Incoming CountryCode: {countrycode}", crmTask.Description);
        }

        [Fact]
        public async Task Given_existing_contact_update_itt_and_qualification_with_existing_active_sanction_returns_success_and_creates_crm_task()
        {
            // Arrange
            var (teacherId, ittProviderUkprn) = await CreatePerson(earlyYears: true, hasActiveSanctions: true);

            var updateHeSubjectId = await _dataverseAdapter.GetHeSubjectByCode("100366");  // computer science
            var updatedHeCountryId = await _dataverseAdapter.GetCountry("XK");
            var updateIttSubject1Id = await _dataverseAdapter.GetIttSubjectByCode("100403");  // mathematics
            var updateIttSubject2Id = await _dataverseAdapter.GetIttSubjectByCode("100366");  // computer science

            // Act
            var (result, transactionRequest) = await _dataverseAdapter.UpdateTeacherImpl(new UpdateTeacherCommand()
            {
                TeacherId = teacherId,
                InitialTeacherTraining = new UpdateTeacherCommandInitialTeacherTraining()
                {
                    ProviderUkprn = ittProviderUkprn,
                    ProgrammeStartDate = new DateOnly(2011, 11, 01),
                    ProgrammeEndDate = new DateOnly(2012, 11, 01),
                    ProgrammeType = dfeta_ITTProgrammeType.EYITTAssessmentOnly,
                    Subject1 = "100403",  // mathematics,
                    Subject2 = "100366",  // computer science
                    Subject3 = "100302",  // history
                    AgeRangeFrom = dfeta_AgeRange._11,
                    AgeRangeTo = dfeta_AgeRange._12,
                    IttQualificationAim = dfeta_ITTQualificationAim.Professionalstatusbyassessmentonly
                },
                Qualification = new UpdateTeacherCommandQualification()
                {
                    CountryCode = "XK",
                    Subject = "100366",  // computer science
                    Class = dfeta_classdivision.Firstclasshonours,
                    Date = new DateOnly(2022, 01, 28),
                    ProviderUkprn = ittProviderUkprn,
                }
            });

            var oldProvider = (await _dataverseAdapter.GetOrganizationsByUkprn(ittProviderUkprn)).Single().Id;

            var qualifications = await _dataverseAdapter.GetQualificationsForTeacher(
                teacherId,
                columnNames: new[]
                {
                    dfeta_qualification.Fields.dfeta_CompletionorAwardDate,
                    dfeta_qualification.Fields.dfeta_Type,
                    dfeta_qualification.Fields.dfeta_HE_EstablishmentId,
                    dfeta_qualification.Fields.dfeta_PersonId,
                    dfeta_qualification.Fields.dfeta_HE_ClassDivision,
                    dfeta_qualification.Fields.dfeta_HE_CompletionDate,
                    dfeta_qualification.Fields.dfeta_HE_HESubject1Id,
                    dfeta_qualification.Fields.dfeta_HE_CountryId,
                });

            var itt = await _dataverseAdapter.GetInitialTeacherTrainingByTeacher(
                teacherId,
                columnNames: new[]
                {
                    dfeta_initialteachertraining.Fields.dfeta_ProgrammeType,
                    dfeta_initialteachertraining.Fields.dfeta_Result,
                    dfeta_initialteachertraining.Fields.dfeta_EstablishmentId,
                    dfeta_initialteachertraining.Fields.StateCode,
                    dfeta_initialteachertraining.Fields.dfeta_ProgrammeEndDate,
                    dfeta_initialteachertraining.Fields.dfeta_ProgrammeStartDate,
                    dfeta_initialteachertraining.Fields.dfeta_AgeRangeFrom,
                    dfeta_initialteachertraining.Fields.dfeta_AgeRangeTo,
                    dfeta_initialteachertraining.Fields.dfeta_Result,
                    dfeta_initialteachertraining.Fields.dfeta_Subject1Id,
                    dfeta_initialteachertraining.Fields.dfeta_Subject2Id,
                    dfeta_initialteachertraining.Fields.dfeta_ittqualificationaim
                });

            // Assert
            Assert.True(result.Succeeded);

            Assert.Collection(
                itt,
                item1 =>
                {
                    Assert.Equal(oldProvider, item1.dfeta_EstablishmentId.Id);
                    Assert.Equal(dfeta_ITTProgrammeType.EYITTAssessmentOnly, item1.dfeta_ProgrammeType);
                    Assert.Equal(dfeta_AgeRange._11, item1.dfeta_AgeRangeFrom);
                    Assert.Equal(dfeta_AgeRange._12, item1.dfeta_AgeRangeTo);
                    Assert.Equal(dfeta_ITTResult.InTraining, item1.dfeta_Result);
                    Assert.Equal(updateIttSubject1Id.Id, item1.dfeta_Subject1Id.Id);
                    Assert.Equal(updateIttSubject2Id.Id, item1.dfeta_Subject2Id.Id);
                    Assert.Equal(dfeta_ITTQualificationAim.Professionalstatusbyassessmentonly, item1.dfeta_ittqualificationaim);
                }
            );
            Assert.Collection(
                qualifications,
                item1 =>
                {
                    Assert.Equal(updatedHeCountryId.Id, item1.dfeta_HE_CountryId.Id);
                    Assert.Equal(updateHeSubjectId.Id, item1.dfeta_HE_HESubject1Id.Id);
                    Assert.Equal(oldProvider, item1.dfeta_HE_EstablishmentId.Id);
                    Assert.Equal(dfeta_classdivision.Firstclasshonours, item1.dfeta_HE_ClassDivision);
                    Assert.Equal(new DateTime(2022, 01, 28), item1.dfeta_CompletionorAwardDate);
                });

            var crmTask = transactionRequest.AssertSingleCreateRequest<CrmTask>();
            Assert.Equal("Notification for QTS unit - Register: matched record holds active sanction", crmTask.Category);
            Assert.Equal("Register: active sanction match", crmTask.Subject);
        }

        [Fact]
        public async Task Given_update_with_unknown_itt_provider_request_fails()
        {
            // Arrange
            var (teacherId, _) = await CreatePerson(earlyYears: true, hasActiveSanctions: false);

            // Act
            var (result, _) = await _dataverseAdapter.UpdateTeacherImpl(new UpdateTeacherCommand()
            {
                TeacherId = teacherId,
                InitialTeacherTraining = new UpdateTeacherCommandInitialTeacherTraining()
                {
                    ProviderUkprn = "SOME INVALID",
                    ProgrammeStartDate = new DateOnly(2011, 11, 01),
                    ProgrammeEndDate = new DateOnly(2012, 11, 01),
                    ProgrammeType = dfeta_ITTProgrammeType.EYITTAssessmentOnly,
                    Subject1 = "Mathematics",
                    Subject2 = "Computer Science",
                    AgeRangeFrom = dfeta_AgeRange._11,
                    AgeRangeTo = dfeta_AgeRange._12,
                    IttQualificationAim = dfeta_ITTQualificationAim.Academicawardonly,
                },
                Qualification = new UpdateTeacherCommandQualification()
                {
                    CountryCode = "XK",
                    Subject = "100366",  // computer science
                    Class = dfeta_classdivision.Firstclasshonours,
                    Date = new DateOnly(2022, 01, 28),
                    ProviderUkprn = "SOME INVALID"
                }
            });

            // Assert
            Assert.False(result.Succeeded);
        }

        [Theory]
        [InlineData("Invalid Subject1", "100403", "100366", "001", "XK", "100366", "400", UpdateTeacherFailedReasons.Subject1NotFound)]
        [InlineData("100302", "Invalid subject2", "100403", "001", "XK", "100366", "400", UpdateTeacherFailedReasons.Subject2NotFound)]
        [InlineData("100302", "100403", "Invalid subject3", "001", "XK", "100366", "400", UpdateTeacherFailedReasons.Subject3NotFound)]
        [InlineData("100302", "100403", "100366", "001", "XK", "Invalid Qualification subject", "400", UpdateTeacherFailedReasons.QualificationSubjectNotFound)]
        [InlineData("100302", "100403", "100366", "001", "INVALID COUNTRY CODE", "100366", "400", UpdateTeacherFailedReasons.QualificationCountryNotFound)]
        [InlineData("100302", "100403", "100366", "xxx", "XK", "100366", "400", UpdateTeacherFailedReasons.IttQualificationNotFound)]
        [InlineData("100302", "100403", "100366", "001", "XK", "100366", "xxx", UpdateTeacherFailedReasons.QualificationNotFound)]
        public async Task Given_invalid_reference_data_request_fails(
            string ittSubject1,
            string ittSubject2,
            string ittSubject3,
            string ittQualificationCode,
            string qualificationCountryCode,
            string qualificationSubject,
            string heQualificationCode,
            UpdateTeacherFailedReasons expectedFailedReasons)
        {
            // Arrange
            var (teacherId, _) = await CreatePerson(earlyYears: true, hasActiveSanctions: false);

            var newIttProviderUkprn = "10045988";

            // Act
            var (result, _) = await _dataverseAdapter.UpdateTeacherImpl(new UpdateTeacherCommand()
            {
                TeacherId = teacherId,
                InitialTeacherTraining = new UpdateTeacherCommandInitialTeacherTraining()
                {
                    ProviderUkprn = newIttProviderUkprn,
                    ProgrammeStartDate = new DateOnly(2011, 11, 01),
                    ProgrammeEndDate = new DateOnly(2012, 11, 01),
                    ProgrammeType = dfeta_ITTProgrammeType.EYITTAssessmentOnly,
                    Subject1 = ittSubject1,
                    Subject2 = ittSubject2,
                    Subject3 = ittSubject3,
                    AgeRangeFrom = dfeta_AgeRange._11,
                    AgeRangeTo = dfeta_AgeRange._12,
                    IttQualificationValue = ittQualificationCode,
                    IttQualificationAim = dfeta_ITTQualificationAim.Academicawardonly,
                },
                Qualification = new UpdateTeacherCommandQualification()
                {
                    CountryCode = qualificationCountryCode,
                    Subject = qualificationSubject,
                    Class = dfeta_classdivision.Firstclasshonours,
                    Date = new DateOnly(2022, 01, 28),
                    ProviderUkprn = newIttProviderUkprn,
                    HeQualificationValue = heQualificationCode
                }
            });

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal(expectedFailedReasons, result.FailedReasons);
        }

        [Fact]
        public async Task Given_existing_itt_and_qualification_create_new_itt_for_new_provider_and_update_existing_qualification_succeeds()
        {
            // Arrange
            var (teacherId, ittProviderUkprn) = await CreatePerson(earlyYears: true, hasActiveSanctions: false);

            var newIttProviderUkprn = "10045988";
            var updateHeSubjectId = await _dataverseAdapter.GetHeSubjectByCode("100366");  // computer science
            var updatedHeCountryId = await _dataverseAdapter.GetCountry("XQ");
            var updateIttSubject1Id = await _dataverseAdapter.GetIttSubjectByCode("100403");  // mathematics
            var updateIttSubject2Id = await _dataverseAdapter.GetIttSubjectByCode("100366");  // computer science
            var updateIttSubject3Id = await _dataverseAdapter.GetIttSubjectByCode("100302");  // history
            var husId = "1234567890123";

            // Act
            var (result, transactionRequest) = await _dataverseAdapter.UpdateTeacherImpl(new UpdateTeacherCommand()
            {
                TeacherId = teacherId,
                InitialTeacherTraining = new UpdateTeacherCommandInitialTeacherTraining()
                {
                    ProviderUkprn = newIttProviderUkprn,
                    ProgrammeStartDate = new DateOnly(2011, 11, 01),
                    ProgrammeEndDate = new DateOnly(2012, 11, 01),
                    ProgrammeType = dfeta_ITTProgrammeType.EYITTAssessmentOnly,
                    Subject1 = "100403",  // mathematics,
                    Subject2 = "100366",  // computer science
                    Subject3 = "100302",  // history
                    AgeRangeFrom = dfeta_AgeRange._11,
                    AgeRangeTo = dfeta_AgeRange._12,
                    IttQualificationAim = dfeta_ITTQualificationAim.Professionalstatusbyassessmentonly,
                },
                Qualification = new UpdateTeacherCommandQualification()
                {
                    CountryCode = "XQ", //Africa
                    Subject = "100366",  // computer science
                    Class = dfeta_classdivision.Firstclasshonours,
                    Date = new DateOnly(2022, 01, 28),
                    ProviderUkprn = newIttProviderUkprn,
                },
                HusId = husId
            });

            var oldProvider = (await _dataverseAdapter.GetOrganizationsByUkprn(ittProviderUkprn)).Single().Id;
            var newProviderProvider = (await _dataverseAdapter.GetOrganizationsByUkprn(newIttProviderUkprn)).Single().Id;

            var qualifications = await _dataverseAdapter.GetQualificationsForTeacher(
                teacherId,
                columnNames: new[]
                {
                    dfeta_qualification.Fields.dfeta_CompletionorAwardDate,
                    dfeta_qualification.Fields.dfeta_Type,
                    dfeta_qualification.Fields.dfeta_HE_EstablishmentId,
                    dfeta_qualification.Fields.dfeta_PersonId,
                    dfeta_qualification.Fields.dfeta_HE_ClassDivision,
                    dfeta_qualification.Fields.dfeta_HE_CompletionDate,
                    dfeta_qualification.Fields.dfeta_HE_HESubject1Id,
                    dfeta_qualification.Fields.dfeta_HE_CountryId,
                });

            var itt = await _dataverseAdapter.GetInitialTeacherTrainingByTeacher(
                teacherId,
                columnNames: new[]
                {
                    dfeta_initialteachertraining.Fields.dfeta_ProgrammeType,
                    dfeta_initialteachertraining.Fields.dfeta_Result,
                    dfeta_initialteachertraining.Fields.dfeta_EstablishmentId,
                    dfeta_initialteachertraining.Fields.StateCode,
                    dfeta_initialteachertraining.Fields.dfeta_ProgrammeEndDate,
                    dfeta_initialteachertraining.Fields.dfeta_ProgrammeStartDate,
                    dfeta_initialteachertraining.Fields.dfeta_AgeRangeFrom,
                    dfeta_initialteachertraining.Fields.dfeta_AgeRangeTo,
                    dfeta_initialteachertraining.Fields.dfeta_Result,
                    dfeta_initialteachertraining.Fields.dfeta_Subject1Id,
                    dfeta_initialteachertraining.Fields.dfeta_Subject2Id,
                    dfeta_initialteachertraining.Fields.dfeta_Subject3Id,
                    dfeta_initialteachertraining.Fields.dfeta_TraineeID,
                    dfeta_initialteachertraining.Fields.dfeta_ittqualificationaim
                });

            // Assert
            Assert.True(result.Succeeded);

            Assert.Collection(
                itt,
                item1 =>
                {
                    Assert.Equal(oldProvider, item1.dfeta_EstablishmentId.Id);
                },
                item2 =>
                {
                    Assert.Equal(newProviderProvider, item2.dfeta_EstablishmentId.Id);
                    Assert.Equal(dfeta_ITTProgrammeType.EYITTAssessmentOnly, item2.dfeta_ProgrammeType);
                    Assert.Equal(dfeta_AgeRange._11, item2.dfeta_AgeRangeFrom);
                    Assert.Equal(dfeta_AgeRange._12, item2.dfeta_AgeRangeTo);
                    Assert.Equal(updateIttSubject1Id.Id, item2.dfeta_Subject1Id.Id);
                    Assert.Equal(updateIttSubject2Id.Id, item2.dfeta_Subject2Id.Id);
                    Assert.Equal(updateIttSubject3Id.Id, item2.dfeta_Subject3Id.Id);
                    Assert.Equal(husId, item2.dfeta_TraineeID);
                    Assert.Equal(dfeta_ITTQualificationAim.Professionalstatusbyassessmentonly, item2.dfeta_ittqualificationaim);
                }
            );

            Assert.Collection(
                qualifications,
                item1 =>
                {
                    Assert.Equal(updatedHeCountryId.Id, item1.dfeta_HE_CountryId.Id);
                    Assert.Equal(updateHeSubjectId.Id, item1.dfeta_HE_HESubject1Id.Id);
                    Assert.Equal(newProviderProvider, item1.dfeta_HE_EstablishmentId.Id);
                    Assert.Equal(dfeta_classdivision.Firstclasshonours, item1.dfeta_HE_ClassDivision);
                    Assert.Equal(new DateTime(2022, 01, 28), item1.dfeta_CompletionorAwardDate);
                });
        }

        [Fact]
        public async Task Given_update_without_qualification_returns_success()
        {
            // Arrange
            var (teacherId, ittProviderUkprn) = await CreatePerson(earlyYears: false, hasActiveSanctions: false);

            // Act
            var (_, txnRequest) = await _dataverseAdapter.UpdateTeacherImpl(new UpdateTeacherCommand()
            {
                TeacherId = teacherId,
                InitialTeacherTraining = new UpdateTeacherCommandInitialTeacherTraining()
                {
                    ProviderUkprn = ittProviderUkprn,
                    ProgrammeStartDate = new DateOnly(2011, 11, 01),
                    ProgrammeEndDate = new DateOnly(2012, 11, 01),
                    ProgrammeType = dfeta_ITTProgrammeType.RegisteredTeacherProgramme,
                    Subject1 = "100366",  // computer science
                    Subject2 = "100403",  // mathematics
                    Subject3 = "100302",  // history
                    AgeRangeFrom = dfeta_AgeRange._11,
                    AgeRangeTo = dfeta_AgeRange._12,
                    IttQualificationAim = dfeta_ITTQualificationAim.Professionalstatusbyassessmentonly
                },
                Qualification = null
            });

            // Assert
            txnRequest.AssertDoesNotContainCreateRequest<dfeta_hequalification>();
            txnRequest.AssertDoesNotContainUpdateRequest<dfeta_hequalification>();
            txnRequest.AssertDoesNotContainUpsertRequest<dfeta_hequalification>();
        }

        [Fact]
        public async Task Given_update_with_changed_husid_updates_contact()
        {
            // Arrange
            var (teacherId, ittProviderUkprn) = await CreatePerson(earlyYears: false, hasActiveSanctions: false);
            var husId = "1234567890123";

            // Act
            var (_, txnRequest) = await _dataverseAdapter.UpdateTeacherImpl(new UpdateTeacherCommand()
            {
                TeacherId = teacherId,
                InitialTeacherTraining = new UpdateTeacherCommandInitialTeacherTraining()
                {
                    ProviderUkprn = ittProviderUkprn,
                    ProgrammeStartDate = new DateOnly(2011, 11, 01),
                    ProgrammeEndDate = new DateOnly(2012, 11, 01),
                    ProgrammeType = dfeta_ITTProgrammeType.RegisteredTeacherProgramme,
                    Subject1 = "100366",  // computer science
                    Subject2 = "100403",  // mathematics
                    Subject3 = "100302",  // history
                    AgeRangeFrom = dfeta_AgeRange._11,
                    AgeRangeTo = dfeta_AgeRange._12,
                    IttQualificationValue = "001",  // BEd,
                    IttQualificationAim = dfeta_ITTQualificationAim.Professionalstatusbyassessmentonly
                },
                Qualification = null,
                HusId = husId
            });

            // Assert
            var contact = txnRequest.AssertSingleUpdateRequest<Contact>();
            Assert.Equal(husId, contact.dfeta_HUSID);
        }

        private async Task<(Guid TeacherId, string IttProviderUkprn)> CreatePerson(
            bool earlyYears,
            bool assessmentOnly = false,
            bool hasActiveSanctions = false)
        {
            var createPersonResult = await _testDataHelper.CreatePerson(
                earlyYears,
                assessmentOnly,
                withQualification: true,
                withActiveSanction: hasActiveSanctions);

            return (createPersonResult.TeacherId, createPersonResult.IttProviderUkprn);
        }
    }
}
