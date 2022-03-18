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
    [Collection(nameof(ExclusiveCrmTestCollection))]
    public class UpdateTeacherTests : IAsyncLifetime
    {
        private readonly CrmClientFixture.TestDataScope _dataScope;
        private readonly DataverseAdapter _dataverseAdapter;
        private readonly IOrganizationServiceAsync _organizationService;

        public UpdateTeacherTests(CrmClientFixture crmClientFixture)
        {
            _dataScope = crmClientFixture.CreateTestDataScope();
            _dataverseAdapter = _dataScope.CreateDataverseAdapter();
            _organizationService = _dataScope.OrganizationService;
        }

        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync() => await _dataScope.DisposeAsync();

        [Fact]
        public async Task Given_update_without_providing_qualification_ukprn_returns_success()
        {
            // Arrange
            var (teacherId, _,
                ittProviderUkprn, _, _,
                _) = await CreatePerson(earlyYears: false, hasActiveSanctions: false);

            var updateHeSubjectId = await _dataverseAdapter.GetHeSubjectByCode("100366");  // computer science
            var updatedHeCountryId = await _dataverseAdapter.GetCountry("XK");
            var updateIttSubject1Id = await _dataverseAdapter.GetIttSubjectByCode("100403");  // mathematics
            var updateIttSubject2Id = await _dataverseAdapter.GetIttSubjectByCode("100366");  // computer science

            // Act
            var (result, transactionRequest) = await _dataverseAdapter.UpdateTeacherImpl(new UpdateTeacherCommand()
            {
                TeacherId = teacherId.ToString(),
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
        }

        [Fact]
        public async Task Given_existing_itt_update_programmetype_from_qts_to_another_qts_programmetype_succeeds()
        {
            // Arrange
            var (teacherId, _,
                ittProviderUkprn, _, _,
                _) = await CreatePerson(earlyYears: false, hasActiveSanctions: false);

            var updateHeSubjectId = await _dataverseAdapter.GetHeSubjectByCode("100366");  // computer science
            var updatedHeCountryId = await _dataverseAdapter.GetCountry("XK");
            var updateIttSubject1Id = await _dataverseAdapter.GetIttSubjectByCode("100403");  // mathematics
            var updateIttSubject2Id = await _dataverseAdapter.GetIttSubjectByCode("100366");  // computer science

            // Act
            var (result, transactionRequest) = await _dataverseAdapter.UpdateTeacherImpl(new UpdateTeacherCommand()
            {
                TeacherId = teacherId.ToString(),
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

            var oldProvider = (await _dataverseAdapter.GetOrganizationByUkprn(ittProviderUkprn)).Id;
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
            Assert.Collection(itt,
                item1 =>
                {
                    Assert.Equal(dfeta_ITTProgrammeType.RegisteredTeacherProgramme, item1.dfeta_ProgrammeType);
                }
            );
        }

        [Fact]
        public async Task Given_existing_itt_update_programmetype_from_eyts_to_another_eyts_programmetype_succeeds()
        {
            // Arrange
            var (teacherId, _,
                ittProviderUkprn, _, _,
                _) = await CreatePerson(earlyYears: true, hasActiveSanctions: false);

            var updateHeSubjectId = await _dataverseAdapter.GetHeSubjectByCode("100366");  // computer science
            var updatedHeCountryId = await _dataverseAdapter.GetCountry("XK");
            var updateIttSubject1Id = await _dataverseAdapter.GetIttSubjectByCode("100403");  // mathematics
            var updateIttSubject2Id = await _dataverseAdapter.GetIttSubjectByCode("100366");  // computer science

            // Act
            var (result, transactionRequest) = await _dataverseAdapter.UpdateTeacherImpl(new UpdateTeacherCommand()
            {
                TeacherId = teacherId.ToString(),
                InitialTeacherTraining = new UpdateTeacherCommandInitialTeacherTraining()
                {
                    ProviderUkprn = ittProviderUkprn,
                    ProgrammeStartDate = new DateOnly(2011, 11, 01),
                    ProgrammeEndDate = new DateOnly(2012, 11, 01),
                    ProgrammeType = dfeta_ITTProgrammeType.EYITTGraduateEmploymentBased,
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

            var oldProvider = (await _dataverseAdapter.GetOrganizationByUkprn(ittProviderUkprn)).Id;
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
            Assert.Collection(itt,
                item1 =>
                {
                    Assert.Equal(dfeta_ITTProgrammeType.EYITTGraduateEmploymentBased, item1.dfeta_ProgrammeType);
                }
            );
        }

        [Fact]
        public async Task Given_updating_existing_contact_update_without_subject3_returns_success()
        {
            // Arrange
            var (teacherId, _,
                ittProviderUkprn, _, _,
                _) = await CreatePerson(earlyYears: true, hasActiveSanctions: false);

            var updateHeSubjectId = await _dataverseAdapter.GetHeSubjectByCode("100366");  // computer science
            var updatedHeCountryId = await _dataverseAdapter.GetCountry("XK");
            var updateIttSubject1Id = await _dataverseAdapter.GetIttSubjectByCode("100403");  // mathematics
            var updateIttSubject2Id = await _dataverseAdapter.GetIttSubjectByCode("100366");  // computer science

            // Act
            var (result, transactionRequest) = await _dataverseAdapter.UpdateTeacherImpl(new UpdateTeacherCommand()
            {
                TeacherId = teacherId.ToString(),
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

            var oldProvider = (await _dataverseAdapter.GetOrganizationByUkprn(ittProviderUkprn)).Id;
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
            Assert.Collection(itt,
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
                }
            );
        }

        [Fact]
        public async Task Given_existing_contact_update_itt_and_qualification_returns_success()
        {
            // Arrange
            var (teacherId, _,
                ittProviderUkprn, _, _,
                _) = await CreatePerson(earlyYears: true, hasActiveSanctions: false);

            var updateHeSubjectId = await _dataverseAdapter.GetHeSubjectByCode("100366");  // computer science
            var updatedHeCountryId = await _dataverseAdapter.GetCountry("XK");
            var updateIttSubject1Id = await _dataverseAdapter.GetIttSubjectByCode("100403");  // mathematics
            var updateIttSubject2Id = await _dataverseAdapter.GetIttSubjectByCode("100366");  // computer science
            var updateIttSubject3Id = await _dataverseAdapter.GetIttSubjectByCode("100302");  // history

            // Act
            var (result, transactionRequest) = await _dataverseAdapter.UpdateTeacherImpl(new UpdateTeacherCommand()
            {
                TeacherId = teacherId.ToString(),
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

            var oldProvider = (await _dataverseAdapter.GetOrganizationByUkprn(ittProviderUkprn)).Id;
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
            Assert.Collection(itt,
                item1 =>
                {
                    Assert.Equal(oldProvider, item1.dfeta_EstablishmentId.Id);
                    Assert.Equal(dfeta_ITTProgrammeType.EYITTAssessmentOnly, item1.dfeta_ProgrammeType);
                    Assert.Equal(dfeta_AgeRange._11, item1.dfeta_AgeRangeFrom);
                    Assert.Equal(dfeta_AgeRange._12, item1.dfeta_AgeRangeTo);
                    Assert.Equal(dfeta_ITTResult.InTraining, item1.dfeta_Result);
                    Assert.Equal(updateIttSubject1Id.Id, item1.dfeta_Subject1Id.Id);
                    Assert.Equal(updateIttSubject2Id.Id, item1.dfeta_Subject2Id.Id);
                    Assert.Equal(updateIttSubject3Id.Id, item1.dfeta_Subject3Id.Id);
                }
            );
            Assert.Collection(qualifications,
                item1 =>
                {
                    Assert.Equal(updatedHeCountryId.Id, item1.dfeta_HE_CountryId.Id);
                    Assert.Equal(updateHeSubjectId.Id, item1.dfeta_HE_HESubject1Id.Id);
                    Assert.Equal(oldProvider, item1.dfeta_HE_EstablishmentId.Id);
                    Assert.Equal(dfeta_classdivision.Firstclasshonours, item1.dfeta_HE_ClassDivision);
                    Assert.Equal(new DateTime(2022, 01, 28), item1.dfeta_CompletionorAwardDate);
                });
        }

        [Fact]
        public async Task Given_update_itt_and_qualification_with_noactive_sanctions_does_not_create_crm_task_and_returns_success_w()
        {
            // Arrange
            var (teacherId, _,
                ittProviderUkprn, _, _,
                _) = await CreatePerson(earlyYears: true, hasActiveSanctions: false);

            // Act
            var (result, transactionRequest) = await _dataverseAdapter.UpdateTeacherImpl(new UpdateTeacherCommand()
            {
                TeacherId = teacherId.ToString(),
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

        [Fact]
        public async Task Given_qts_itt_cannot_change_qts_programmetype_to_eyts_programmetype()
        {
            // Arrange
            var (teacherId, _,
                ittProviderUkprn, _, _,
                _) = await CreatePerson(earlyYears: false, hasActiveSanctions: false);

            // Act
            var (result, _) = await _dataverseAdapter.UpdateTeacherImpl(new UpdateTeacherCommand()
            {
                TeacherId = teacherId.ToString(),
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
            Assert.False(result.Succeeded);
            Assert.Equal(UpdateTeacherFailedReasons.CannotChangeProgrammeType, result.FailedReasons);
        }

        [Fact]
        public async Task Given_earlyyears_itt_cannot_change_eyts_programmetype_to_qts()
        {
            // Arrange
            var (teacherId, _,
                ittProviderUkprn, _, _,
                _) = await CreatePerson(earlyYears: true, hasActiveSanctions: false);

            // Act
            var (result, _) = await _dataverseAdapter.UpdateTeacherImpl(new UpdateTeacherCommand()
            {
                TeacherId = teacherId.ToString(),
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

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal(UpdateTeacherFailedReasons.CannotChangeProgrammeType, result.FailedReasons);
        }

        [Fact]
        public async Task Given_two_or_more_qualifications_create_new_qualification_and_warning_crm_task()
        {
            // Arrange
            var (teacherId, _,
                ittProviderUkprn, _, _,
                _) = await CreatePerson(earlyYears: true, hasActiveSanctions: false);

            var countryId = await _dataverseAdapter.GetCountry("XK");
            var qualificationSubject1Id = await _dataverseAdapter.GetHeSubjectByCode("100366");  // computer science
            var providerId = await _dataverseAdapter.GetOrganizationByUkprn(ittProviderUkprn);
            var qualification = await _dataverseAdapter.GetHeQualificationByName("First Degree");

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
                TeacherId = teacherId.ToString(),
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
            var crmTask = transactionRequest.AssertSingleCreateRequest<CrmTask>();
            Assert.Equal("More than one qualification record found", crmTask.Description);
            Assert.Equal("Notification for QTS unit - Register: matched record holds multiple qualifications", crmTask.Category);
            Assert.Equal("Register: multiple qualifications", crmTask.Subject);

            Assert.Collection(qualifications,
                item1 =>
                {
                    Assert.Equal(providerId.Id, item1.dfeta_HE_EstablishmentId.Id);
                },
                item2 =>
                {
                    Assert.Equal(providerId.Id, item2.dfeta_HE_EstablishmentId.Id);
                },
                item3 =>
                {
                    Assert.Equal(providerId.Id, item3.dfeta_HE_EstablishmentId.Id);
                    Assert.Equal(dfeta_classdivision.Fourthclasshonours, item3.dfeta_HE_ClassDivision);
                    Assert.Equal(new DateTime(2022, 01, 15), item3.dfeta_CompletionorAwardDate);
                });
        }

        [Fact]
        public async Task Given_there_are_no_records_matched_create_new_itt_and_warning_crm_task()
        {
            // Arrange
            var (teacherId, _,
                ittProviderUkprn, _, _,
                _) = await CreatePerson(earlyYears: true, hasActiveSanctions: false);

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
            var entity = existingItt.FirstOrDefault();
            await _organizationService.ExecuteAsync(new ExecuteTransactionRequest()
            {
                Requests = new()
                {
                    new SetStateRequest()
                    {
                        EntityMoniker = new EntityReference(dfeta_initialteachertraining.EntityLogicalName, entity.Id),
                        State = new OptionSetValue(1),  // Inactive
                        Status = new OptionSetValue(2)
                    }
                },
                ReturnResponses = true
            });

            // Act
            var (result, transactionRequest) = await _dataverseAdapter.UpdateTeacherImpl(new UpdateTeacherCommand()
            {
                TeacherId = teacherId.ToString(),
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
                    ProviderUkprn = ittProviderUkprn,
                }
            });

            var crmTask = transactionRequest.AssertSingleCreateRequest<CrmTask>();
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
                });

            // Assert
            Assert.True(result.Succeeded);
            Assert.Equal($"No ITT UKPRN match for TRN {teacherId}", crmTask.Description);
            Assert.Equal($"Register: missing ITT UKPRN", crmTask.Subject);
            Assert.Equal($"Notification for QTS unit - Register: matched record holds no ITT UKPRN", crmTask.Category);
        }

        [Fact]
        public async Task Given_two_or_more_itt_earlyyears_records_match_create_new_itt_and_warning_crm_task()
        {
            // Arrange
            var (teacherId, _,
                ittProviderUkprn, _, _,
                _) = await CreatePerson(earlyYears: true, hasActiveSanctions: false);

            var providerId = await _dataverseAdapter.GetOrganizationByUkprn(ittProviderUkprn);

            // Create second Itt record
            await _organizationService.ExecuteAsync(new ExecuteTransactionRequest()
            {
                Requests = new()
                {
                    new CreateRequest()
                    {
                        Target = new dfeta_initialteachertraining()
                        {
                            dfeta_PersonId = new EntityReference(Contact.EntityLogicalName, teacherId),
                            dfeta_EstablishmentId = new EntityReference(Account.EntityLogicalName, providerId.Id),
                            dfeta_ProgrammeType = dfeta_ITTProgrammeType.EYITTAssessmentOnly,
                            dfeta_Result = dfeta_ITTResult.InTraining,
                            dfeta_AgeRangeFrom = dfeta_AgeRange._00,
                            dfeta_AgeRangeTo = dfeta_AgeRange._18
                        }
                    }
                },
                ReturnResponses = true
            });

            // Act
            var (result, transactionRequest) = await _dataverseAdapter.UpdateTeacherImpl(new UpdateTeacherCommand()
            {
                TeacherId = teacherId.ToString(),
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
                    ProviderUkprn = ittProviderUkprn,
                }
            });

            var crmTask = transactionRequest.AssertSingleCreateRequest<CrmTask>();
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
            Assert.Equal($"Multiple ITT UKPRNs found for TRN {teacherId}", crmTask.Description);
            Assert.Equal("Notification for QTS unit - Register: matched record holds multiple ITT UKPRNs", crmTask.Category);
            Assert.Equal("Register: multiple ITT UKPRNs", crmTask.Subject);
        }

        [Fact]
        public async Task Given_existing_contact_update_itt_and_qualification_with_existing_active_sanction_returns_success_and_creates_crm_task()
        {
            // Arrange
            var (teacherId, _,
                ittProviderUkprn, _, _,
                _) = await CreatePerson(earlyYears: true, hasActiveSanctions: true);

            var updateHeSubjectId = await _dataverseAdapter.GetHeSubjectByCode("100366");  // computer science
            var updatedHeCountryId = await _dataverseAdapter.GetCountry("XK");
            var updateIttSubject1Id = await _dataverseAdapter.GetIttSubjectByCode("100403");  // mathematics
            var updateIttSubject2Id = await _dataverseAdapter.GetIttSubjectByCode("100366");  // computer science

            // Act
            var (result, transactionRequest) = await _dataverseAdapter.UpdateTeacherImpl(new UpdateTeacherCommand()
            {
                TeacherId = teacherId.ToString(),
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

            var oldProvider = (await _dataverseAdapter.GetOrganizationByUkprn(ittProviderUkprn)).Id;
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
                });

            // Assert
            Assert.True(result.Succeeded);
            Assert.Collection(itt,
                item1 =>
                {
                    Assert.Equal(oldProvider, item1.dfeta_EstablishmentId.Id);
                    Assert.Equal(dfeta_ITTProgrammeType.EYITTAssessmentOnly, item1.dfeta_ProgrammeType);
                    Assert.Equal(dfeta_AgeRange._11, item1.dfeta_AgeRangeFrom);
                    Assert.Equal(dfeta_AgeRange._12, item1.dfeta_AgeRangeTo);
                    Assert.Equal(dfeta_ITTResult.InTraining, item1.dfeta_Result);
                    Assert.Equal(updateIttSubject1Id.Id, item1.dfeta_Subject1Id.Id);
                    Assert.Equal(updateIttSubject2Id.Id, item1.dfeta_Subject2Id.Id);
                }
            );
            Assert.Collection(qualifications,
                item1 =>
                {
                    Assert.Equal(updatedHeCountryId.Id, item1.dfeta_HE_CountryId.Id);
                    Assert.Equal(updateHeSubjectId.Id, item1.dfeta_HE_HESubject1Id.Id);
                    Assert.Equal(oldProvider, item1.dfeta_HE_EstablishmentId.Id);
                    Assert.Equal(dfeta_classdivision.Firstclasshonours, item1.dfeta_HE_ClassDivision);
                    Assert.Equal(new DateTime(2022, 01, 28), item1.dfeta_CompletionorAwardDate);
                });

            var crmTask = transactionRequest.AssertSingleCreateRequest<CrmTask>();
            var dob = DateOnly.FromDateTime(new DateTime(1990, 4, 1));

            // Assert
            Assert.Equal($"Active sanction found: TRN {teacherId}", crmTask.Description);
            Assert.Equal("Notification for QTS unit - Register: matched record holds active sanction", crmTask.Category);
            Assert.Equal("Register: active sanction match", crmTask.Subject);
        }

        [Fact]
        public async Task Given_update_with_unknown_itt_provider_request_fails()
        {
            // Arrange
            var (teacherId, _,
                _, _, _,
                _) = await CreatePerson(earlyYears: true, hasActiveSanctions: false);

            // Act
            var (result, _) = await _dataverseAdapter.UpdateTeacherImpl(new UpdateTeacherCommand()
            {
                TeacherId = teacherId.ToString(),
                InitialTeacherTraining = new UpdateTeacherCommandInitialTeacherTraining()
                {
                    ProviderUkprn = "SOME INVALID",
                    ProgrammeStartDate = new DateOnly(2011, 11, 01),
                    ProgrammeEndDate = new DateOnly(2012, 11, 01),
                    ProgrammeType = dfeta_ITTProgrammeType.EYITTAssessmentOnly,
                    Subject1 = "Mathematics",
                    Subject2 = "Computer Science",
                    AgeRangeFrom = dfeta_AgeRange._11,
                    AgeRangeTo = dfeta_AgeRange._12
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
        [InlineData("Invalid Subject1", "100403", "100366", "XK", "Computer Science")]
        [InlineData("100302", "Invalid subject2", "100403", "XK", "Computer Science")]
        [InlineData("100302", "100403", "100366", "INVALID COUNTRY CODE", "Computer Science")]
        [InlineData("100302", "100403", "100366", "XK", "Invalid Qualification subject")]
        [InlineData("100302", "100403", "Invalid subject3", "XK", "Computer Science")]
        public async Task Given_Invalid_reference_data_request_fails(string ittSubject1, string ittSubject2, string ittSubject3, string qualificationCountryCode, string qualificationSubject)
        {
            // Arrange
            var (teacherId, _,
                _, _, _,
                _) = await CreatePerson(earlyYears: true, hasActiveSanctions: false);

            var newIttProviderUkprn = "10045988";

            // Act
            var (result, _) = await _dataverseAdapter.UpdateTeacherImpl(new UpdateTeacherCommand()
            {
                TeacherId = teacherId.ToString(),
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
                    AgeRangeTo = dfeta_AgeRange._12
                },
                Qualification = new UpdateTeacherCommandQualification()
                {
                    CountryCode = qualificationCountryCode,
                    Subject = qualificationSubject,
                    Class = dfeta_classdivision.Firstclasshonours,
                    Date = new DateOnly(2022, 01, 28),
                    ProviderUkprn = newIttProviderUkprn,
                }
            });

            // Assert
            Assert.False(result.Succeeded);
        }

        [Fact]
        public async Task Given_existing_itt_and_qualification_create_new_itt_for_new_provider_and_update_exisitng_qualification_succeeds()
        {
            // Arrange
            var (teacherId, _,
                ittProviderUkprn, _, _,
                _) = await CreatePerson(earlyYears: true, hasActiveSanctions: false);

            var newIttProviderUkprn = "10045988";
            var updateHeSubjectId = await _dataverseAdapter.GetHeSubjectByCode("100366");  // computer science
            var updatedHeCountryId = await _dataverseAdapter.GetCountry("XQ");
            var updateIttSubject1Id = await _dataverseAdapter.GetIttSubjectByCode("100403");  // mathematics
            var updateIttSubject2Id = await _dataverseAdapter.GetIttSubjectByCode("100366");  // computer science
            var updateIttSubject3Id = await _dataverseAdapter.GetIttSubjectByCode("100302");  // history

            // Act
            var (result, transactionRequest) = await _dataverseAdapter.UpdateTeacherImpl(new UpdateTeacherCommand()
            {
                TeacherId = teacherId.ToString(),
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
                    AgeRangeTo = dfeta_AgeRange._12
                },
                Qualification = new UpdateTeacherCommandQualification()
                {
                    CountryCode = "XQ", //Africa
                    Subject = "100366",  // computer science
                    Class = dfeta_classdivision.Firstclasshonours,
                    Date = new DateOnly(2022, 01, 28),
                    ProviderUkprn = newIttProviderUkprn,

                }
            });

            var oldProvider = (await _dataverseAdapter.GetOrganizationByUkprn(ittProviderUkprn)).Id;
            var newProviderProvider = (await _dataverseAdapter.GetOrganizationByUkprn(newIttProviderUkprn)).Id;
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
                });

            // Assert
            Assert.True(result.Succeeded);
            Assert.Collection(itt,
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
                }
            );
            Assert.Collection(qualifications,
                item1 =>
                {
                    Assert.Equal(updatedHeCountryId.Id, item1.dfeta_HE_CountryId.Id);
                    Assert.Equal(updateHeSubjectId.Id, item1.dfeta_HE_HESubject1Id.Id);
                    Assert.Equal(newProviderProvider, item1.dfeta_HE_EstablishmentId.Id);
                    Assert.Equal(dfeta_classdivision.Firstclasshonours, item1.dfeta_HE_ClassDivision);
                    Assert.Equal(new DateTime(2022, 01, 28), item1.dfeta_CompletionorAwardDate);
                });
        }

        private async Task<(Guid TeacherId, Guid IttId, string IttProviderUkprn, Guid qualificationId, Guid? sanctionId, string qualificationProviderUkprn)> CreatePerson(
            bool earlyYears,
            bool assessmentOnly = false,
            bool hasActiveSanctions = false)
        {
            var sanctionId = default(Guid?);
            var teacherId = Guid.NewGuid();

            var programmeType = earlyYears ? dfeta_ITTProgrammeType.EYITTAssessmentOnly :
                    assessmentOnly ? dfeta_ITTProgrammeType.AssessmentOnlyRoute :
                    dfeta_ITTProgrammeType.RegisteredTeacherProgramme;

            var ittProviderUkprn = "10044534";

            var ittProviderId = (await _dataverseAdapter.GetOrganizationByUkprn(ittProviderUkprn)).Id;

            var earlyYearsStatusId = earlyYears ?
                (await _dataverseAdapter.GetEarlyYearsStatus("220")).Id : // 220 == 'Early Years Trainee'
                (Guid?)null;

            var teacherStatusId = !earlyYears ?
                (await _dataverseAdapter.GetTeacherStatus(assessmentOnly ? "212" : "211", qtsDateRequired: false)).Id :  // 212 == 'AOR Candidate', 211 == 'Trainee Teacher:DMS'
                (Guid?)null;

            var countryCodeId = (await _dataverseAdapter.GetCountry("XK")).Id;
            var subject = (await _dataverseAdapter.GetHeSubjectByCode("100366"));  // computer science
            var qualification = (await _dataverseAdapter.GetHeQualificationByName("First Degree"));

            var txnResponse = (ExecuteTransactionResponse)await _organizationService.ExecuteAsync(new ExecuteTransactionRequest()
            {
                Requests = new()
                {
                    new CreateRequest()
                    {
                        Target = new Contact()
                        {
                            Id = teacherId,
                            BirthDate = new DateTime(1990, 4, 1)
                        }
                    },
                    new UpdateRequest()
                    {
                        Target = new Contact()
                        {
                            Id = teacherId,
                            dfeta_TRNAllocateRequest = DateTime.UtcNow
                        }
                    },
                    new CreateRequest()
                    {
                        Target = new dfeta_initialteachertraining()
                        {
                            dfeta_PersonId = new EntityReference(Contact.EntityLogicalName, teacherId),
                            dfeta_EstablishmentId = new EntityReference(Account.EntityLogicalName, ittProviderId),
                            dfeta_ProgrammeType = programmeType,
                            dfeta_Result = assessmentOnly ? dfeta_ITTResult.UnderAssessment : dfeta_ITTResult.InTraining,
                            dfeta_AgeRangeFrom = dfeta_AgeRange._00,
                            dfeta_AgeRangeTo = dfeta_AgeRange._18
                        }
                    },
                    new CreateRequest()
                    {
                        Target = new dfeta_qtsregistration()
                        {
                            dfeta_PersonId = new EntityReference(Contact.EntityLogicalName, teacherId),
                            dfeta_EarlyYearsStatusId = earlyYearsStatusId.HasValue ? new EntityReference(dfeta_earlyyearsstatus.EntityLogicalName, earlyYearsStatusId.Value) : null,
                            dfeta_TeacherStatusId = teacherStatusId.HasValue ? new EntityReference(dfeta_teacherstatus.EntityLogicalName, teacherStatusId.Value) : null
                        }
                    },
                    new CreateRequest()
                    {
                        Target = new dfeta_qualification()
                        {
                            dfeta_PersonId = new EntityReference(Contact.EntityLogicalName, teacherId),
                            dfeta_HE_CountryId = new EntityReference(dfeta_qualification.EntityLogicalName, countryCodeId),
                            dfeta_HE_EstablishmentId = new EntityReference(Account.EntityLogicalName, ittProviderId),
                            dfeta_Type = dfeta_qualification_dfeta_Type.HigherEducation,
                            dfeta_HE_ClassDivision = dfeta_classdivision.Pass,
                            dfeta_HE_CompletionDate = DateTime.Now.AddMonths(-1),
                            dfeta_HE_HESubject1Id = new EntityReference(dfeta_hesubject.EntityLogicalName, subject.Id),
                            dfeta_HE_HEQualificationId = new EntityReference(dfeta_hequalification.EntityLogicalName, qualification.Id)
                        }
                    }
                },
                ReturnResponses = true
            });

            if (hasActiveSanctions)
            {
                var sanctionsResponse = (ExecuteTransactionResponse)await _organizationService.ExecuteAsync(new ExecuteTransactionRequest()
                {
                    Requests = new()
                    {
                        new CreateRequest()
                        {
                            Target = new dfeta_sanction()
                            {
                                dfeta_PersonId = new EntityReference(Contact.EntityLogicalName, teacherId),
                            },
                        }
                    },
                    ReturnResponses = true
                });

                sanctionId = ((CreateResponse)sanctionsResponse.Responses[0]).id;
            }

            var ittId = ((CreateResponse)txnResponse.Responses[2]).id;
            var qtsId = ((CreateResponse)txnResponse.Responses[3]).id;
            var qualificationId = ((CreateResponse)txnResponse.Responses[4]).id;
            return (teacherId, ittId, ittProviderUkprn, qualificationId, sanctionId, ittProviderUkprn);
        }
    }
}
