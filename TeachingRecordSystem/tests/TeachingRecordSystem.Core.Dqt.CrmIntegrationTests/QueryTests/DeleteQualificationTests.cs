using TeachingRecordSystem.Core.Events;

namespace TeachingRecordSystem.Core.Dqt.CrmIntegrationTests.QueryTests;

public class DeleteQualificationTests : IAsyncLifetime
{
    private readonly CrmClientFixture.TestDataScope _dataScope;
    private readonly CrmQueryDispatcher _crmQueryDispatcher;

    public DeleteQualificationTests(CrmClientFixture crmClientFixture)
    {
        _dataScope = crmClientFixture.CreateTestDataScope();
        _crmQueryDispatcher = crmClientFixture.CreateQueryDispatcher();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _dataScope.DisposeAsync();

    [Fact]
    public async Task QueryExecutesSuccessfully()
    {
        // Arrange
        var person = await _dataScope.TestData.CreatePerson(b => b.WithMandatoryQualification());
        var qualification = person.MandatoryQualifications.Single();

        // Act
        await _crmQueryDispatcher.ExecuteQuery(new DeleteQualificationQuery(qualification.QualificationId, DummyEvent.Create()));

        // Assert
        using var ctx = new DqtCrmServiceContext(_dataScope.OrganizationService);
        var updatedQualification = ctx.dfeta_qualificationSet.SingleOrDefault(q => q.GetAttributeValue<Guid>(dfeta_qualification.PrimaryIdAttribute) == qualification.QualificationId);
        Assert.NotNull(updatedQualification);
        Assert.Equal(dfeta_qualificationState.Inactive, updatedQualification.StateCode);
    }
}
