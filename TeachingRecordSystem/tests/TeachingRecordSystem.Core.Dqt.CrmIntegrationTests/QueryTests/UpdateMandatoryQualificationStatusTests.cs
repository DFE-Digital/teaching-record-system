namespace TeachingRecordSystem.Core.Dqt.CrmIntegrationTests.QueryTests;

public class UpdateMandatoryQualificationStatusTests : IAsyncLifetime
{
    private readonly CrmClientFixture.TestDataScope _dataScope;
    private readonly CrmQueryDispatcher _crmQueryDispatcher;

    public UpdateMandatoryQualificationStatusTests(CrmClientFixture crmClientFixture)
    {
        _dataScope = crmClientFixture.CreateTestDataScope();
        _crmQueryDispatcher = crmClientFixture.CreateQueryDispatcher();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _dataScope.DisposeAsync();

    [Theory]
    [InlineData(dfeta_qualification_dfeta_MQ_Status.Failed, null, dfeta_qualification_dfeta_MQ_Status.Passed, "2021-10-05")]
    [InlineData(dfeta_qualification_dfeta_MQ_Status.Passed, "2021-10-05", dfeta_qualification_dfeta_MQ_Status.Failed, null)]
    public async Task QueryExecutesSuccessfully(dfeta_qualification_dfeta_MQ_Status? originalMqStatus, string? originalEndDateString, dfeta_qualification_dfeta_MQ_Status newMqStatus, string? newEndDateString)
    {
        // Arrange
        DateOnly? originalEndDate = !string.IsNullOrEmpty(originalEndDateString) ? DateOnly.Parse(originalEndDateString) : null;
        DateOnly? newEndDate = !string.IsNullOrEmpty(newEndDateString) ? DateOnly.Parse(newEndDateString) : null;

        var person = await _dataScope.TestData.CreatePerson(
            x => x.WithQts(qtsDate: new DateOnly(2021, 10, 5))
                .WithMandatoryQualification(result: originalMqStatus, endDate: originalEndDate));

        var qualification = person.MandatoryQualifications.First();

        // Act
        await _crmQueryDispatcher.ExecuteQuery(
            new UpdateMandatoryQualificationStatusQuery(
                qualification.QualificationId,
                newMqStatus,
                newEndDate));

        // Assert
        using var ctx = new DqtCrmServiceContext(_dataScope.OrganizationService);

        var updatedQualification = ctx.dfeta_qualificationSet.SingleOrDefault(c => c.GetAttributeValue<Guid>(dfeta_qualification.PrimaryIdAttribute) == qualification.QualificationId);
        Assert.NotNull(updatedQualification);
        Assert.Equal(newMqStatus, updatedQualification.dfeta_MQ_Status);
        Assert.Equal(newEndDate.ToDateTimeWithDqtBstFix(isLocalTime: true), updatedQualification.dfeta_MQ_Date);
    }
}
