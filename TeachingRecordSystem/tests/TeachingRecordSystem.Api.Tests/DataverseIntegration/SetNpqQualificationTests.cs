#nullable disable
using Microsoft.PowerPlatform.Dataverse.Client;
using TeachingRecordSystem.Api.DataStore.Crm;
using TeachingRecordSystem.Api.DataStore.Crm.Models;
using Xunit;


namespace TeachingRecordSystem.Api.Tests.DataverseIntegration;

public class SetNpqQualificationTests : IAsyncLifetime
{
    private readonly CrmClientFixture.TestDataScope _dataScope;
    private readonly DataverseAdapter _dataverseAdapter;
    private readonly IOrganizationServiceAsync _organizationService;
    private readonly TestDataHelper _testDataHelper;

    public SetNpqQualificationTests(CrmClientFixture crmClientFixture)
    {
        _dataScope = crmClientFixture.CreateTestDataScope();
        _dataverseAdapter = _dataScope.CreateDataverseAdapter();
        _organizationService = _dataScope.OrganizationService;
        _testDataHelper = _dataScope.CreateTestDataHelper();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _dataScope.DisposeAsync();

    [Theory]
    [InlineData(dfeta_qualification_dfeta_Type.NPQLL)]
    [InlineData(dfeta_qualification_dfeta_Type.NPQEYL)]
    [InlineData(dfeta_qualification_dfeta_Type.NPQEL)]
    [InlineData(dfeta_qualification_dfeta_Type.NPQSL)]
    [InlineData(dfeta_qualification_dfeta_Type.NPQH)]
    [InlineData(dfeta_qualification_dfeta_Type.NPQLT)]
    [InlineData(dfeta_qualification_dfeta_Type.NPQLTD)]
    [InlineData(dfeta_qualification_dfeta_Type.NPQLBC)]
    public async Task Given_existing_api_created_npq_qualification_clearing_completiondate_returns_success(dfeta_qualification_dfeta_Type type)
    {
        // Arrange
        var (teacherId, _) = await CreatePerson(false, false);
        var (_, qualificationId, _) = await CreateQualification(teacherId, type, true);

        // Act
        var (result, transactionRequest) = await _dataverseAdapter.SetNpqQualificationImpl(new SetNpqQualificationCommand()
        {
            TeacherId = teacherId,
            CompletionDate = null,
            QualificationType = type
        });

        // Assert
        var updatedqualification = transactionRequest.AssertSingleUpdateRequest<dfeta_qualification>();
        Assert.Equal(qualificationId, updatedqualification.Id);
        Assert.Null(updatedqualification.dfeta_CompletionorAwardDate);
        Assert.True(result.Succeeded);
    }

    [Theory]
    [InlineData(dfeta_qualification_dfeta_Type.NPQLL)]
    [InlineData(dfeta_qualification_dfeta_Type.NPQEYL)]
    [InlineData(dfeta_qualification_dfeta_Type.NPQEL)]
    [InlineData(dfeta_qualification_dfeta_Type.NPQSL)]
    [InlineData(dfeta_qualification_dfeta_Type.NPQH)]
    [InlineData(dfeta_qualification_dfeta_Type.NPQLT)]
    [InlineData(dfeta_qualification_dfeta_Type.NPQLTD)]
    [InlineData(dfeta_qualification_dfeta_Type.NPQLBC)]
    public async Task Given_create_new_npq_qualification_returns_success(dfeta_qualification_dfeta_Type type)
    {
        // Arrange
        var (teacherId, _) = await CreatePerson(false, false);
        await CreateQualification(teacherId, type, false);

        // Act
        var (result, transactionRequest) = await _dataverseAdapter.SetNpqQualificationImpl(new SetNpqQualificationCommand()
        {
            TeacherId = teacherId,
            CompletionDate = DateTime.Now.AddDays(-100),
            QualificationType = type
        });

        // Assert
        var createqualification = transactionRequest.AssertSingleCreateRequest<dfeta_qualification>();
        Assert.True(createqualification.dfeta_createdbyapi);
        Assert.True(result.Succeeded);
    }

    [Theory]
    [InlineData(dfeta_qualification_dfeta_Type.NPQLL)]
    [InlineData(dfeta_qualification_dfeta_Type.NPQEYL)]
    [InlineData(dfeta_qualification_dfeta_Type.NPQEL)]
    [InlineData(dfeta_qualification_dfeta_Type.NPQSL)]
    [InlineData(dfeta_qualification_dfeta_Type.NPQH)]
    [InlineData(dfeta_qualification_dfeta_Type.NPQLT)]
    [InlineData(dfeta_qualification_dfeta_Type.NPQLTD)]
    [InlineData(dfeta_qualification_dfeta_Type.NPQLBC)]
    public async Task Given_existing_api_and_none_api_created_npq_qualification_update_only_apicreated_npq_returns_success(dfeta_qualification_dfeta_Type type)
    {
        // Arrange
        var (teacherId, _) = await CreatePerson(false, false);
        await CreateQualification(teacherId, type, false);
        var (_, apiqualificationid, _) = await CreateQualification(teacherId, type, true);

        // Act
        var (result, transactionRequest) = await _dataverseAdapter.SetNpqQualificationImpl(new SetNpqQualificationCommand()
        {
            TeacherId = teacherId,
            CompletionDate = DateTime.Now.AddDays(-100),
            QualificationType = type
        });

        // Assert
        var updatedqualification = transactionRequest.AssertSingleUpdateRequest<dfeta_qualification>();
        Assert.Equal(apiqualificationid, updatedqualification.Id);
        Assert.Equal(SetNpqQualificationFailedReasons.None, result.FailedReasons);
    }

    [Fact]

    public async Task Given_attempting_to_remove_existing_npq_qualification_completeddate_not_created_by_api_returns_error()
    {
        // Arrange
        var type = dfeta_qualification_dfeta_Type.NPQLT;
        var (teacherId, _) = await CreatePerson(false, false);
        await CreateQualification(teacherId, type, false);

        // Act
        var (result, transactionRequest) = await _dataverseAdapter.SetNpqQualificationImpl(new SetNpqQualificationCommand()
        {
            TeacherId = teacherId,
            CompletionDate = null,
            QualificationType = type
        });

        // Assert
        Assert.Null(transactionRequest);
        Assert.Equal(SetNpqQualificationFailedReasons.NpqQualificationNotCreatedByApi, result.FailedReasons);
    }


    [Theory]
    [InlineData(dfeta_qualification_dfeta_Type.NPQLL)]
    [InlineData(dfeta_qualification_dfeta_Type.NPQEYL)]
    [InlineData(dfeta_qualification_dfeta_Type.NPQEL)]
    [InlineData(dfeta_qualification_dfeta_Type.NPQSL)]
    [InlineData(dfeta_qualification_dfeta_Type.NPQH)]
    [InlineData(dfeta_qualification_dfeta_Type.NPQLT)]
    [InlineData(dfeta_qualification_dfeta_Type.NPQLTD)]
    [InlineData(dfeta_qualification_dfeta_Type.NPQLBC)]
    public async Task Given_updating_existing_api_created_qualification_with_more_than_one_matching_qualificationtype_returns_error(dfeta_qualification_dfeta_Type type)
    {
        // Arrange
        var (teacherId, _) = await CreatePerson(false, false);
        await CreateQualification(teacherId, type, true);
        await CreateQualification(teacherId, type, true);

        // Act
        var (result, transactionRequest) = await _dataverseAdapter.SetNpqQualificationImpl(new SetNpqQualificationCommand()
        {
            TeacherId = teacherId,
            CompletionDate = DateTime.Now.AddDays(-100),
            QualificationType = type
        });

        // Assert

        Assert.False(result.Succeeded);
        Assert.Null(transactionRequest);
        Assert.Equal(SetNpqQualificationFailedReasons.MultipleNpqQualificationsWithQualificationType, result.FailedReasons);
    }

    [Theory]
    [InlineData(dfeta_qualification_dfeta_Type.NPQLL)]
    [InlineData(dfeta_qualification_dfeta_Type.NPQEYL)]
    [InlineData(dfeta_qualification_dfeta_Type.NPQEL)]
    [InlineData(dfeta_qualification_dfeta_Type.NPQSL)]
    [InlineData(dfeta_qualification_dfeta_Type.NPQH)]
    [InlineData(dfeta_qualification_dfeta_Type.NPQLT)]
    [InlineData(dfeta_qualification_dfeta_Type.NPQLTD)]
    [InlineData(dfeta_qualification_dfeta_Type.NPQLBC)]
    public async Task Given_creating_new_npq_qualification_for_teacher_with_activesanctions_creates_reviewtask(dfeta_qualification_dfeta_Type type)
    {
        // Arrange
        var (teacherId, _) = await CreatePerson(false, false, true);

        // Act
        var (result, transactionRequest) = await _dataverseAdapter.SetNpqQualificationImpl(new SetNpqQualificationCommand()
        {
            TeacherId = teacherId,
            CompletionDate = DateTime.Now.AddDays(-100),
            QualificationType = type
        });

        // Assert

        Assert.True(result.Succeeded);
        transactionRequest.AssertSingleCreateRequest<dfeta_qualification>();
        transactionRequest.AssertSingleCreateRequest<CrmTask>();
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

    private async Task<(Guid TeacherId, Guid QualificationId, dfeta_qualification_dfeta_Type QualificationType)> CreateQualification(
        Guid TeacherId,
        dfeta_qualification_dfeta_Type qualificationType,
        bool createdByApi = true)
    {
        var createQualification = await _testDataHelper.CreateQualification(TeacherId,
            qualificationType, createdByApi);

        return (createQualification.TeacherId, createQualification.QualificationId, qualificationType);
    }
}
