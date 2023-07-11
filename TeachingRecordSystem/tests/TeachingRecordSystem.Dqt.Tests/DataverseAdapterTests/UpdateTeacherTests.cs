#nullable disable
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;

namespace TeachingRecordSystem.Dqt.Tests.DataverseAdapterTests;

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
        var (teacherId, ittProviderUkprn, _) = await CreatePerson(earlyYears: false, hasActiveSanctions: false);

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
                Subject2 = "H6601", //Radio Technology
                Subject3 = "V1030" //Regional History
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
        var (teacherId, ittProviderUkprn, _) = await CreatePerson(earlyYears: false, hasActiveSanctions: false);

        var updateHeSubjectId = await _dataverseAdapter.GetHeSubjectByCode("100366");  // computer science
        var updateHeSubject2Id = await _dataverseAdapter.GetHeSubjectByCode("X300");  // Academic Studies in Education
        var updateHeSubject3Id = await _dataverseAdapter.GetHeSubjectByCode("N400");  // Accounting
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
                Subject2 = "X300",
                Subject3 = "N400"
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
        var (teacherId, ittProviderUkprn, _) = await CreatePerson(earlyYears: true, hasActiveSanctions: false);

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
        var (teacherId, ittProviderUkprn, _) = await CreatePerson(earlyYears: true, hasActiveSanctions: false);

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

    [Theory]
    [InlineData(dfeta_ITTResult.Pass)]
    [InlineData(dfeta_ITTResult.Fail)]
    [InlineData(dfeta_ITTResult.Deferred)]
    [InlineData(dfeta_ITTResult.InTraining)]
    [InlineData(dfeta_ITTResult.UnderAssessment)]
    [InlineData(dfeta_ITTResult.Withdrawn)]
    public async Task Given_passed_eyts_itt_record_cannot_change_result_to(dfeta_ITTResult ittResult)
    {
        // Arrange
        var allStatuses = await _dataverseAdapter.GetAllEarlyYearsStatuses(null);
        var eytsStatus = allStatuses.SingleOrDefault(x => x.dfeta_Value == "220");
        var (teacherId, ittProviderUkprn, _) = await CreatePerson(earlyYears: true, hasActiveSanctions: false);
        var assessmentDate = new DateOnly(2023, 03, 24);
        var updateIttQualification = await _dataverseAdapter.GetIttQualificationByCode("001");  // BEd
        var updateHeQualification = await _dataverseAdapter.GetHeQualificationByCode("401");  // Higher Degree
        var (passResult, _) = await _dataverseAdapter.SetIttResultForTeacherImpl(
            teacherId,
            ittProviderUkprn,
            dfeta_ITTResult.Pass,
            assessmentDate);

        // Act
        var (result, _) = await _dataverseAdapter.UpdateTeacherImpl(new UpdateTeacherCommand()
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
                IttQualificationValue = updateIttQualification.dfeta_Value,
                Result = ittResult
            },
            Qualification = new UpdateTeacherCommandQualification()
            {
                CountryCode = "XK",
                Subject = "100366",  // computer science
                Class = dfeta_classdivision.Firstclasshonours,
                Date = new DateOnly(2022, 01, 28),
                ProviderUkprn = ittProviderUkprn,
                HeQualificationValue = updateHeQualification.dfeta_Value,
                Subject2 = "X300",
                Subject3 = "N400"
            }
        });

        // Assert
        Assert.True(passResult.Succeeded);
        Assert.False(result.Succeeded);
        Assert.Equal(UpdateTeacherFailedReasons.AlreadyHaveEytsDate, result.FailedReasons);
    }

    [Theory]
    [InlineData(dfeta_ITTResult.Pass)]
    [InlineData(dfeta_ITTResult.Fail)]
    [InlineData(dfeta_ITTResult.Withdrawn)]
    public async Task Given_intraining_eyts_itt_record_cannot_passing_result_returns_error(dfeta_ITTResult ittResult)
    {
        // Arrange
        var allStatuses = await _dataverseAdapter.GetAllEarlyYearsStatuses(null);
        var eytsStatus = allStatuses.SingleOrDefault(x => x.dfeta_Value == "220");
        var (teacherId, ittProviderUkprn, _) = await CreatePerson(earlyYears: true, hasActiveSanctions: false);
        var updateIttQualification = await _dataverseAdapter.GetIttQualificationByCode("001");  // BEd
        var updateHeQualification = await _dataverseAdapter.GetHeQualificationByCode("401");  // Higher Degree

        // Act
        var ex = await Assert.ThrowsAsync<ArgumentException>(async () => await _dataverseAdapter.UpdateTeacherImpl(new UpdateTeacherCommand()
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
                IttQualificationValue = updateIttQualification.dfeta_Value,
                Result = ittResult
            },
            Qualification = new UpdateTeacherCommandQualification()
            {
                CountryCode = "XK",
                Subject = "100366",  // computer science
                Class = dfeta_classdivision.Firstclasshonours,
                Date = new DateOnly(2022, 01, 28),
                ProviderUkprn = ittProviderUkprn,
                HeQualificationValue = updateHeQualification.dfeta_Value,
                Subject2 = "X300",
                Subject3 = "N400"
            }
        }));

        // Assert
        Assert.NotNull(ex);
        Assert.Contains($"Invalid ITT outcome: '{ittResult}'", ex.Message);
    }

    [Theory]
    [InlineData(dfeta_ITTResult.Deferred)]
    [InlineData(dfeta_ITTResult.InTraining)]
    public async Task Given_intraining_eyts_itt_record_cannot_change_result_to(dfeta_ITTResult ittResult)
    {
        // Arrange
        var allStatuses = await _dataverseAdapter.GetAllEarlyYearsStatuses(null);
        var eytsStatus = allStatuses.SingleOrDefault(x => x.dfeta_Value == "220");
        var (teacherId, ittProviderUkprn, _) = await CreatePerson(earlyYears: true, hasActiveSanctions: false);
        var updateIttQualification = await _dataverseAdapter.GetIttQualificationByCode("001");  // BEd
        var updateHeQualification = await _dataverseAdapter.GetHeQualificationByCode("401");  // Higher Degree

        // Act
        var (result, _) = await _dataverseAdapter.UpdateTeacherImpl(new UpdateTeacherCommand()
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
                IttQualificationValue = updateIttQualification.dfeta_Value,
                Result = ittResult
            },
            Qualification = new UpdateTeacherCommandQualification()
            {
                CountryCode = "XK",
                Subject = "100366",  // computer science
                Class = dfeta_classdivision.Firstclasshonours,
                Date = new DateOnly(2022, 01, 28),
                ProviderUkprn = ittProviderUkprn,
                HeQualificationValue = updateHeQualification.dfeta_Value,
                Subject2 = "X300",
                Subject3 = "N400"
            }
        });

        // Assert
        Assert.True(result.Succeeded);
    }

    [Theory]
    [InlineData(true, false, dfeta_ITTProgrammeType.EYITTAssessmentOnly, dfeta_ITTResult.Withdrawn)]  //eyts
    [InlineData(false, true, dfeta_ITTProgrammeType.AssessmentOnlyRoute, dfeta_ITTResult.Pass)] //aor
    [InlineData(false, false, dfeta_ITTProgrammeType.RegisteredTeacherProgramme, dfeta_ITTResult.Fail)] //qts
    public async Task Given_itt_record_passing_result_returns_error(bool eyts, bool asessmentonlyroute, dfeta_ITTProgrammeType programmetype, dfeta_ITTResult ittResult)
    {
        // Arrange
        var allStatuses = await _dataverseAdapter.GetAllEarlyYearsStatuses(null);
        var eytsStatus = allStatuses.SingleOrDefault(x => x.dfeta_Value == "220");
        var (teacherId, ittProviderUkprn, _) = await CreatePerson(earlyYears: eyts, hasActiveSanctions: false, assessmentOnly: asessmentonlyroute);
        var updateIttQualification = await _dataverseAdapter.GetIttQualificationByCode("001");  // BEd
        var updateHeQualification = await _dataverseAdapter.GetHeQualificationByCode("401");  // Higher Degree

        // Act
        var ex = await Assert.ThrowsAsync<ArgumentException>(async () => await _dataverseAdapter.UpdateTeacherImpl(new UpdateTeacherCommand()
        {
            TeacherId = teacherId,
            InitialTeacherTraining = new UpdateTeacherCommandInitialTeacherTraining()
            {
                ProviderUkprn = ittProviderUkprn,
                ProgrammeStartDate = new DateOnly(2011, 11, 01),
                ProgrammeEndDate = new DateOnly(2012, 11, 01),
                ProgrammeType = programmetype,
                Subject1 = "100403",  // mathematics
                Subject2 = "100366",  // computer science
                Subject3 = "100302",  // history
                AgeRangeFrom = dfeta_AgeRange._11,
                AgeRangeTo = dfeta_AgeRange._12,
                IttQualificationValue = updateIttQualification.dfeta_Value,
                Result = ittResult
            },
            Qualification = new UpdateTeacherCommandQualification()
            {
                CountryCode = "XK",
                Subject = "100366",  // computer science
                Class = dfeta_classdivision.Firstclasshonours,
                Date = new DateOnly(2022, 01, 28),
                ProviderUkprn = ittProviderUkprn,
                HeQualificationValue = updateHeQualification.dfeta_Value,
                Subject2 = "X300",
                Subject3 = "N400"
            }
        }));

        // Assert
        Assert.NotNull(ex);
        Assert.Contains($"Invalid ITT outcome: '{ittResult}'", ex.Message);
    }

    [Theory]
    [InlineData(true, false, dfeta_ITTProgrammeType.EYITTAssessmentOnly, dfeta_ITTResult.InTraining)]  //eyts
    [InlineData(false, true, dfeta_ITTProgrammeType.AssessmentOnlyRoute, dfeta_ITTResult.UnderAssessment)] //aor
    [InlineData(false, false, dfeta_ITTProgrammeType.RegisteredTeacherProgramme, dfeta_ITTResult.Deferred)] //qts
    public async Task Given_failed_itt_record_passing_result_returns_error(bool eyts, bool asessmentonlyroute, dfeta_ITTProgrammeType programmetype, dfeta_ITTResult ittResult)
    {
        // Arrange
        var allStatuses = await _dataverseAdapter.GetAllEarlyYearsStatuses(null);
        var eytsStatus = allStatuses.SingleOrDefault(x => x.dfeta_Value == "220");
        var (teacherId, ittProviderUkprn, _) = await CreatePerson(earlyYears: eyts, hasActiveSanctions: false, assessmentOnly: asessmentonlyroute);
        var updateIttQualification = await _dataverseAdapter.GetIttQualificationByCode("001");  // BEd
        var updateHeQualification = await _dataverseAdapter.GetHeQualificationByCode("401");  // Higher Degree
        var (failResult, _) = await _dataverseAdapter.SetIttResultForTeacherImpl(
            teacherId,
            ittProviderUkprn,
            dfeta_ITTResult.Fail,
            null);

        // Act
        var (result, _) = await _dataverseAdapter.UpdateTeacherImpl(new UpdateTeacherCommand()
        {
            TeacherId = teacherId,
            InitialTeacherTraining = new UpdateTeacherCommandInitialTeacherTraining()
            {
                ProviderUkprn = ittProviderUkprn,
                ProgrammeStartDate = new DateOnly(2011, 11, 01),
                ProgrammeEndDate = new DateOnly(2012, 11, 01),
                ProgrammeType = programmetype,
                Subject1 = "100403",  // mathematics
                Subject2 = "100366",  // computer science
                Subject3 = "100302",  // history
                AgeRangeFrom = dfeta_AgeRange._11,
                AgeRangeTo = dfeta_AgeRange._12,
                IttQualificationValue = updateIttQualification.dfeta_Value,
                Result = ittResult
            },
            Qualification = new UpdateTeacherCommandQualification()
            {
                CountryCode = "XK",
                Subject = "100366",  // computer science
                Class = dfeta_classdivision.Firstclasshonours,
                Date = new DateOnly(2022, 01, 28),
                ProviderUkprn = ittProviderUkprn,
                HeQualificationValue = updateHeQualification.dfeta_Value,
                Subject2 = "X300",
                Subject3 = "N400"
            }
        });

        // Assert
        Assert.True(failResult.Succeeded);
        Assert.False(result.Succeeded);
        Assert.Equal(UpdateTeacherFailedReasons.UnableToChangeFailedResult, result.FailedReasons);
    }

    [Theory]
    [InlineData(dfeta_ITTResult.Pass)]
    [InlineData(dfeta_ITTResult.Fail)]
    [InlineData(dfeta_ITTResult.Withdrawn)]
    public async Task Given_failed_eyts_itt_record_cannot_change_result_to(dfeta_ITTResult ittResult)
    {
        // Arrange
        var allStatuses = await _dataverseAdapter.GetAllEarlyYearsStatuses(null);
        var eytsStatus = allStatuses.SingleOrDefault(x => x.dfeta_Value == "220");
        var (teacherId, ittProviderUkprn, _) = await CreatePerson(earlyYears: true, hasActiveSanctions: false);
        var updateIttQualification = await _dataverseAdapter.GetIttQualificationByCode("001");  // BEd
        var updateHeQualification = await _dataverseAdapter.GetHeQualificationByCode("401");  // Higher Degree
        var (failResult, _) = await _dataverseAdapter.SetIttResultForTeacherImpl(
            teacherId,
            ittProviderUkprn,
            dfeta_ITTResult.Fail,
            null);

        // Act
        var ex = await Assert.ThrowsAsync<ArgumentException>(async () => await _dataverseAdapter.UpdateTeacherImpl(new UpdateTeacherCommand()
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
                IttQualificationValue = updateIttQualification.dfeta_Value,
                Result = ittResult
            },
            Qualification = new UpdateTeacherCommandQualification()
            {
                CountryCode = "XK",
                Subject = "100366",  // computer science
                Class = dfeta_classdivision.Firstclasshonours,
                Date = new DateOnly(2022, 01, 28),
                ProviderUkprn = ittProviderUkprn,
                HeQualificationValue = updateHeQualification.dfeta_Value,
                Subject2 = "X300",
                Subject3 = "N400"
            }
        }));

        // Assert
        Assert.True(failResult.Succeeded);
        Assert.NotNull(ex);
        Assert.Contains($"Invalid ITT outcome: '{ittResult}'", ex.Message);
    }

    [Theory]
    [InlineData(dfeta_ITTResult.Pass)]
    [InlineData(dfeta_ITTResult.Fail)]
    public async Task Given_withdrawn_eyts_itt_record_cannot_unwithdraw_record_to_result(dfeta_ITTResult ittResult)
    {
        // Arrange
        var allStatuses = await _dataverseAdapter.GetAllEarlyYearsStatuses(null);
        var eytsStatus = allStatuses.SingleOrDefault(x => x.dfeta_Value == "220");
        var (teacherId, ittProviderUkprn, _) = await CreatePerson(earlyYears: true, hasActiveSanctions: false);
        var assessmentDate = default(DateOnly?);
        var updateIttQualification = await _dataverseAdapter.GetIttQualificationByCode("001");  // BEd
        var updateHeQualification = await _dataverseAdapter.GetHeQualificationByCode("401");  // Higher Degree
        var (withdrawResult, _) = await _dataverseAdapter.SetIttResultForTeacherImpl(
            teacherId,
            ittProviderUkprn,
            dfeta_ITTResult.Withdrawn,
            assessmentDate);

        // Act
        var ex = await Assert.ThrowsAsync<ArgumentException>(async () => await _dataverseAdapter.UpdateTeacherImpl(new UpdateTeacherCommand()
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
                IttQualificationValue = updateIttQualification.dfeta_Value,
                Result = ittResult
            },
            Qualification = new UpdateTeacherCommandQualification()
            {
                CountryCode = "XK",
                Subject = "100366",  // computer science
                Class = dfeta_classdivision.Firstclasshonours,
                Date = new DateOnly(2022, 01, 28),
                ProviderUkprn = ittProviderUkprn,
                HeQualificationValue = updateHeQualification.dfeta_Value,
                Subject2 = "X300",
                Subject3 = "N400"
            }
        }));

        // Assert
        Assert.True(withdrawResult.Succeeded);
        Assert.Contains($"Invalid ITT outcome: '{ittResult}'", ex.Message);
    }

    [Fact]
    public async Task Given_withdrawn_eyts_itt_record_passing_withdrawn_returns_success_without_updating()
    {
        // Arrange
        var allStatuses = await _dataverseAdapter.GetAllEarlyYearsStatuses(null);
        var eytsStatus = allStatuses.SingleOrDefault(x => x.dfeta_Value == "220");
        var (teacherId, ittProviderUkprn, _) = await CreatePerson(earlyYears: true, hasActiveSanctions: false);
        var assessmentDate = default(DateOnly?);
        var updateIttQualification = await _dataverseAdapter.GetIttQualificationByCode("001");  // BEd
        var updateHeQualification = await _dataverseAdapter.GetHeQualificationByCode("401");  // Higher Degree
        var (withdrawResult, _) = await _dataverseAdapter.SetIttResultForTeacherImpl(
            teacherId,
            ittProviderUkprn,
            dfeta_ITTResult.Withdrawn,
            assessmentDate);

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
                IttQualificationValue = updateIttQualification.dfeta_Value,
                Result = dfeta_ITTResult.Withdrawn
            },
            Qualification = new UpdateTeacherCommandQualification()
            {
                CountryCode = "XK",
                Subject = "100366",  // computer science
                Class = dfeta_classdivision.Firstclasshonours,
                Date = new DateOnly(2022, 01, 28),
                ProviderUkprn = ittProviderUkprn,
                HeQualificationValue = updateHeQualification.dfeta_Value,
                Subject2 = "X300",
                Subject3 = "N400"
            }
        });

        // Assert
        Assert.True(withdrawResult.Succeeded);
        Assert.Null(transactionRequest);
        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task Given_withdrawn_eyts_itt_record_cannot_unwithdraw_to_deferred()
    {
        // Arrange
        var allStatuses = await _dataverseAdapter.GetAllEarlyYearsStatuses(null);
        var eytsStatus = allStatuses.SingleOrDefault(x => x.dfeta_Value == "220");
        var (teacherId, ittProviderUkprn, _) = await CreatePerson(earlyYears: true, hasActiveSanctions: false);
        var assessmentDate = default(DateOnly?);
        var updateIttQualification = await _dataverseAdapter.GetIttQualificationByCode("001");  // BEd
        var updateHeQualification = await _dataverseAdapter.GetHeQualificationByCode("401");  // Higher Degree
        var (withdrawResult, _) = await _dataverseAdapter.SetIttResultForTeacherImpl(
            teacherId,
            ittProviderUkprn,
            dfeta_ITTResult.Withdrawn,
            assessmentDate);

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
                IttQualificationValue = updateIttQualification.dfeta_Value,
                Result = dfeta_ITTResult.Deferred
            },
            Qualification = new UpdateTeacherCommandQualification()
            {
                CountryCode = "XK",
                Subject = "100366",  // computer science
                Class = dfeta_classdivision.Firstclasshonours,
                Date = new DateOnly(2022, 01, 28),
                ProviderUkprn = ittProviderUkprn,
                HeQualificationValue = updateHeQualification.dfeta_Value,
                Subject2 = "X300",
                Subject3 = "N400"
            }
        });

        // Assert
        Assert.True(withdrawResult.Succeeded);
        Assert.Null(transactionRequest);
        Assert.False(result.Succeeded);
        Assert.Equal(UpdateTeacherFailedReasons.UnableToUnwithdrawToDeferredStatus, result.FailedReasons);
    }

    [Theory]
    [InlineData(dfeta_ITTResult.InTraining)]
    public async Task Given_withdrawn_eyts_itt_record_can_unwithdraw_record(dfeta_ITTResult ittResult)
    {
        // Arrange
        var allStatuses = await _dataverseAdapter.GetAllEarlyYearsStatuses(null);
        var eytsStatus = allStatuses.SingleOrDefault(x => x.dfeta_Value == "220");
        var (teacherId, ittProviderUkprn, _) = await CreatePerson(earlyYears: true, hasActiveSanctions: false);
        var assessmentDate = default(DateOnly?);
        var updateIttQualification = await _dataverseAdapter.GetIttQualificationByCode("001");  // BEd
        var updateHeQualification = await _dataverseAdapter.GetHeQualificationByCode("401");  // Higher Degree
        var (withdrawResult, _) = await _dataverseAdapter.SetIttResultForTeacherImpl(
            teacherId,
            ittProviderUkprn,
            dfeta_ITTResult.Withdrawn,
            assessmentDate);

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
                IttQualificationValue = updateIttQualification.dfeta_Value,
                Result = ittResult
            },
            Qualification = new UpdateTeacherCommandQualification()
            {
                CountryCode = "XK",
                Subject = "100366",  // computer science
                Class = dfeta_classdivision.Firstclasshonours,
                Date = new DateOnly(2022, 01, 28),
                ProviderUkprn = ittProviderUkprn,
                HeQualificationValue = updateHeQualification.dfeta_Value,
                Subject2 = "X300",
                Subject3 = "N400"
            }
        });

        // Assert
        var qtsRegistration = transactionRequest.AssertSingleUpdateRequest<dfeta_qtsregistration>();
        Assert.True(withdrawResult.Succeeded);
        Assert.True(result.Succeeded);
        Assert.Equal(qtsRegistration.dfeta_EarlyYearsStatusId.Id, eytsStatus.Id);
    }

    [Theory]
    [InlineData(dfeta_ITTResult.Pass)]
    [InlineData(dfeta_ITTResult.Fail)]
    public async Task Given_withdrawn_qts_itt_record_cannot_unwithdraw_record_to_result(dfeta_ITTResult ittResult)
    {
        // Arrange
        var allStatuses = await _dataverseAdapter.GetAllEarlyYearsStatuses(null);
        var eytsStatus = allStatuses.SingleOrDefault(x => x.dfeta_Value == "220");
        var (teacherId, ittProviderUkprn, _) = await CreatePerson(earlyYears: false, hasActiveSanctions: false);
        var assessmentDate = default(DateOnly?);
        var updateIttQualification = await _dataverseAdapter.GetIttQualificationByCode("001");  // BEd
        var updateHeQualification = await _dataverseAdapter.GetHeQualificationByCode("401");  // Higher Degree
        var (withdrawResult, _) = await _dataverseAdapter.SetIttResultForTeacherImpl(
            teacherId,
            ittProviderUkprn,
            dfeta_ITTResult.Withdrawn,
            assessmentDate);

        // Act
        var ex = await Assert.ThrowsAsync<ArgumentException>(async () => await _dataverseAdapter.UpdateTeacherImpl(new UpdateTeacherCommand()
        {
            TeacherId = teacherId,
            InitialTeacherTraining = new UpdateTeacherCommandInitialTeacherTraining()
            {
                ProviderUkprn = ittProviderUkprn,
                ProgrammeStartDate = new DateOnly(2011, 11, 01),
                ProgrammeEndDate = new DateOnly(2012, 11, 01),
                ProgrammeType = dfeta_ITTProgrammeType.RegisteredTeacherProgramme,
                Subject1 = "100403",  // mathematics
                Subject2 = "100366",  // computer science
                Subject3 = "100302",  // history
                AgeRangeFrom = dfeta_AgeRange._11,
                AgeRangeTo = dfeta_AgeRange._12,
                IttQualificationValue = updateIttQualification.dfeta_Value,
                Result = ittResult
            },
            Qualification = new UpdateTeacherCommandQualification()
            {
                CountryCode = "XK",
                Subject = "100366",  // computer science
                Class = dfeta_classdivision.Firstclasshonours,
                Date = new DateOnly(2022, 01, 28),
                ProviderUkprn = ittProviderUkprn,
                HeQualificationValue = updateHeQualification.dfeta_Value,
                Subject2 = "X300",
                Subject3 = "N400"
            }
        }));

        // Assert
        Assert.True(withdrawResult.Succeeded);
        Assert.Contains($"Invalid ITT outcome: '{ittResult}'", ex.Message);
    }

    [Fact]
    public async Task Given_withdrawn_qts_itt_record_cannot_unwithdraw_to_deferred()
    {
        // Arrange
        var allStatuses = await _dataverseAdapter.GetAllEarlyYearsStatuses(null);
        var eytsStatus = allStatuses.SingleOrDefault(x => x.dfeta_Value == "220");
        var (teacherId, ittProviderUkprn, _) = await CreatePerson(earlyYears: false, hasActiveSanctions: false, assessmentOnly: false);
        var assessmentDate = default(DateOnly?);
        var updateIttQualification = await _dataverseAdapter.GetIttQualificationByCode("001");  // BEd
        var updateHeQualification = await _dataverseAdapter.GetHeQualificationByCode("401");  // Higher Degree
        var (withdrawResult, _) = await _dataverseAdapter.SetIttResultForTeacherImpl(
            teacherId,
            ittProviderUkprn,
            dfeta_ITTResult.Withdrawn,
            assessmentDate);

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
                Subject1 = "100403",  // mathematics
                Subject2 = "100366",  // computer science
                Subject3 = "100302",  // history
                AgeRangeFrom = dfeta_AgeRange._11,
                AgeRangeTo = dfeta_AgeRange._12,
                IttQualificationValue = updateIttQualification.dfeta_Value,
                Result = dfeta_ITTResult.Deferred
            },
            Qualification = new UpdateTeacherCommandQualification()
            {
                CountryCode = "XK",
                Subject = "100366",  // computer science
                Class = dfeta_classdivision.Firstclasshonours,
                Date = new DateOnly(2022, 01, 28),
                ProviderUkprn = ittProviderUkprn,
                HeQualificationValue = updateHeQualification.dfeta_Value,
                Subject2 = "X300",
                Subject3 = "N400"
            }
        });

        // Assert
        Assert.True(withdrawResult.Succeeded);
        Assert.Null(transactionRequest);
        Assert.False(result.Succeeded);
        Assert.Equal(UpdateTeacherFailedReasons.UnableToUnwithdrawToDeferredStatus, result.FailedReasons);
    }

    [Fact]
    public async Task Given_withdrawn_qts_itt_record_passing_withdrawn_returns_success_without_updating()
    {
        // Arrange
        var allStatuses = await _dataverseAdapter.GetAllEarlyYearsStatuses(null);
        var eytsStatus = allStatuses.SingleOrDefault(x => x.dfeta_Value == "220");
        var (teacherId, ittProviderUkprn, _) = await CreatePerson(earlyYears: false, hasActiveSanctions: false, assessmentOnly: false);
        var assessmentDate = default(DateOnly?);
        var updateIttQualification = await _dataverseAdapter.GetIttQualificationByCode("001");  // BEd
        var updateHeQualification = await _dataverseAdapter.GetHeQualificationByCode("401");  // Higher Degree
        var (withdrawResult, _) = await _dataverseAdapter.SetIttResultForTeacherImpl(
            teacherId,
            ittProviderUkprn,
            dfeta_ITTResult.Withdrawn,
            assessmentDate);

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
                Subject1 = "100403",  // mathematics
                Subject2 = "100366",  // computer science
                Subject3 = "100302",  // history
                AgeRangeFrom = dfeta_AgeRange._11,
                AgeRangeTo = dfeta_AgeRange._12,
                IttQualificationValue = updateIttQualification.dfeta_Value,
                Result = dfeta_ITTResult.Withdrawn
            },
            Qualification = new UpdateTeacherCommandQualification()
            {
                CountryCode = "XK",
                Subject = "100366",  // computer science
                Class = dfeta_classdivision.Firstclasshonours,
                Date = new DateOnly(2022, 01, 28),
                ProviderUkprn = ittProviderUkprn,
                HeQualificationValue = updateHeQualification.dfeta_Value,
                Subject2 = "X300",
                Subject3 = "N400"
            }
        });

        // Assert
        Assert.True(withdrawResult.Succeeded);
        Assert.Null(transactionRequest);
        Assert.True(result.Succeeded);
    }

    [Theory]
    [InlineData(dfeta_ITTResult.InTraining)]
    public async Task Given_withdrawn_qts_itt_record_can_unwithdraw_record(dfeta_ITTResult ittResult)
    {
        // Arrange
        var allStatuses = await _dataverseAdapter.GetAllTeacherStatuses(null);
        var qtsStatus = allStatuses.SingleOrDefault(x => x.dfeta_Value == "211");
        var (teacherId, ittProviderUkprn, _) = await CreatePerson(earlyYears: false, hasActiveSanctions: false);
        var assessmentDate = default(DateOnly?);
        var updateIttQualification = await _dataverseAdapter.GetIttQualificationByCode("001");  // BEd
        var updateHeQualification = await _dataverseAdapter.GetHeQualificationByCode("401");  // Higher Degree
        var (withdrawResult, _) = await _dataverseAdapter.SetIttResultForTeacherImpl(
            teacherId,
            ittProviderUkprn,
            dfeta_ITTResult.Withdrawn,
            assessmentDate);

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
                Subject1 = "100403",  // mathematics
                Subject2 = "100366",  // computer science
                Subject3 = "100302",  // history
                AgeRangeFrom = dfeta_AgeRange._11,
                AgeRangeTo = dfeta_AgeRange._12,
                IttQualificationValue = updateIttQualification.dfeta_Value,
                Result = ittResult
            },
            Qualification = new UpdateTeacherCommandQualification()
            {
                CountryCode = "XK",
                Subject = "100366",  // computer science
                Class = dfeta_classdivision.Firstclasshonours,
                Date = new DateOnly(2022, 01, 28),
                ProviderUkprn = ittProviderUkprn,
                HeQualificationValue = updateHeQualification.dfeta_Value,
                Subject2 = "X300",
                Subject3 = "N400"
            }
        });

        // Assert
        var qtsRegistration = transactionRequest.AssertSingleUpdateRequest<dfeta_qtsregistration>();
        Assert.True(withdrawResult.Succeeded);
        Assert.True(result.Succeeded);
        Assert.Equal(qtsRegistration.dfeta_TeacherStatusId.Id, qtsStatus.Id);
    }


    [Fact]
    public async Task Given_withdrawn_assessmentonlyroute_itt_record_cannot_unwithdraw_to_deferred()
    {
        // Arrange
        var allStatuses = await _dataverseAdapter.GetAllEarlyYearsStatuses(null);
        var eytsStatus = allStatuses.SingleOrDefault(x => x.dfeta_Value == "220");
        var (teacherId, ittProviderUkprn, _) = await CreatePerson(earlyYears: false, hasActiveSanctions: false, assessmentOnly: true);
        var assessmentDate = default(DateOnly?);
        var updateIttQualification = await _dataverseAdapter.GetIttQualificationByCode("001");  // BEd
        var updateHeQualification = await _dataverseAdapter.GetHeQualificationByCode("401");  // Higher Degree
        var (withdrawResult, _) = await _dataverseAdapter.SetIttResultForTeacherImpl(
            teacherId,
            ittProviderUkprn,
            dfeta_ITTResult.Withdrawn,
            assessmentDate);

        // Act
        var (result, transactionRequest) = await _dataverseAdapter.UpdateTeacherImpl(new UpdateTeacherCommand()
        {
            TeacherId = teacherId,
            InitialTeacherTraining = new UpdateTeacherCommandInitialTeacherTraining()
            {
                ProviderUkprn = ittProviderUkprn,
                ProgrammeStartDate = new DateOnly(2011, 11, 01),
                ProgrammeEndDate = new DateOnly(2012, 11, 01),
                ProgrammeType = dfeta_ITTProgrammeType.AssessmentOnlyRoute,
                Subject1 = "100403",  // mathematics
                Subject2 = "100366",  // computer science
                Subject3 = "100302",  // history
                AgeRangeFrom = dfeta_AgeRange._11,
                AgeRangeTo = dfeta_AgeRange._12,
                IttQualificationValue = updateIttQualification.dfeta_Value,
                Result = dfeta_ITTResult.Deferred
            },
            Qualification = new UpdateTeacherCommandQualification()
            {
                CountryCode = "XK",
                Subject = "100366",  // computer science
                Class = dfeta_classdivision.Firstclasshonours,
                Date = new DateOnly(2022, 01, 28),
                ProviderUkprn = ittProviderUkprn,
                HeQualificationValue = updateHeQualification.dfeta_Value,
                Subject2 = "X300",
                Subject3 = "N400"
            }
        });

        // Assert
        Assert.True(withdrawResult.Succeeded);
        Assert.Null(transactionRequest);
        Assert.False(result.Succeeded);
        Assert.Equal(UpdateTeacherFailedReasons.UnableToUnwithdrawToDeferredStatus, result.FailedReasons);
    }

    [Theory]
    [InlineData(dfeta_ITTResult.Pass)]
    [InlineData(dfeta_ITTResult.Fail)]
    public async Task Given_withdrawn_assessmentonlyroute_itt_record_cannot_unwithdraw_record_to_result(dfeta_ITTResult ittResult)
    {
        // Arrange
        var allStatuses = await _dataverseAdapter.GetAllEarlyYearsStatuses(null);
        var eytsStatus = allStatuses.SingleOrDefault(x => x.dfeta_Value == "220");
        var (teacherId, ittProviderUkprn, _) = await CreatePerson(earlyYears: false, hasActiveSanctions: false, assessmentOnly: true);
        var assessmentDate = default(DateOnly?);
        var updateIttQualification = await _dataverseAdapter.GetIttQualificationByCode("001");  // BEd
        var updateHeQualification = await _dataverseAdapter.GetHeQualificationByCode("401");  // Higher Degree
        var (withdrawResult, _) = await _dataverseAdapter.SetIttResultForTeacherImpl(
            teacherId,
            ittProviderUkprn,
            dfeta_ITTResult.Withdrawn,
            assessmentDate);

        // Act
        var ex = await Assert.ThrowsAsync<ArgumentException>(async () => await _dataverseAdapter.UpdateTeacherImpl(new UpdateTeacherCommand()
        {
            TeacherId = teacherId,
            InitialTeacherTraining = new UpdateTeacherCommandInitialTeacherTraining()
            {
                ProviderUkprn = ittProviderUkprn,
                ProgrammeStartDate = new DateOnly(2011, 11, 01),
                ProgrammeEndDate = new DateOnly(2012, 11, 01),
                ProgrammeType = dfeta_ITTProgrammeType.AssessmentOnlyRoute,
                Subject1 = "100403",  // mathematics
                Subject2 = "100366",  // computer science
                Subject3 = "100302",  // history
                AgeRangeFrom = dfeta_AgeRange._11,
                AgeRangeTo = dfeta_AgeRange._12,
                IttQualificationValue = updateIttQualification.dfeta_Value,
                Result = ittResult
            },
            Qualification = new UpdateTeacherCommandQualification()
            {
                CountryCode = "XK",
                Subject = "100366",  // computer science
                Class = dfeta_classdivision.Firstclasshonours,
                Date = new DateOnly(2022, 01, 28),
                ProviderUkprn = ittProviderUkprn,
                HeQualificationValue = updateHeQualification.dfeta_Value,
                Subject2 = "X300",
                Subject3 = "N400"
            }
        }));

        // Assert
        Assert.True(withdrawResult.Succeeded);
        Assert.Contains($"Invalid ITT outcome: '{ittResult}'", ex.Message);
    }

    [Fact]
    public async Task Given_withdrawn_assessmentonlyroute_with_none_empty_teacherstatus_fail()
    {
        // Arrange
        var allStatuses = await _dataverseAdapter.GetAllTeacherStatuses(null);
        var teacherStatus = allStatuses.SingleOrDefault(x => x.dfeta_Value == "211");
        var (teacherId, ittProviderUkprn, _) = await CreatePerson(earlyYears: false, hasActiveSanctions: false, assessmentOnly: true);
        var assessmentDate = default(DateOnly?);
        var updateIttQualification = await _dataverseAdapter.GetIttQualificationByCode("001");  // BEd
        var updateHeQualification = await _dataverseAdapter.GetHeQualificationByCode("401");  // Higher Degree
        var (withdrawResult, _) = await _dataverseAdapter.SetIttResultForTeacherImpl(
            teacherId,
            ittProviderUkprn,
            dfeta_ITTResult.Withdrawn,
            assessmentDate);

        var qts = await _dataverseAdapter.GetQtsRegistrationsByTeacher(teacherId, new[] { dfeta_qtsregistration.Fields.dfeta_EarlyYearsStatusId,
                    dfeta_qtsregistration.Fields.dfeta_TeacherStatusId,
                    dfeta_qtsregistration.Fields.StateCode });

        var qtsId = qts.Single().Id;
        await _organizationService.ExecuteAsync(new UpdateRequest()
        {
            Target = new dfeta_qtsregistration()
            {
                Id = qtsId,
                dfeta_TeacherStatusId = new EntityReference(dfeta_teacherstatus.EntityLogicalName, teacherStatus.Id),
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
                ProgrammeType = dfeta_ITTProgrammeType.AssessmentOnlyRoute,
                Subject1 = "100403",  // mathematics
                Subject2 = "100366",  // computer science
                Subject3 = "100302",  // history
                AgeRangeFrom = dfeta_AgeRange._11,
                AgeRangeTo = dfeta_AgeRange._12,
                IttQualificationValue = updateIttQualification.dfeta_Value,
                Result = dfeta_ITTResult.UnderAssessment
            },
            Qualification = new UpdateTeacherCommandQualification()
            {
                CountryCode = "XK",
                Subject = "100366",  // computer science
                Class = dfeta_classdivision.Firstclasshonours,
                Date = new DateOnly(2022, 01, 28),
                ProviderUkprn = ittProviderUkprn,
                HeQualificationValue = updateHeQualification.dfeta_Value,
                Subject2 = "X300",
                Subject3 = "N400"
            }
        });

        // Assert
        Assert.True(withdrawResult.Succeeded);
        Assert.False(result.Succeeded);
        Assert.Equal(UpdateTeacherFailedReasons.NoMatchingQtsRecord, result.FailedReasons);
    }

    [Fact]
    public async Task Given_withdrawn_eyts_with_none_empty_earlyyearsteacherstatus_fail()
    {
        // Arrange
        var allStatuses = await _dataverseAdapter.GetAllEarlyYearsStatuses(null);
        var eytsteacherStatus = allStatuses.SingleOrDefault(x => x.dfeta_Value == "220");
        var (teacherId, ittProviderUkprn, _) = await CreatePerson(earlyYears: true, hasActiveSanctions: false, assessmentOnly: false);
        var assessmentDate = default(DateOnly?);
        var updateIttQualification = await _dataverseAdapter.GetIttQualificationByCode("001");  // BEd
        var updateHeQualification = await _dataverseAdapter.GetHeQualificationByCode("401");  // Higher Degree
        var (withdrawResult, _) = await _dataverseAdapter.SetIttResultForTeacherImpl(
            teacherId,
            ittProviderUkprn,
            dfeta_ITTResult.Withdrawn,
            assessmentDate);

        var qts = await _dataverseAdapter.GetQtsRegistrationsByTeacher(teacherId, new[] { dfeta_qtsregistration.Fields.dfeta_EarlyYearsStatusId,
                    dfeta_qtsregistration.Fields.dfeta_TeacherStatusId,
                    dfeta_qtsregistration.Fields.StateCode });

        var qtsId = qts.Single().Id;
        await _organizationService.ExecuteAsync(new UpdateRequest()
        {
            Target = new dfeta_qtsregistration()
            {
                Id = qtsId,
                dfeta_EarlyYearsStatusId = new EntityReference(dfeta_earlyyearsstatus.EntityLogicalName, eytsteacherStatus.Id),
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
                Subject1 = "100403",  // mathematics
                Subject2 = "100366",  // computer science
                Subject3 = "100302",  // history
                AgeRangeFrom = dfeta_AgeRange._11,
                AgeRangeTo = dfeta_AgeRange._12,
                IttQualificationValue = updateIttQualification.dfeta_Value,
                Result = dfeta_ITTResult.InTraining
            },
            Qualification = new UpdateTeacherCommandQualification()
            {
                CountryCode = "XK",
                Subject = "100366",  // computer science
                Class = dfeta_classdivision.Firstclasshonours,
                Date = new DateOnly(2022, 01, 28),
                ProviderUkprn = ittProviderUkprn,
                HeQualificationValue = updateHeQualification.dfeta_Value,
                Subject2 = "X300",
                Subject3 = "N400"
            }
        });

        // Assert
        Assert.True(withdrawResult.Succeeded);
        Assert.False(result.Succeeded);
        Assert.Equal(UpdateTeacherFailedReasons.NoMatchingQtsRecord, result.FailedReasons);
    }


    [Fact]
    public async Task Given_withdrawn_qts_with_none_empty_teacherstatus_fail()
    {
        // Arrange
        var allStatuses = await _dataverseAdapter.GetAllTeacherStatuses(null);
        var teacherStatus = allStatuses.SingleOrDefault(x => x.dfeta_Value == "211");
        var (teacherId, ittProviderUkprn, _) = await CreatePerson(earlyYears: false, hasActiveSanctions: false, assessmentOnly: false);
        var assessmentDate = default(DateOnly?);
        var updateIttQualification = await _dataverseAdapter.GetIttQualificationByCode("001");  // BEd
        var updateHeQualification = await _dataverseAdapter.GetHeQualificationByCode("401");  // Higher Degree
        var (withdrawResult, _) = await _dataverseAdapter.SetIttResultForTeacherImpl(
            teacherId,
            ittProviderUkprn,
            dfeta_ITTResult.Withdrawn,
            assessmentDate);

        var qts = await _dataverseAdapter.GetQtsRegistrationsByTeacher(teacherId, new[] { dfeta_qtsregistration.Fields.dfeta_EarlyYearsStatusId,
                    dfeta_qtsregistration.Fields.dfeta_TeacherStatusId,
                    dfeta_qtsregistration.Fields.StateCode });

        var qtsId = qts.Single().Id;
        await _organizationService.ExecuteAsync(new UpdateRequest()
        {
            Target = new dfeta_qtsregistration()
            {
                Id = qtsId,
                dfeta_TeacherStatusId = new EntityReference(dfeta_teacherstatus.EntityLogicalName, teacherStatus.Id),
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
                ProgrammeType = dfeta_ITTProgrammeType.AssessmentOnlyRoute,
                Subject1 = "100403",  // mathematics
                Subject2 = "100366",  // computer science
                Subject3 = "100302",  // history
                AgeRangeFrom = dfeta_AgeRange._11,
                AgeRangeTo = dfeta_AgeRange._12,
                IttQualificationValue = updateIttQualification.dfeta_Value,
                Result = dfeta_ITTResult.InTraining
            },
            Qualification = new UpdateTeacherCommandQualification()
            {
                CountryCode = "XK",
                Subject = "100366",  // computer science
                Class = dfeta_classdivision.Firstclasshonours,
                Date = new DateOnly(2022, 01, 28),
                ProviderUkprn = ittProviderUkprn,
                HeQualificationValue = updateHeQualification.dfeta_Value,
                Subject2 = "X300",
                Subject3 = "N400"
            }
        });

        // Assert
        Assert.True(withdrawResult.Succeeded);
        Assert.False(result.Succeeded);
        Assert.Equal(UpdateTeacherFailedReasons.NoMatchingQtsRecord, result.FailedReasons);
    }

    [Fact]
    public async Task Given_withdrawn_assessmentonlyroute_itt_record_passing_withdrawn_returns_success_without_updating()
    {
        // Arrange
        var allStatuses = await _dataverseAdapter.GetAllEarlyYearsStatuses(null);
        var eytsStatus = allStatuses.SingleOrDefault(x => x.dfeta_Value == "220");
        var (teacherId, ittProviderUkprn, _) = await CreatePerson(earlyYears: false, hasActiveSanctions: false, assessmentOnly: true);
        var assessmentDate = default(DateOnly?);
        var updateIttQualification = await _dataverseAdapter.GetIttQualificationByCode("001");  // BEd
        var updateHeQualification = await _dataverseAdapter.GetHeQualificationByCode("401");  // Higher Degree
        var (withdrawResult, _) = await _dataverseAdapter.SetIttResultForTeacherImpl(
            teacherId,
            ittProviderUkprn,
            dfeta_ITTResult.Withdrawn,
            assessmentDate);

        // Act
        var (result, transactionRequest) = await _dataverseAdapter.UpdateTeacherImpl(new UpdateTeacherCommand()
        {
            TeacherId = teacherId,
            InitialTeacherTraining = new UpdateTeacherCommandInitialTeacherTraining()
            {
                ProviderUkprn = ittProviderUkprn,
                ProgrammeStartDate = new DateOnly(2011, 11, 01),
                ProgrammeEndDate = new DateOnly(2012, 11, 01),
                ProgrammeType = dfeta_ITTProgrammeType.AssessmentOnlyRoute,
                Subject1 = "100403",  // mathematics
                Subject2 = "100366",  // computer science
                Subject3 = "100302",  // history
                AgeRangeFrom = dfeta_AgeRange._11,
                AgeRangeTo = dfeta_AgeRange._12,
                IttQualificationValue = updateIttQualification.dfeta_Value,
                Result = dfeta_ITTResult.Withdrawn
            },
            Qualification = new UpdateTeacherCommandQualification()
            {
                CountryCode = "XK",
                Subject = "100366",  // computer science
                Class = dfeta_classdivision.Firstclasshonours,
                Date = new DateOnly(2022, 01, 28),
                ProviderUkprn = ittProviderUkprn,
                HeQualificationValue = updateHeQualification.dfeta_Value,
                Subject2 = "X300",
                Subject3 = "N400"
            }
        });

        // Assert
        Assert.True(withdrawResult.Succeeded);
        Assert.Null(transactionRequest);
        Assert.True(result.Succeeded);
    }

    [Theory]
    [InlineData(dfeta_ITTResult.UnderAssessment)]
    public async Task Given_withdrawn_assessmentonlyroute_itt_record_can_unwithdraw_record(dfeta_ITTResult ittResult)
    {
        // Arrange
        var allStatuses = await _dataverseAdapter.GetAllTeacherStatuses(null);
        var aorStatus = allStatuses.SingleOrDefault(x => x.dfeta_Value == "212");
        var (teacherId, ittProviderUkprn, _) = await CreatePerson(earlyYears: false, hasActiveSanctions: false, assessmentOnly: true);
        var assessmentDate = default(DateOnly?);
        var updateIttQualification = await _dataverseAdapter.GetIttQualificationByCode("001");  // BEd
        var updateHeQualification = await _dataverseAdapter.GetHeQualificationByCode("401");  // Higher Degree
        var (withdrawResult, _) = await _dataverseAdapter.SetIttResultForTeacherImpl(
            teacherId,
            ittProviderUkprn,
            dfeta_ITTResult.Withdrawn,
            assessmentDate);

        // Act
        var (result, transactionRequest) = await _dataverseAdapter.UpdateTeacherImpl(new UpdateTeacherCommand()
        {
            TeacherId = teacherId,
            InitialTeacherTraining = new UpdateTeacherCommandInitialTeacherTraining()
            {
                ProviderUkprn = ittProviderUkprn,
                ProgrammeStartDate = new DateOnly(2011, 11, 01),
                ProgrammeEndDate = new DateOnly(2012, 11, 01),
                ProgrammeType = dfeta_ITTProgrammeType.AssessmentOnlyRoute,
                Subject1 = "100403",  // mathematics
                Subject2 = "100366",  // computer science
                Subject3 = "100302",  // history
                AgeRangeFrom = dfeta_AgeRange._11,
                AgeRangeTo = dfeta_AgeRange._12,
                IttQualificationValue = updateIttQualification.dfeta_Value,
                Result = ittResult
            },
            Qualification = new UpdateTeacherCommandQualification()
            {
                CountryCode = "XK",
                Subject = "100366",  // computer science
                Class = dfeta_classdivision.Firstclasshonours,
                Date = new DateOnly(2022, 01, 28),
                ProviderUkprn = ittProviderUkprn,
                HeQualificationValue = updateHeQualification.dfeta_Value,
                Subject2 = "X300",
                Subject3 = "N400"
            }
        });

        // Assert
        var qtsRegistration = transactionRequest.AssertSingleUpdateRequest<dfeta_qtsregistration>();
        Assert.True(withdrawResult.Succeeded);
        Assert.True(result.Succeeded);
        Assert.Equal(qtsRegistration.dfeta_TeacherStatusId.Id, aorStatus.Id);
    }

    [Fact]
    public async Task Given_updating_existing_contact_update_without_subject3_returns_success()
    {
        // Arrange
        var (teacherId, ittProviderUkprn, _) = await CreatePerson(earlyYears: true, hasActiveSanctions: false);

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

        var oldProvider = (await _dataverseAdapter.GetOrganizationsByUkprn(ittProviderUkprn, columnNames: Array.Empty<string>())).Single().Id;

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
                dfeta_qualification.Fields.dfeta_HE_HESubject2Id,
                dfeta_qualification.Fields.dfeta_HE_HESubject3Id,
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
        var (teacherId, ittProviderUkprn, _) = await CreatePerson(earlyYears: true, hasActiveSanctions: false);

        var updateHeSubject2Id = await _dataverseAdapter.GetHeSubjectByCode("X300");  // Academic Studies in Education
        var updateHeSubject3Id = await _dataverseAdapter.GetHeSubjectByCode("N400");  // Accounting
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
                HeQualificationValue = updateHeQualification.dfeta_Value,
                Subject2 = "X300",
                Subject3 = "N400"
            }
        });

        var oldProvider = (await _dataverseAdapter.GetOrganizationsByUkprn(ittProviderUkprn, columnNames: Array.Empty<string>())).Single().Id;

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
                dfeta_qualification.Fields.dfeta_HE_HESubject2Id,
                dfeta_qualification.Fields.dfeta_HE_HESubject3Id,
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
                Assert.Equal(updateHeSubject2Id.Id, item1.dfeta_HE_HESubject2Id.Id);
                Assert.Equal(updateHeSubject3Id.Id, item1.dfeta_HE_HESubject3Id.Id);
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
        var (teacherId, ittProviderUkprn, _) = await CreatePerson(earlyYears: true, hasActiveSanctions: false);

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
                Subject2 = "X300",
                Subject3 = "N400"
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
        var (teacherId, ittProviderUkprn, _) = await CreatePerson(earlyYears: false, hasActiveSanctions: false);

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
                Subject2 = "X300",
                Subject3 = "N400"
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
    [InlineData(dfeta_ITTProgrammeType.Providerled_postgrad)]
    [InlineData(dfeta_ITTProgrammeType.Providerled_undergrad)]
    public async Task Given_earlyyears_itt_cannot_change_eyts_programmetype_to_qts(dfeta_ITTProgrammeType programmeType)
    {
        // Arrange
        var (teacherId, ittProviderUkprn, _) = await CreatePerson(earlyYears: true, hasActiveSanctions: false);

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
                Subject2 = "X300",
                Subject3 = "N400"
            }
        });

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal(UpdateTeacherFailedReasons.CannotChangeProgrammeType, result.FailedReasons);
    }

    [Fact]
    public async Task Given_itt_cannot_change_eyts_programmetype_to_qts()
    {
        // Arrange
        var (teacherId, ittProviderUkprn, _) = await CreatePerson(earlyYears: true, hasActiveSanctions: false);

        // Act
        var (result, _) = await _dataverseAdapter.UpdateTeacherImpl(new UpdateTeacherCommand()
        {
            TeacherId = teacherId,
            InitialTeacherTraining = new UpdateTeacherCommandInitialTeacherTraining()
            {
                ProviderUkprn = ittProviderUkprn,
                ProgrammeStartDate = new DateOnly(2011, 11, 01),
                ProgrammeEndDate = new DateOnly(2012, 11, 01),
                ProgrammeType = dfeta_ITTProgrammeType.AssessmentOnlyRoute,
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
                Subject2 = "X300",
                Subject3 = "N400"
            }
        });

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal(UpdateTeacherFailedReasons.CannotChangeProgrammeType, result.FailedReasons);
    }

    [Fact]
    public async Task Given_deferred_asessmentonlyeroute_itt_update_to_InTraining_fails()
    {
        // Arrange
        var (teacherId, ittProviderUkprn, ittId) = await CreatePerson(earlyYears: false, true, hasActiveSanctions: false);
        await _organizationService.ExecuteAsync(new UpdateRequest()
        {
            Target = new dfeta_initialteachertraining()
            {
                Id = ittId,
                dfeta_Result = dfeta_ITTResult.Deferred,
            }
        });

        var ex = await Assert.ThrowsAsync<ArgumentException>(async () => await _dataverseAdapter.UpdateTeacherImpl(new UpdateTeacherCommand()
        {
            TeacherId = teacherId,
            InitialTeacherTraining = new UpdateTeacherCommandInitialTeacherTraining()
            {
                ProviderUkprn = ittProviderUkprn,
                ProgrammeStartDate = new DateOnly(2011, 11, 01),
                ProgrammeEndDate = new DateOnly(2012, 11, 01),
                ProgrammeType = dfeta_ITTProgrammeType.AssessmentOnlyRoute,
                Subject1 = "100366",  // computer science
                Subject2 = "100403",  // mathematics
                Subject3 = "100302",  // history
                AgeRangeFrom = dfeta_AgeRange._11,
                AgeRangeTo = dfeta_AgeRange._12,
                Result = dfeta_ITTResult.InTraining
            },
            Qualification = new UpdateTeacherCommandQualification()
            {
                CountryCode = "XK",
                Subject = "100366",  // computer science
                Class = dfeta_classdivision.Firstclasshonours,
                Date = new DateOnly(2022, 01, 28),
                ProviderUkprn = ittProviderUkprn,
                Subject2 = "X300",
                Subject3 = "N400"
            }
        }));

        // Assert
        Assert.Contains("InTraining not permitted for AsessmentOnlyRoute", ex.Message);
    }

    [Fact]
    public async Task Given_deferred_assessmentonlyeroute_itt_update_to_UnderAssessment_succeeds()
    {
        // Arrange
        var (teacherId, ittProviderUkprn, ittId) = await CreatePerson(earlyYears: false, true, hasActiveSanctions: false);
        await _organizationService.ExecuteAsync(new UpdateRequest()
        {
            Target = new dfeta_initialteachertraining()
            {
                Id = ittId,
                dfeta_Result = dfeta_ITTResult.Deferred,
            }
        });

        var (result, _) = await _dataverseAdapter.UpdateTeacherImpl(new UpdateTeacherCommand()
        {
            TeacherId = teacherId,
            InitialTeacherTraining = new UpdateTeacherCommandInitialTeacherTraining()
            {
                ProviderUkprn = ittProviderUkprn,
                ProgrammeStartDate = new DateOnly(2011, 11, 01),
                ProgrammeEndDate = new DateOnly(2012, 11, 01),
                ProgrammeType = dfeta_ITTProgrammeType.AssessmentOnlyRoute,
                Subject1 = "100366",  // computer science
                Subject2 = "100403",  // mathematics
                Subject3 = "100302",  // history
                AgeRangeFrom = dfeta_AgeRange._11,
                AgeRangeTo = dfeta_AgeRange._12,
                Result = dfeta_ITTResult.UnderAssessment
            },
            Qualification = new UpdateTeacherCommandQualification()
            {
                CountryCode = "XK",
                Subject = "100366",  // computer science
                Class = dfeta_classdivision.Firstclasshonours,
                Date = new DateOnly(2022, 01, 28),
                ProviderUkprn = ittProviderUkprn,
                Subject2 = "X300",
                Subject3 = "N400"
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
        Assert.Equal(UpdateTeacherFailedReasons.None, result.FailedReasons);
        Assert.Collection(itt,
            item1 => Assert.Equal(dfeta_ITTResult.UnderAssessment, item1.dfeta_Result)
        );
    }

    [Fact]
    public async Task Given_deferred_non_assessmentonlyeroute_itt_update_to_InTraining_succeeds()
    {
        // Arrange
        var (teacherId, ittProviderUkprn, ittId) = await CreatePerson(earlyYears: false, false, hasActiveSanctions: false);
        await _organizationService.ExecuteAsync(new UpdateRequest()
        {
            Target = new dfeta_initialteachertraining()
            {
                Id = ittId,
                dfeta_Result = dfeta_ITTResult.Deferred,
            }
        });

        var (result, _) = await _dataverseAdapter.UpdateTeacherImpl(new UpdateTeacherCommand()
        {
            TeacherId = teacherId,
            InitialTeacherTraining = new UpdateTeacherCommandInitialTeacherTraining()
            {
                ProviderUkprn = ittProviderUkprn,
                ProgrammeStartDate = new DateOnly(2011, 11, 01),
                ProgrammeEndDate = new DateOnly(2012, 11, 01),
                ProgrammeType = dfeta_ITTProgrammeType.UndergraduateOptIn,
                Subject1 = "100366",  // computer science
                Subject2 = "100403",  // mathematics
                Subject3 = "100302",  // history
                AgeRangeFrom = dfeta_AgeRange._11,
                AgeRangeTo = dfeta_AgeRange._12,
                Result = dfeta_ITTResult.InTraining
            },
            Qualification = new UpdateTeacherCommandQualification()
            {
                CountryCode = "XK",
                Subject = "100366",  // computer science
                Class = dfeta_classdivision.Firstclasshonours,
                Date = new DateOnly(2022, 01, 28),
                ProviderUkprn = ittProviderUkprn,
                Subject2 = "X300",
                Subject3 = "N400"
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
        Assert.Equal(UpdateTeacherFailedReasons.None, result.FailedReasons);
        Assert.Collection(itt,
            item1 => Assert.Equal(dfeta_ITTResult.InTraining, item1.dfeta_Result));
    }

    [Fact]
    public async Task Given_deferred_non_assessmentonlyeroute_itt_update_UnderAssessment_fails()
    {
        // Arrange
        var (teacherId, ittProviderUkprn, ittId) = await CreatePerson(earlyYears: false, false, hasActiveSanctions: false);
        await _organizationService.ExecuteAsync(new UpdateRequest()
        {
            Target = new dfeta_initialteachertraining()
            {
                Id = ittId,
                dfeta_Result = dfeta_ITTResult.Deferred,
            }
        });

        var ex = await Assert.ThrowsAsync<ArgumentException>(async () => await _dataverseAdapter.UpdateTeacherImpl(new UpdateTeacherCommand()
        {
            TeacherId = teacherId,
            InitialTeacherTraining = new UpdateTeacherCommandInitialTeacherTraining()
            {
                ProviderUkprn = ittProviderUkprn,
                ProgrammeStartDate = new DateOnly(2011, 11, 01),
                ProgrammeEndDate = new DateOnly(2012, 11, 01),
                ProgrammeType = dfeta_ITTProgrammeType.UndergraduateOptIn,
                Subject1 = "100366",  // computer science
                Subject2 = "100403",  // mathematics
                Subject3 = "100302",  // history
                AgeRangeFrom = dfeta_AgeRange._11,
                AgeRangeTo = dfeta_AgeRange._12,
                Result = dfeta_ITTResult.UnderAssessment
            },
            Qualification = new UpdateTeacherCommandQualification()
            {
                CountryCode = "XK",
                Subject = "100366",  // computer science
                Class = dfeta_classdivision.Firstclasshonours,
                Date = new DateOnly(2022, 01, 28),
                ProviderUkprn = ittProviderUkprn,
                Subject2 = "X300",
                Subject3 = "N400"
            }
        }));

        // Assert
        Assert.Contains("UnderAsessment only permitted for AsessmentOnlyRoute", ex.Message);
    }

    [Fact]
    public async Task Given_two_or_more_qualifications_does_not_create_new_qualification_and_creates_warning_crm_task()
    {
        // Arrange
        var (teacherId, ittProviderUkprn, _) = await CreatePerson(earlyYears: true, hasActiveSanctions: false);

        var countryId = await _dataverseAdapter.GetCountry("XK");
        var qualificationSubject1Id = await _dataverseAdapter.GetHeSubjectByCode("100366");  // computer science
        var providerId = (await _dataverseAdapter.GetOrganizationsByUkprn(ittProviderUkprn, columnNames: Array.Empty<string>())).Single();
        var qualification = await _dataverseAdapter.GetHeQualificationByCode("400");  // First Degree
        var HeSubject2Id = await _dataverseAdapter.GetHeSubjectByCode("X300");  // Academic Studies in Education
        var HeSubject3Id = await _dataverseAdapter.GetHeSubjectByCode("N400");  // Accounting

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
                        dfeta_HE_HEQualificationId = new EntityReference(dfeta_hequalification.EntityLogicalName, qualification.Id),
                        dfeta_HE_HESubject2Id = new EntityReference(dfeta_hesubject.EntityLogicalName, HeSubject2Id.Id),
                        dfeta_HE_HESubject3Id = new EntityReference(dfeta_hesubject.EntityLogicalName, HeSubject3Id.Id),
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
                dfeta_qualification.Fields.dfeta_HE_HESubject2Id,
                dfeta_qualification.Fields.dfeta_HE_HESubject3Id,
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
                dfeta_qualification.Fields.dfeta_HE_HESubject2Id,
                dfeta_qualification.Fields.dfeta_HE_HESubject3Id,
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
    [InlineData(dfeta_ITTResult.UnderAssessment)]
    [InlineData(dfeta_ITTResult.Fail)]
    [InlineData(dfeta_ITTResult.Withdrawn)]
    [InlineData(dfeta_ITTResult.Deferred)]
    public async Task Given_two_or_more_itt_assessmentonly_records_for_two_providers_return_error(dfeta_ITTResult res)
    {
        // Arrange
        var (teacherId, ittProviderUkprn, _) = await CreatePerson(earlyYears: false, hasActiveSanctions: false, assessmentOnly: true);
        var providerId = (await _dataverseAdapter.GetOrganizationsByUkprn("10045988", columnNames: Array.Empty<string>())).Single();

        // Create second Itt record
        await _organizationService.ExecuteAsync(new CreateRequest()
        {
            Target = new dfeta_initialteachertraining()
            {
                dfeta_PersonId = new EntityReference(Contact.EntityLogicalName, teacherId),
                dfeta_EstablishmentId = new EntityReference(Account.EntityLogicalName, providerId.Id),
                dfeta_ProgrammeType = dfeta_ITTProgrammeType.AssessmentOnlyRoute,
                dfeta_Result = res,
                dfeta_AgeRangeFrom = dfeta_AgeRange._00,
                dfeta_AgeRangeTo = dfeta_AgeRange._18,
                dfeta_ittqualificationaim = dfeta_ITTQualificationAim.Academicawardonly,
            }
        });

        // Act
        var (result, _) = await _dataverseAdapter.UpdateTeacherImpl(new UpdateTeacherCommand()
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
                Subject2 = "X300", //Academic studies in education
                Subject3 = "N400"  //accounting
            }
        });

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal(UpdateTeacherFailedReasons.MultipleInTrainingIttRecords, result.FailedReasons);
    }

    [Theory]
    [InlineData(dfeta_ITTResult.InTraining)]
    [InlineData(dfeta_ITTResult.Fail)]
    [InlineData(dfeta_ITTResult.Withdrawn)]
    [InlineData(dfeta_ITTResult.Deferred)]
    public async Task Given_two_or_more_itt_earlyyears_records_for_two_providers_return_error(dfeta_ITTResult res)
    {
        // Arrange
        var (teacherId, ittProviderUkprn, _) = await CreatePerson(earlyYears: true, hasActiveSanctions: false);
        var providerId = (await _dataverseAdapter.GetOrganizationsByUkprn("10045988", columnNames: Array.Empty<string>())).Single();

        // Create second Itt record
        await _organizationService.ExecuteAsync(new CreateRequest()
        {
            Target = new dfeta_initialteachertraining()
            {
                dfeta_PersonId = new EntityReference(Contact.EntityLogicalName, teacherId),
                dfeta_EstablishmentId = new EntityReference(Account.EntityLogicalName, providerId.Id),
                dfeta_ProgrammeType = dfeta_ITTProgrammeType.EYITTAssessmentOnly,
                dfeta_Result = res,
                dfeta_AgeRangeFrom = dfeta_AgeRange._00,
                dfeta_AgeRangeTo = dfeta_AgeRange._18,
                dfeta_ittqualificationaim = dfeta_ITTQualificationAim.Academicawardonly,
            }
        });

        // Act
        var (result, _) = await _dataverseAdapter.UpdateTeacherImpl(new UpdateTeacherCommand()
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
                Subject2 = "X300", //Academic studies in education
                Subject3 = "N400"  //accounting
            }
        });

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal(UpdateTeacherFailedReasons.MultipleInTrainingIttRecords, result.FailedReasons);
    }

    [Fact]
    public async Task Given_two_or_more_itt_earlyyears_records_for_same_provider_return_error()
    {
        // Arrange
        var (teacherId, ittProviderUkprn, _) = await CreatePerson(earlyYears: true, hasActiveSanctions: false);

        var providerId = (await _dataverseAdapter.GetOrganizationsByUkprn(ittProviderUkprn, columnNames: Array.Empty<string>())).Single();

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
        var (result, _) = await _dataverseAdapter.UpdateTeacherImpl(new UpdateTeacherCommand()
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
                Subject2 = "X300", //Academic studies in education
                Subject3 = "N400"  //accounting
            }
        });

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal(UpdateTeacherFailedReasons.MultipleInTrainingIttRecords, result.FailedReasons);
    }

    [Fact]
    public void Given_crm_task_is_created_when_no_itt_for_ukprn_description_is_correct()
    {
        // Arrange
        var teacherId = Guid.NewGuid();
        var trn = "1234567";
        var updateCommand = new UpdateTeacherCommand() { Trn = trn, TeacherId = teacherId };
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
        var updateCommand = new UpdateTeacherCommand() { Trn = trn, TeacherId = teacherId };
        var helper = new DataverseAdapter.UpdateTeacherHelper(_dataverseAdapter, updateCommand);

        // Act
        var crmTask = helper.CreateMultipleMatchIttReviewTask();

        // Assert
        Assert.Equal($"Multiple ITT UKPRNs found for TRN {trn}, SlugId ", crmTask.Description);
    }

    [Fact]
    public void Given_crm_task_is_created_when_multiple_itt_for_slugid_for_ukprn_description_is_correct()
    {
        // Arrange
        var slugId = Guid.NewGuid().ToString();
        var teacherId = Guid.NewGuid();
        var trn = "1234567";
        var updateCommand = new UpdateTeacherCommand() { Trn = trn, TeacherId = teacherId, SlugId = slugId };
        var helper = new DataverseAdapter.UpdateTeacherHelper(_dataverseAdapter, updateCommand);

        // Act
        var crmTask = helper.CreateMultipleMatchIttReviewTask();

        // Assert
        Assert.Equal($"Multiple ITT UKPRNs found for TRN {trn}, SlugId {slugId ?? string.Empty}", crmTask.Description);
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
        var updateCommand = new UpdateTeacherCommand() { Qualification = qualification, Trn = trn, TeacherId = teacherId };
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
        var (teacherId, ittProviderUkprn, _) = await CreatePerson(earlyYears: true, hasActiveSanctions: true);

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
                Subject2 = "X300", //Academic studies in education
                Subject3 = "N400"  //accounting
            }
        });

        var oldProvider = (await _dataverseAdapter.GetOrganizationsByUkprn(ittProviderUkprn, columnNames: Array.Empty<string>())).Single().Id;

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
    public async Task Given_contact_with_slugid_exists_but_itt_record_does_not_exist_for_slugid_return_error()
    {
        // Arrange
        var slugId = Guid.NewGuid().ToString();
        var (teacherId, ittProviderUkprn, ittId) = await CreatePerson(earlyYears: false, hasActiveSanctions: false, slugId: slugId);
        await _organizationService.ExecuteAsync(new UpdateRequest()
        {
            Target = new dfeta_initialteachertraining()
            {
                Id = ittId,
                dfeta_SlugId = Guid.NewGuid().ToString(),
            }
        });

        // Act
        var (result, _) = await _dataverseAdapter.UpdateTeacherImpl(new UpdateTeacherCommand()
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
            Qualification = null,
            SlugId = slugId
        });

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal(UpdateTeacherFailedReasons.NoMatchingIttRecord, result.FailedReasons);
    }

    [Fact]
    public async Task Given_contact_slugid_does_not_exist_update_contact_slugid()
    {
        // Arrange
        var slugId = Guid.NewGuid().ToString();
        var (teacherId, ittProviderUkprn, ittId) = await CreatePerson(earlyYears: false, hasActiveSanctions: false);
        await _organizationService.ExecuteAsync(new UpdateRequest()
        {
            Target = new dfeta_initialteachertraining()
            {
                Id = ittId,
                dfeta_SlugId = slugId,
            }
        });

        // Act
        var (result, txnRequest) = await _dataverseAdapter.UpdateTeacherImpl(new UpdateTeacherCommand()
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
            Qualification = null,
            SlugId = slugId
        });

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(UpdateTeacherFailedReasons.None, result.FailedReasons);
        var contact = txnRequest.AssertSingleUpdateRequest<Contact>();
        Assert.Equal(slugId, contact.dfeta_SlugId);
    }

    [Fact]
    public async Task Given_update_with_multiple_itt_records_with_same_slugid_request_fails()
    {
        // Arrange
        var slugId = Guid.NewGuid().ToString();
        var (teacherId, ittProviderUkprn, _) = await CreatePerson(earlyYears: false, hasActiveSanctions: false, slugId: slugId);
        var (_, _, _) = await CreatePerson(earlyYears: false, hasActiveSanctions: false, slugId: slugId);

        // Act
        var (result, _) = await _dataverseAdapter.UpdateTeacherImpl(new UpdateTeacherCommand()
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
            Qualification = null,
            SlugId = slugId
        });

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal(UpdateTeacherFailedReasons.MultipleInTrainingIttRecords, result.FailedReasons);
    }

    [Fact]
    public async Task Given_update_with_unknown_itt_provider_request_fails()
    {
        // Arrange
        var (teacherId, _, _) = await CreatePerson(earlyYears: true, hasActiveSanctions: false);

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
                ProviderUkprn = "SOME INVALID",
                Subject2 = "X300", //Academic studies in education
                Subject3 = "N400"  //accounting
            }
        });

        // Assert
        Assert.False(result.Succeeded);
    }

    [Theory]
    [InlineData("Invalid Subject1", "100403", "100366", "001", "XK", "100366", "400", "N400", "X300", UpdateTeacherFailedReasons.Subject1NotFound)]
    [InlineData("100302", "Invalid subject2", "100403", "001", "XK", "100366", "400", "N400", "X300", UpdateTeacherFailedReasons.Subject2NotFound)]
    [InlineData("100302", "100403", "Invalid subject3", "001", "XK", "100366", "400", "N400", "X300", UpdateTeacherFailedReasons.Subject3NotFound)]
    [InlineData("100302", "100403", "100366", "001", "XK", "Invalid Qualification subject", "400", "N400", "X300", UpdateTeacherFailedReasons.QualificationSubjectNotFound)]
    [InlineData("100302", "100403", "100366", "001", "INVALID COUNTRY CODE", "100366", "400", "N400", "X300", UpdateTeacherFailedReasons.QualificationCountryNotFound)]
    [InlineData("100302", "100403", "100366", "xxx", "XK", "100366", "400", "N400", "X300", UpdateTeacherFailedReasons.IttQualificationNotFound)]
    [InlineData("100302", "100403", "100366", "001", "XK", "100366", "xxx", "N400", "X300", UpdateTeacherFailedReasons.QualificationNotFound)]
    [InlineData("100302", "100403", "100366", "001", "XK", "100366", "400", "NOT VALID SUBJECT2", "X300", UpdateTeacherFailedReasons.QualificationSubject2NotFound)]
    [InlineData("100302", "100403", "100366", "001", "XK", "100366", "400", "N400", "NOT VALID SUBJECT3", UpdateTeacherFailedReasons.QualificationSubject3NotFound)]
    public async Task Given_invalid_reference_data_request_fails(
        string ittSubject1,
        string ittSubject2,
        string ittSubject3,
        string ittQualificationCode,
        string qualificationCountryCode,
        string qualificationSubject,
        string heQualificationCode,
        string qualificationSubject2,
        string qualificationSubject3,
        UpdateTeacherFailedReasons expectedFailedReasons)
    {
        // Arrange
        var (teacherId, _, _) = await CreatePerson(earlyYears: true, hasActiveSanctions: false);

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
                HeQualificationValue = heQualificationCode,
                Subject2 = qualificationSubject2,
                Subject3 = qualificationSubject3,
            }
        });

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal(expectedFailedReasons, result.FailedReasons);
    }

    [Fact]
    public async Task Given_existing_itt_not_present_for_provider_return_error()
    {
        // Arrange
        var (teacherId, ittProviderUkprn, _) = await CreatePerson(earlyYears: true, hasActiveSanctions: false);

        var newIttProviderUkprn = "10045988";
        var updateHeSubjectId = await _dataverseAdapter.GetHeSubjectByCode("100366");  // computer science
        var updatedHeCountryId = await _dataverseAdapter.GetCountry("XQ");
        var updateIttSubject1Id = await _dataverseAdapter.GetIttSubjectByCode("100403");  // mathematics
        var updateIttSubject2Id = await _dataverseAdapter.GetIttSubjectByCode("100366");  // computer science
        var updateIttSubject3Id = await _dataverseAdapter.GetIttSubjectByCode("100302");  // history
        var husId = new Random().NextInt64(2000000000000, 2999999999999).ToString();
        var updateHeSubject2Id = await _dataverseAdapter.GetHeSubjectByCode("X300");  // Academic Studies in Education
        var updateHeSubject3Id = await _dataverseAdapter.GetHeSubjectByCode("N400");  // Accounting

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
                Subject2 = "X300", //Academic studies in education
                Subject3 = "N400"  //accounting
            },
            HusId = husId
        });

        var oldProvider = (await _dataverseAdapter.GetOrganizationsByUkprn(ittProviderUkprn, columnNames: Array.Empty<string>())).Single().Id;
        var newProviderProvider = (await _dataverseAdapter.GetOrganizationsByUkprn(newIttProviderUkprn, columnNames: Array.Empty<string>())).Single().Id;

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
                dfeta_qualification.Fields.dfeta_HE_HESubject2Id,
                dfeta_qualification.Fields.dfeta_HE_HESubject3Id,
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
        Assert.False(result.Succeeded);
        Assert.Equal(UpdateTeacherFailedReasons.NoMatchingIttRecord, result.FailedReasons);
        Assert.Collection(
            itt,
            item1 =>
            {
                Assert.Equal(oldProvider, item1.dfeta_EstablishmentId.Id);
            }
        );
    }

    [Fact]
    public async Task Given_update_without_qualification_returns_success()
    {
        // Arrange
        var (teacherId, ittProviderUkprn, _) = await CreatePerson(earlyYears: false, hasActiveSanctions: false);

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
        var (teacherId, ittProviderUkprn, _) = await CreatePerson(earlyYears: false, hasActiveSanctions: false);
        var husId = new Random().NextInt64(2000000000000, 2999999999999).ToString();

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

    [Fact]
    public async Task Given_update_without_using_slugid_and_changed_ittproviderukprn_retuns_error()
    {
        // Arrange
        var ukprn = "10000571";
        var (teacherId, ittProviderUkprn, ittId) = await CreatePerson(earlyYears: false, hasActiveSanctions: false);
        var husId = new Random().NextInt64(2000000000000, 2999999999999).ToString();

        // Act
        var (result, txnRequest) = await _dataverseAdapter.UpdateTeacherImpl(new UpdateTeacherCommand()
        {
            TeacherId = teacherId,
            InitialTeacherTraining = new UpdateTeacherCommandInitialTeacherTraining()
            {
                ProviderUkprn = ukprn,
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
            HusId = husId,
        });

        // Assert
        Assert.Equal(UpdateTeacherFailedReasons.NoMatchingIttRecord, result.FailedReasons);
    }

    [Fact]
    public async Task Given_update_using_slugid_and_changed_ittproviderukprn_updates_itt_providerukprn()
    {
        // Arrange
        var slugId = Guid.NewGuid().ToString();
        var ukprn = "10000571";
        var (teacherId, ittProviderUkprn, ittId) = await CreatePerson(earlyYears: false, hasActiveSanctions: false, slugId: slugId);
        var husId = new Random().NextInt64(2000000000000, 2999999999999).ToString();
        var ukproviderukprnid = await _dataverseAdapter.GetIttProviderOrganizationsByUkprn(ukprn, Array.Empty<string>(), true);

        // Act
        var (_, txnRequest) = await _dataverseAdapter.UpdateTeacherImpl(new UpdateTeacherCommand()
        {
            TeacherId = teacherId,
            InitialTeacherTraining = new UpdateTeacherCommandInitialTeacherTraining()
            {
                ProviderUkprn = ukprn,
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
            HusId = husId,
            SlugId = slugId
        });

        // Assert
        var ittRecord = txnRequest.AssertSingleUpdateRequest<dfeta_initialteachertraining>();
        Assert.Equal(ittId, ittRecord.Id);
        Assert.Equal(ukproviderukprnid.Single().Id, ittRecord.dfeta_EstablishmentId.Id);
    }

    private async Task<(Guid TeacherId, string IttProviderUkprn, Guid InitialTeacherTrainingId)> CreatePerson(
        bool earlyYears,
        bool assessmentOnly = false,
        bool hasActiveSanctions = false,
        string slugId = null)
    {
        var createPersonResult = await _testDataHelper.CreatePerson(
            earlyYears,
            assessmentOnly,
            withQualification: true,
            withActiveSanction: hasActiveSanctions,
            slugId: slugId);

        return (createPersonResult.TeacherId, createPersonResult.IttProviderUkprn, createPersonResult.InitialTeacherTrainingId);
    }
}
