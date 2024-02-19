using TeachingRecordSystem.Core.Events;

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
    [InlineData(MandatoryQualificationStatus.Failed, null, MandatoryQualificationStatus.Passed, "2021-10-05")]
    [InlineData(MandatoryQualificationStatus.Passed, "2021-10-05", MandatoryQualificationStatus.Failed, null)]
    public async Task QueryExecutesSuccessfully(
        MandatoryQualificationStatus? originalMqStatus,
        string? originalEndDateString,
        MandatoryQualificationStatus newMqStatus,
        string? newEndDateString)
    {
        // Arrange
        DateOnly? originalEndDate = !string.IsNullOrEmpty(originalEndDateString) ? DateOnly.Parse(originalEndDateString) : null;
        DateOnly? newEndDate = !string.IsNullOrEmpty(newEndDateString) ? DateOnly.Parse(newEndDateString) : null;

        var person = await _dataScope.TestData.CreatePerson(x => x
            .WithMandatoryQualification(q => q.WithStatus(originalMqStatus, originalEndDate)));

        var qualification = person.MandatoryQualifications.First();

        // Act
        await _crmQueryDispatcher.ExecuteQuery(
            new UpdateMandatoryQualificationStatusQuery(
                qualification.QualificationId,
                newMqStatus.GetDqtStatus(),
                newEndDate,
                DummyEvent.Create()));

        // Assert
        using var ctx = new DqtCrmServiceContext(_dataScope.OrganizationService);

        var updatedQualification = ctx.dfeta_qualificationSet.SingleOrDefault(c => c.GetAttributeValue<Guid>(dfeta_qualification.PrimaryIdAttribute) == qualification.QualificationId);
        Assert.NotNull(updatedQualification);
        Assert.Equal(newMqStatus, updatedQualification.dfeta_MQ_Status?.ToMandatoryQualificationStatus());
        Assert.Equal(newEndDate.ToDateTimeWithDqtBstFix(isLocalTime: true), updatedQualification.dfeta_MQ_Date);
    }
}
