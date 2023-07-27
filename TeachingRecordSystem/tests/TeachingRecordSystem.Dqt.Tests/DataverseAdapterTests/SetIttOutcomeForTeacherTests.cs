#nullable disable
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;

namespace TeachingRecordSystem.Dqt.Tests.DataverseAdapterTests;

public class SetIttOutcomeForTeacherTests : IAsyncLifetime
{
    private readonly CrmClientFixture.TestDataScope _dataScope;
    private readonly DataverseAdapter _dataverseAdapter;
    private readonly IOrganizationServiceAsync _organizationService;
    private readonly TestDataHelper _testDataHelper;
    private readonly TestableClock _clock;

    public SetIttOutcomeForTeacherTests(CrmClientFixture crmClientFixture)
    {
        _dataScope = crmClientFixture.CreateTestDataScope();
        _dataverseAdapter = _dataScope.CreateDataverseAdapter();
        _organizationService = _dataScope.OrganizationService;
        _testDataHelper = _dataScope.CreateTestDataHelper();
        _clock = crmClientFixture.Clock;
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _dataScope.DisposeAsync();
    [Fact]
    public async Task Given_existing_eyts_update_with_same_assessment_date_returns_success()
    {
        // Arrange
        var createPersonResult = await _testDataHelper.CreatePerson(earlyYears: true);
        var ittResult = dfeta_ITTResult.Pass;
        var assessmentDate = _clock.Today.AddDays(-1);

        // Act
        var (setPassResult, _) = await _dataverseAdapter.SetIttResultForTeacherImpl(
            createPersonResult.TeacherId,
            createPersonResult.IttProviderUkprn,
            ittResult,
            assessmentDate);

        var (result, _) = await _dataverseAdapter.SetIttResultForTeacherImpl(
            createPersonResult.TeacherId,
            createPersonResult.IttProviderUkprn,
            ittResult,
            assessmentDate);

        // Assert
        Assert.True(setPassResult.Succeeded);
        Assert.True(result.Succeeded);
    }


    [Fact]
    public async Task Given_existing_eyts_update_with_different_assessment_date_returns_error()
    {
        // Arrange
        var createPersonResult = await _testDataHelper.CreatePerson(earlyYears: true);
        var ittResult = dfeta_ITTResult.Pass;
        var assessmentDate = _clock.Today.AddDays(-1);
        var amendedAssessmentDate = _clock.Today.AddDays(-5);

        // Act
        var (setPassResult, _) = await _dataverseAdapter.SetIttResultForTeacherImpl(
            createPersonResult.TeacherId,
            createPersonResult.IttProviderUkprn,
            ittResult,
            assessmentDate);

        var (result, _) = await _dataverseAdapter.SetIttResultForTeacherImpl(
            createPersonResult.TeacherId,
            createPersonResult.IttProviderUkprn,
            ittResult,
            amendedAssessmentDate);

        // Assert
        Assert.True(setPassResult.Succeeded);
        Assert.False(result.Succeeded);
        Assert.Equal(SetIttResultForTeacherFailedReason.EytsDateMismatch, result.FailedReason);
    }

    [Fact]
    public async Task Given_existing_qts_update_with_same_assessment_date_returns_success()
    {
        // Arrange
        var createPersonResult = await _testDataHelper.CreatePerson(earlyYears: false, iqts: true);
        var ittResult = dfeta_ITTResult.Pass;
        var assessmentDate = _clock.Today.AddDays(-1);

        // Act
        var (setPassResult, _) = await _dataverseAdapter.SetIttResultForTeacherImpl(
            createPersonResult.TeacherId,
            createPersonResult.IttProviderUkprn,
            ittResult,
            assessmentDate);

        var (result, _) = await _dataverseAdapter.SetIttResultForTeacherImpl(
            createPersonResult.TeacherId,
            createPersonResult.IttProviderUkprn,
            ittResult,
            assessmentDate);

        // Assert
        Assert.True(setPassResult.Succeeded);
        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task Given_existing_qts_update_with_different_assessment_date_returns_error()
    {
        // Arrange
        var createPersonResult = await _testDataHelper.CreatePerson(earlyYears: false, iqts: true);
        var ittResult = dfeta_ITTResult.Pass;
        var assessmentDate = _clock.Today.AddDays(-1);
        var amendedAssessmentDate = _clock.Today.AddDays(-5);

        // Act
        var (setPassResult, _) = await _dataverseAdapter.SetIttResultForTeacherImpl(
            createPersonResult.TeacherId,
            createPersonResult.IttProviderUkprn,
            ittResult,
            assessmentDate);

        var (result, _) = await _dataverseAdapter.SetIttResultForTeacherImpl(
            createPersonResult.TeacherId,
            createPersonResult.IttProviderUkprn,
            ittResult,
            amendedAssessmentDate);

        // Assert
        Assert.True(setPassResult.Succeeded);
        Assert.False(result.Succeeded);
        Assert.Equal(SetIttResultForTeacherFailedReason.QtsDateMismatch, result.FailedReason);
    }

    [Fact]
    public async Task Given_existing_qts_update_assessment_date_with_same_assessmentdate_succeeds()
    {
        // Arrange
        var createPersonResult = await _testDataHelper.CreatePerson(earlyYears: false, iqts: true);
        var ittResult = dfeta_ITTResult.Pass;
        var assessmentDate = _clock.Today.AddDays(-1);

        // Act
        var (setPassResult, _) = await _dataverseAdapter.SetIttResultForTeacherImpl(
            createPersonResult.TeacherId,
            createPersonResult.IttProviderUkprn,
            ittResult,
            assessmentDate);

        var (result, failedReasons) = await _dataverseAdapter.SetIttResultForTeacherImpl(
            createPersonResult.TeacherId,
            createPersonResult.IttProviderUkprn,
            ittResult,
            assessmentDate);

        // Assert
        Assert.True(setPassResult.Succeeded);
        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task Given_valid_request_with_Pass_result_for_early_years_updates_qts_and_does_not_create_induction()
    {
        // Arrange
        var createPersonResult = await _testDataHelper.CreatePerson(earlyYears: true);
        var ittResult = dfeta_ITTResult.Pass;
        var assessmentDate = _clock.Today;

        var earlyYearsTeacherStatusId = (await _dataverseAdapter.GetEarlyYearsStatus("221", null)).Id;

        // Act
        var (result, transactionRequest) = await _dataverseAdapter.SetIttResultForTeacherImpl(
            createPersonResult.TeacherId,
            createPersonResult.IttProviderUkprn,
            ittResult,
            assessmentDate);

        // Assert
        Assert.True(result.Succeeded);

        var ittUpdate = transactionRequest.AssertSingleUpdateRequest<dfeta_initialteachertraining>();
        Assert.Equal(createPersonResult.InitialTeacherTrainingId, ittUpdate.Id);
        Assert.Equal(ittResult, ittUpdate.dfeta_Result);

        var qtsUpdate = transactionRequest.AssertSingleUpdateRequest<dfeta_qtsregistration>();
        Assert.Equal(createPersonResult.QtsRegistrationId, qtsUpdate.Id);
        Assert.Equal(earlyYearsTeacherStatusId, qtsUpdate.dfeta_EarlyYearsStatusId?.Id);
        Assert.Equal(assessmentDate.ToDateTime(), qtsUpdate.dfeta_EYTSDate);
        Assert.Null(qtsUpdate.dfeta_QTSDate);
        Assert.Null(qtsUpdate.dfeta_TeacherStatusId);

        transactionRequest.AssertDoesNotContainUpdateRequest<dfeta_induction>();

        var teacher = await _dataverseAdapter.GetTeacher(createPersonResult.TeacherId, columnNames: new[] { Contact.Fields.dfeta_EYTSDate });
        Assert.Equal(assessmentDate.ToDateTime(), teacher.dfeta_EYTSDate);
    }

    [Theory]
    [InlineData(true, false, "100")]
    [InlineData(false, false, "71")]
    [InlineData(false, true, "90")]
    public async Task Given_valid_request_with_Pass_result_for_qts_updates_qts_and_creates_induction(
        bool assessmentOnly,
        bool iqtsProgrammeType,
        string expectedTeacherStatus)
    {
        // Arrange
        var createPersonResult = await _testDataHelper.CreatePerson(earlyYears: false, assessmentOnly, iqts: iqtsProgrammeType);

        var ittResult = dfeta_ITTResult.Pass;
        var assessmentDate = _clock.Today;
        var teacherStatusId = (await _dataverseAdapter.GetTeacherStatus(expectedTeacherStatus, null)).Id;

        // Act
        var (result, transactionRequest) = await _dataverseAdapter.SetIttResultForTeacherImpl(
            createPersonResult.TeacherId,
            createPersonResult.IttProviderUkprn,
            ittResult,
            assessmentDate);

        // Assert
        Assert.True(result.Succeeded);

        var ittUpdate = transactionRequest.AssertSingleUpdateRequest<dfeta_initialteachertraining>();
        Assert.Equal(createPersonResult.InitialTeacherTrainingId, ittUpdate.Id);
        Assert.Equal(ittResult, ittUpdate.dfeta_Result);

        var qtsUpdate = transactionRequest.AssertSingleUpdateRequest<dfeta_qtsregistration>();
        Assert.Equal(createPersonResult.QtsRegistrationId, qtsUpdate.Id);
        Assert.Null(qtsUpdate.dfeta_EarlyYearsStatusId);
        Assert.Null(qtsUpdate.dfeta_EYTSDate);
        Assert.Equal(assessmentDate.ToDateTime(), qtsUpdate.dfeta_QTSDate);
        Assert.Equal(teacherStatusId, qtsUpdate.dfeta_TeacherStatusId?.Id);

        var induction = transactionRequest.AssertSingleCreateRequest<dfeta_induction>();
        Assert.Equal(createPersonResult.TeacherId, induction.dfeta_PersonId?.Id);
        Assert.Equal(dfeta_InductionStatus.RequiredtoComplete, induction.dfeta_InductionStatus);

        var teacher = await _dataverseAdapter.GetTeacher(createPersonResult.TeacherId, columnNames: new[] { Contact.Fields.dfeta_QTSDate });
        Assert.Equal(assessmentDate.ToDateTime(), teacher.dfeta_QTSDate);
    }

    [Theory]
    [InlineData(dfeta_ITTResult.Fail)]
    [InlineData(dfeta_ITTResult.DeferredforSkillsTests)]
    [InlineData(dfeta_ITTResult.Deferred)]
    public async Task Given_valid_request_with_non_withdrawn_result_for_early_years_does_not_clear_earlyyearsstatusid(dfeta_ITTResult ittResult)
    {
        // Arrange
        var createPersonResult = await _testDataHelper.CreatePerson(earlyYears: true);

        // Act
        var (result, _) = await _dataverseAdapter.SetIttResultForTeacherImpl(
            createPersonResult.TeacherId,
            createPersonResult.IttProviderUkprn,
            ittResult,
            assessmentDate: null);

        var qts = await _dataverseAdapter.GetQtsRegistrationsByTeacher(createPersonResult.TeacherId,
            columnNames: new[]
            {
                dfeta_qtsregistration.Fields.dfeta_EarlyYearsStatusId,
                dfeta_qtsregistration.Fields.dfeta_TeacherStatusId,
                dfeta_qtsregistration.Fields.StateCode
            });

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(qts[0].dfeta_EarlyYearsStatusId?.Id);
    }

    [Theory]
    [InlineData(dfeta_ITTResult.Fail)]
    [InlineData(dfeta_ITTResult.DeferredforSkillsTests)]
    [InlineData(dfeta_ITTResult.Deferred)]
    public async Task Given_valid_request_with_non_withdrawn_result_for_teacher_does_not_clear_teacherstatusid(dfeta_ITTResult ittResult)
    {
        // Arrange
        var createPersonResult = await _testDataHelper.CreatePerson(earlyYears: false);

        // Act
        var (result, _) = await _dataverseAdapter.SetIttResultForTeacherImpl(
            createPersonResult.TeacherId,
            createPersonResult.IttProviderUkprn,
            ittResult,
            assessmentDate: null);

        var qts = await _dataverseAdapter.GetQtsRegistrationsByTeacher(createPersonResult.TeacherId,
            columnNames: new[]
            {
                dfeta_qtsregistration.Fields.dfeta_EarlyYearsStatusId,
                dfeta_qtsregistration.Fields.dfeta_TeacherStatusId,
                dfeta_qtsregistration.Fields.StateCode
            });

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(qts[0].dfeta_TeacherStatusId?.Id);
    }

    [Theory]
    [MemberData(nameof(GetNoMatchingIttRecordsData))]
    public void SelectIttRecord_given_no_itt_records_returns_NoMatchingIttRecord(dfeta_initialteachertraining[] records)
    {
        // Arrange
        var teacherId = SelectIttRecordTestData.TeacherId;
        var ittProviderId = SelectIttRecordTestData.IttProviderId;
        var ittProviderUkprn = SelectIttRecordTestData.IttProviderUkprn;
        var helper = new DataverseAdapter.SetIttResultForTeacherHelper(_dataverseAdapter, teacherId, ittProviderUkprn);

        // Act
        var (record, failedReason) = helper.SelectIttRecord(records, ittProviderId);

        // Assert
        Assert.Null(record);
        Assert.Equal(SetIttResultForTeacherFailedReason.NoMatchingIttRecord, failedReason);
    }

    [Theory]
    [MemberData(nameof(GetMultipleMatchingIttRecordsData))]
    public void SelectIttRecord_given_multiple_matching_itt_records_returns_MultipleMatchingIttRecords(dfeta_initialteachertraining[] records)
    {
        // Arrange
        var teacherId = SelectIttRecordTestData.TeacherId;
        var ittProviderId = SelectIttRecordTestData.IttProviderId;
        var ittProviderUkprn = SelectIttRecordTestData.IttProviderUkprn;
        var helper = new DataverseAdapter.SetIttResultForTeacherHelper(_dataverseAdapter, teacherId, ittProviderUkprn);

        // Act
        var (record, failedReason) = helper.SelectIttRecord(records, ittProviderId);

        // Assert
        Assert.Null(record);
        Assert.Equal(SetIttResultForTeacherFailedReason.MultipleIttRecords, failedReason);
    }

    [Theory]
    [MemberData(nameof(GetSingleMatchingIttRecordData))]
    public void SelectIttRecord_given_single_matching_itt_record_returns_that_record(
        dfeta_initialteachertraining[] records,
        dfeta_initialteachertraining expectedResult)
    {
        // Arrange
        var teacherId = SelectIttRecordTestData.TeacherId;
        var ittProviderId = SelectIttRecordTestData.IttProviderId;
        var ittProviderUkprn = SelectIttRecordTestData.IttProviderUkprn;
        var helper = new DataverseAdapter.SetIttResultForTeacherHelper(_dataverseAdapter, teacherId, ittProviderUkprn);

        // Act
        var (record, failedReason) = helper.SelectIttRecord(records, ittProviderId);

        // Assert
        Assert.Same(expectedResult, record);
        Assert.Null(failedReason);
    }

    [Theory]
    [InlineData(dfeta_ITTResult.Withdrawn, null)]
    [InlineData(dfeta_ITTResult.Deferred, null)]
    [InlineData(dfeta_ITTResult.DeferredforSkillsTests, null)]
    public async Task Given_teacher_is_failed_cannot_set_result_to(dfeta_ITTResult newResult, DateOnly? assessmentDate)
    {
        // Arrange
        var createPersonResult = await _testDataHelper.CreatePerson(earlyYears: true);
        var ittResult = dfeta_ITTResult.Fail;
        var (result, _) = await _dataverseAdapter.SetIttResultForTeacherImpl(
            createPersonResult.TeacherId,
            createPersonResult.IttProviderUkprn,
            ittResult,
            assessmentDate);

        // Act
        var (result2, _) = await _dataverseAdapter.SetIttResultForTeacherImpl(
            createPersonResult.TeacherId,
            createPersonResult.IttProviderUkprn,
            newResult,
            assessmentDate);

        // Assert
        Assert.True(result.Succeeded);
        Assert.False(result2.Succeeded);
        Assert.Equal(SetIttResultForTeacherFailedReason.NoMatchingIttRecord, result2.FailedReason);
    }

    [Fact]
    public async Task Given_teacher_is_failed_cannot_set_result_to_pass()
    {
        // Arrange
        var assessmentDate = _clock.Today;
        var newResult = dfeta_ITTResult.Pass;
        var createPersonResult = await _testDataHelper.CreatePerson(earlyYears: true);
        var ittResult = dfeta_ITTResult.Fail;
        var (result, _) = await _dataverseAdapter.SetIttResultForTeacherImpl(
            createPersonResult.TeacherId,
            createPersonResult.IttProviderUkprn,
            ittResult,
            null);

        // Act
        var (result2, _) = await _dataverseAdapter.SetIttResultForTeacherImpl(
            createPersonResult.TeacherId,
            createPersonResult.IttProviderUkprn,
            newResult,
            assessmentDate);

        // Assert
        Assert.True(result.Succeeded);
        Assert.False(result2.Succeeded);
        Assert.Equal(SetIttResultForTeacherFailedReason.NoMatchingIttRecord, result2.FailedReason);
    }

    [Fact]
    public async Task Given_teacher_is_failed_do_not_error_when_request_result_is_fail()
    {
        // Arrange
        var newResult = dfeta_ITTResult.Fail;
        var createPersonResult = await _testDataHelper.CreatePerson(earlyYears: true);
        var ittResult = dfeta_ITTResult.Fail;
        var (result, _) = await _dataverseAdapter.SetIttResultForTeacherImpl(
            createPersonResult.TeacherId,
            createPersonResult.IttProviderUkprn,
            ittResult,
            null);

        // Act
        var (result2, trxRequest2) = await _dataverseAdapter.SetIttResultForTeacherImpl(
            createPersonResult.TeacherId,
            createPersonResult.IttProviderUkprn,
            newResult,
            null);

        // Assert
        Assert.True(result.Succeeded);
        Assert.True(result2.Succeeded);
        Assert.Null(trxRequest2);
    }

    [Fact]
    public async Task Given_earlyyears_teacher_is_withdrawn_earlyyearsstatusid_is_set_to_null()
    {
        // Arrange
        var createPersonResult = await _testDataHelper.CreatePerson(earlyYears: true);
        var ittResult = dfeta_ITTResult.Withdrawn;

        // Act
        var (result, transactionRequest) = await _dataverseAdapter.SetIttResultForTeacherImpl(
            createPersonResult.TeacherId,
            createPersonResult.IttProviderUkprn,
            ittResult,
            null);

        // Assert
        Assert.True(result.Succeeded);
        var qtsUpdate = transactionRequest.AssertSingleUpdateRequest<dfeta_qtsregistration>();
        Assert.Equal(createPersonResult.QtsRegistrationId, qtsUpdate.Id);
        Assert.Null(qtsUpdate.dfeta_EarlyYearsStatusId?.Id);
    }

    [Fact]
    public async Task Given_teacher_is_withdrawn_teacherstatusid_is_set_to_null()
    {
        // Arrange
        var createPersonResult = await _testDataHelper.CreatePerson(earlyYears: false);
        var ittResult = dfeta_ITTResult.Withdrawn;

        // Act
        var (result, transactionRequest) = await _dataverseAdapter.SetIttResultForTeacherImpl(
            createPersonResult.TeacherId,
            createPersonResult.IttProviderUkprn,
            ittResult,
            null);

        // Assert
        Assert.True(result.Succeeded);
        var qtsUpdate = transactionRequest.AssertSingleUpdateRequest<dfeta_qtsregistration>();
        Assert.Equal(createPersonResult.QtsRegistrationId, qtsUpdate.Id);
        Assert.Null(qtsUpdate.dfeta_TeacherStatusId?.Id);
    }

    [Fact]
    public async Task Given_teacher_with_active_sanctions_creates_review_task()
    {
        // Arrange
        var createPersonResult = await _testDataHelper.CreatePerson(earlyYears: false, withActiveSanction: true);
        var ittResult = dfeta_ITTResult.Pass;
        var assessmentDate = _clock.Today;

        // Act
        var (_, transactionRequest) = await _dataverseAdapter.SetIttResultForTeacherImpl(
            createPersonResult.TeacherId,
            createPersonResult.IttProviderUkprn,
            ittResult,
            assessmentDate);

        // Assert
        var crmTask = transactionRequest.AssertSingleCreateRequest<CrmTask>();
        Assert.Equal("Notification for QTS unit - Register: matched record holds active sanction", crmTask.Category);
        Assert.Equal("Register: active sanction match", crmTask.Subject);
    }

    [Fact]
    public void Given_teacher_creating_review_task_description_is_correct()
    {
        // Arrange
        var teacherId = Guid.NewGuid();
        var ukprn = "1234567";
        var trn = "555555";
        var helper = new DataverseAdapter.SetIttResultForTeacherHelper(_dataverseAdapter, teacherId, ukprn);

        // Act
        var crmTask = helper.CreateReviewTaskEntityForActiveSanctions(trn);

        // Assert
        Assert.Equal($"Active sanction found: TRN {trn}", crmTask.Description);
    }

    [Fact]
    public async Task Given_teacher_without_active_sanctions_does_not_create_review_task()
    {
        // Arrange
        var createPersonResult = await _testDataHelper.CreatePerson(earlyYears: false, withActiveSanction: false);
        var ittResult = dfeta_ITTResult.Pass;
        var assessmentDate = _clock.Today;

        // Act
        var (_, transactionRequest) = await _dataverseAdapter.SetIttResultForTeacherImpl(
            createPersonResult.TeacherId,
            createPersonResult.IttProviderUkprn,
            ittResult,
            assessmentDate);

        // Assert
        transactionRequest.AssertDoesNotContainCreateRequest<CrmTask>();
    }


    [Fact]
    public async Task Given_withdrawn_itt_record_cannot_change_result()
    {
        // Arrange
        var createPersonResult = await _testDataHelper.CreatePerson(earlyYears: false, withActiveSanction: false);
        var ittResult = dfeta_ITTResult.Pass;
        var assessmentDate = _clock.Today;
        var ittId = createPersonResult.InitialTeacherTrainingId;

        await _organizationService.ExecuteAsync(new UpdateRequest()
        {
            Target = new dfeta_initialteachertraining()
            {
                Id = ittId,
                dfeta_Result = dfeta_ITTResult.Withdrawn,
            }
        });

        // Act
        var (result, transactionRequest) = await _dataverseAdapter.SetIttResultForTeacherImpl(
            createPersonResult.TeacherId,
            createPersonResult.IttProviderUkprn,
            ittResult,
            assessmentDate);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal(SetIttResultForTeacherFailedReason.NoMatchingIttRecord, result.FailedReason);
    }

    [Fact]
    public async Task Given_itt_record_with_slugid_can_be_updated_using_slugid()
    {
        // Arrange
        var slugId = Guid.NewGuid().ToString();
        var createPersonResult = await _testDataHelper.CreatePerson(earlyYears: false, withActiveSanction: false, slugId: slugId);
        var ittResult = dfeta_ITTResult.Pass;
        var assessmentDate = _clock.Today;
        var ittId = createPersonResult.InitialTeacherTrainingId;

        // Act
        var (result, _) = await _dataverseAdapter.SetIttResultForTeacherImpl(
            createPersonResult.TeacherId,
            createPersonResult.IttProviderUkprn,
            ittResult,
            assessmentDate,
            slugId);

        // Assert
        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task Given_itt_does_not_exist_for_slugid_fallback_to_establishment_matching_return_success()
    {
        // Arrange
        var slugId = Guid.NewGuid().ToString();
        var createPersonResult = await _testDataHelper.CreatePerson(earlyYears: false, withActiveSanction: false, slugId: slugId);
        var ittResult = dfeta_ITTResult.Pass;
        var assessmentDate = _clock.Today;
        var ittId = createPersonResult.InitialTeacherTrainingId;

        await _organizationService.ExecuteAsync(new UpdateRequest()
        {
            Target = new dfeta_initialteachertraining()
            {
                Id = ittId,
                dfeta_SlugId = string.Empty,
            }
        });

        // Act
        var (result, _) = await _dataverseAdapter.SetIttResultForTeacherImpl(
            createPersonResult.TeacherId,
            createPersonResult.IttProviderUkprn,
            ittResult,
            assessmentDate,
            slugId);

        // Assert
        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task Given_itt_matches_on_slugid_but_not_establishmentid_return_error()
    {
        // Arrange
        var slugId = Guid.NewGuid().ToString();
        var createPersonResult = await _testDataHelper.CreatePerson(earlyYears: false, withActiveSanction: false, slugId: slugId);
        var ittResult = dfeta_ITTResult.Pass;
        var assessmentDate = _clock.Today;
        var ittId = createPersonResult.InitialTeacherTrainingId;
        var AccountId2 = await _organizationService.CreateAsync(new Account()
        {
            Name = "Testing"
        });

        await _organizationService.ExecuteAsync(new UpdateRequest()
        {
            Target = new dfeta_initialteachertraining()
            {
                Id = ittId,
                dfeta_EstablishmentId = new EntityReference(Account.EntityLogicalName, AccountId2)
            }
        });

        // Act
        var (result, _) = await _dataverseAdapter.SetIttResultForTeacherImpl(
            createPersonResult.TeacherId,
            createPersonResult.IttProviderUkprn,
            ittResult,
            assessmentDate,
            slugId);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal(SetIttResultForTeacherFailedReason.NoMatchingIttRecord, result.FailedReason);
    }

    public static class SelectIttRecordTestData
    {
        public static readonly Guid TeacherId = Guid.NewGuid();
        public static readonly Guid IttProviderId = Guid.NewGuid();
        public static readonly string IttProviderUkprn = "10001234";

        public static readonly dfeta_initialteachertraining DifferentProviderResult = new()
        {
            dfeta_PersonId = new EntityReference(Contact.EntityLogicalName, TeacherId),
            dfeta_EstablishmentId = new EntityReference(Account.EntityLogicalName, Guid.NewGuid()),
            dfeta_ProgrammeType = dfeta_ITTProgrammeType.RegisteredTeacherProgramme,
            dfeta_Result = dfeta_ITTResult.InTraining,
            StateCode = dfeta_initialteachertrainingState.Active
        };

        public static readonly dfeta_initialteachertraining InactiveResult = new()
        {
            dfeta_PersonId = new EntityReference(Contact.EntityLogicalName, TeacherId),
            dfeta_EstablishmentId = new EntityReference(Account.EntityLogicalName, IttProviderId),
            dfeta_ProgrammeType = dfeta_ITTProgrammeType.RegisteredTeacherProgramme,
            dfeta_Result = dfeta_ITTResult.InTraining,
            StateCode = dfeta_initialteachertrainingState.Inactive
        };

        public static readonly dfeta_initialteachertraining WithdrawnResult = new()
        {
            dfeta_PersonId = new EntityReference(Contact.EntityLogicalName, TeacherId),
            dfeta_EstablishmentId = new EntityReference(Account.EntityLogicalName, IttProviderId),
            dfeta_ProgrammeType = dfeta_ITTProgrammeType.RegisteredTeacherProgramme,
            dfeta_Result = dfeta_ITTResult.Withdrawn,
            StateCode = dfeta_initialteachertrainingState.Active
        };

        public static readonly dfeta_initialteachertraining UnderAssessmentWithNonAssessmentOnlyProgrammeTypeResult = new()
        {
            dfeta_PersonId = new EntityReference(Contact.EntityLogicalName, TeacherId),
            dfeta_EstablishmentId = new EntityReference(Account.EntityLogicalName, IttProviderId),
            dfeta_ProgrammeType = dfeta_ITTProgrammeType.RegisteredTeacherProgramme,
            dfeta_Result = dfeta_ITTResult.UnderAssessment,
            StateCode = dfeta_initialteachertrainingState.Active
        };

        public static readonly dfeta_initialteachertraining ValidMatchWithInTrainingResult = new()
        {
            dfeta_PersonId = new EntityReference(Contact.EntityLogicalName, TeacherId),
            dfeta_EstablishmentId = new EntityReference(Account.EntityLogicalName, IttProviderId),
            dfeta_ProgrammeType = dfeta_ITTProgrammeType.RegisteredTeacherProgramme,
            dfeta_Result = dfeta_ITTResult.InTraining,
            StateCode = dfeta_initialteachertrainingState.Active
        };

        public static readonly dfeta_initialteachertraining ValidMatchWithUnderAssessmentResult = new()
        {
            dfeta_PersonId = new EntityReference(Contact.EntityLogicalName, TeacherId),
            dfeta_EstablishmentId = new EntityReference(Account.EntityLogicalName, IttProviderId),
            dfeta_ProgrammeType = dfeta_ITTProgrammeType.AssessmentOnlyRoute,
            dfeta_Result = dfeta_ITTResult.UnderAssessment,
            StateCode = dfeta_initialteachertrainingState.Active
        };
    }

    public static TheoryData<dfeta_initialteachertraining[]> GetNoMatchingIttRecordsData() => new()
    {
        // No ITT records for teacher at all
        Array.Empty<dfeta_initialteachertraining>(),

        // Set of ITT records including one for another provider, one non-active, one with a non-InTraining result
        new[]
        {
            SelectIttRecordTestData.DifferentProviderResult,
            SelectIttRecordTestData.InactiveResult
        },

        // UnderAssessment record where the programme type is not 'AssessmentOnlyRoute'
        new[]
        {
            SelectIttRecordTestData.UnderAssessmentWithNonAssessmentOnlyProgrammeTypeResult
        }
    };

    public static TheoryData<dfeta_initialteachertraining[]> GetMultipleMatchingIttRecordsData() => new()
    {
        new[]
        {
            SelectIttRecordTestData.ValidMatchWithInTrainingResult,
            SelectIttRecordTestData.ValidMatchWithUnderAssessmentResult
        }
    };

    public static TheoryData<dfeta_initialteachertraining[], dfeta_initialteachertraining> GetSingleMatchingIttRecordData() => new()
    {
        // One record for specified provider at InTraining
        {
            new[] { SelectIttRecordTestData.ValidMatchWithInTrainingResult },
            SelectIttRecordTestData.ValidMatchWithInTrainingResult
        },

        // One record for specified provider at UnderAssessment with AssessmentOnlyRoute programme type
        {
            new[] { SelectIttRecordTestData.ValidMatchWithUnderAssessmentResult },
            SelectIttRecordTestData.ValidMatchWithUnderAssessmentResult
        }
    };
}
